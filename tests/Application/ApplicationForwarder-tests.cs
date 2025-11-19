using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using Blackwood.WinForms;

namespace Blackwood.Core.Tests;

/// <summary>
/// Tests for <see cref="ApplicationForwarder"/>.
/// </summary>
[TestFixture]
public class ApplicationForwarderTests
{
    static string CreatePipeName() => $"bw-single-instance-test-{Guid.NewGuid():N}";

    /// <summary>
    /// Verifies that the forwarder listener receives payloads sent via a client stream.
    /// </summary>
    [Test]
    public async Task Listener_WithIncomingPayload_DeliversArguments()
    {
        // Arrange
        var pipeName = CreatePipeName();
        var tcs = new TaskCompletionSource<string[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var forwarder = new ApplicationForwarder(pipeName, args =>
        {
            if (args is { Length: > 0 })
                tcs.TrySetResult(args);
        });

        await Task.Delay(200);

        var files = new[] { "file1.txt", "file2.txt" };

        // Act - connect with a client and send payload
        await Task.Run(async () =>
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            await client.ConnectAsync(2000);
            var payload = JsonSerializer.SerializeToUtf8Bytes(files, JSONDeserializer.JSONOptions);
            var lengthPrefix = BitConverter.GetBytes(payload.Length);
            await client.WriteAsync(lengthPrefix.AsMemory());
            await client.WriteAsync(payload.AsMemory());
            await client.FlushAsync();
        });

        var received = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.That(received, Is.EqualTo(files));
    }

    /// <summary>
    /// Ensures <see cref="ApplicationForwarder.ForwardFilesAsync"/> sends the serialized payload.
    /// </summary>
    [Test]
    public async Task ForwardFilesAsync_SendsSerializedArguments()
    {
        var pipeName = CreatePipeName();
        var files = new[] { "foo.txt", "bar.txt" };
        var received = new TaskCompletionSource<string[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var serverTask = Task.Run(async () =>
        {
            using var server = new NamedPipeServerStream(pipeName, PipeDirection.In);
            ready.TrySetResult();
            await server.WaitForConnectionAsync();
            var lengthBuffer = new byte[sizeof(int)];
            await PipeTestHelpers.ReadExactAsync(server, lengthBuffer, lengthBuffer.Length);
            var payloadLength = BitConverter.ToInt32(lengthBuffer, 0);
            var payloadBuffer = new byte[payloadLength];
            await PipeTestHelpers.ReadExactAsync(server, payloadBuffer, payloadBuffer.Length);
            var args = JsonSerializer.Deserialize<string[]>(payloadBuffer, JSONDeserializer.JSONOptions) ?? Array.Empty<string>();
            received.TrySetResult(args);
        });

        await ready.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var sendResult = await ApplicationForwarder.ForwardFilesAsync(pipeName, files);
        Assert.That(sendResult, Is.True);
        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(result, Is.EqualTo(files));
        await serverTask;
    }

    /// <summary>
    /// Validates that calling <see cref="ApplicationForwarder.Dispose"/> multiple times
    /// does not throw, mirroring typical .NET dispose patterns.
    /// </summary>
    [Test]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var pipeName = CreatePipeName();
        var forwarder = new ApplicationForwarder(pipeName, _ => { });

        Assert.DoesNotThrow(() =>
        {
            forwarder.Dispose();
            forwarder.Dispose();
        });
    }
}

static class PipeTestHelpers
{
    public static async Task ReadExactAsync(Stream stream, byte[] buffer, int count)
    {
        var offset = 0;
        while (offset < count)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, count - offset));
            if (read == 0)
                throw new EndOfStreamException("Pipe closed unexpectedly.");
            offset += read;
        }
    }
}

