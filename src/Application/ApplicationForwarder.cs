// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.IO.Pipes;
using System.Text.Json;
namespace Blackwood;

/// <summary>
/// This is used to forward file-open and other requests to the "main instance"
/// of a single-instance application; it is used by a second instance of the
/// application.
/// </summary>
/// <typeparam name="messageType">The type of message to forward.</typeparam>
/// <remarks>
/// To send application arguments, such as file paths to open, use the
/// <see cref="ForwardFilesAsync"/> method.
///
/// To receive application arguments, such as file paths to open, use the
/// <see cref="ApplicationForwarder{messageType}"/> constructor.  This will
/// create a listener that will call the action when the message is received.
///
/// <code>
/// using (ApplicationForwarder&lt;string[]&gt; forwarder = new (message =>
/// {
///     // Handle the files
/// }))
/// </code>
///
/// To send a message, use the <see cref="ForwardFilesAsync"/> method.
/// <code>
/// await ApplicationForwarder&lt;string[]&gt;.ForwardFilesAsync(pipeName, message);
/// </code>
///
/// The messages sent over the pipe are formattted as:
/// - a 4-byte length prefix (int32)
/// - the JSON payload (array of bytes)
/// This allows the receiver to know how much data to read first,
/// before reading the payload.  This allows sending large payloads
/// as well as payloads that contain control characters.
///
/// The sender and receiver can use a timeout to stop waiting for a response.
///
/// The sender can use a cancellation token to stop sending a message.
/// This cancellation token is passed to the <see cref="ForwardFilesAsync"/>
/// method.
///
/// The receiver uses a cancellation token to stop listening for incoming
/// messages.  This cancellation token is invoked when the <see cref="Dispose"/>
/// method is called.
/// </remarks>
public sealed class ApplicationForwarder<messageType> : IDisposable
{
    #region Variables
    /// <summary>
    /// The name of the pipe to use for communication.
    /// </summary>
    readonly string pipeName_;

    /// <summary>
    /// The cancellation token source.
    /// </summary>
    /// <remarks>
    /// This is used to stop the pipe server when the application is exiting.
    /// </remarks>
    CancellationTokenSource? cts_ = new();

    /// <summary>
    /// The task that listens for incoming file-open requests.
    /// </summary>
    readonly Task listenerTask_;

    /// <summary>
    /// The action to call when arguments (e.g. files) are received.
    /// </summary>
    readonly Action<messageType> onMessage_;
    #endregion

    #region Constructor / Destructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationForwarder{messageType}"/> class.
    /// </summary>
    /// <param name="pipeName">The name of the pipe to use for communication.</param>
    /// <param name="onArguments">The action to call when arguments are received.</param>
    public ApplicationForwarder(string pipeName, Action<messageType> onMessage)
    {
        pipeName_     = pipeName;
        onMessage_    = onMessage;
        listenerTask_ = Task.Run(ListenAsync);
    }


    /// <summary>
    /// Dispose of any internal resources in the <see cref="ApplicationForwarder{messageType}"/>
    /// instance.
    /// </summary>
    public void Dispose()
    {
        var cts = cts_;
        cts_ = null;
        if (null == cts)
            return;

        // Cancel the pipe server using the cancellation token
        cts.Cancel();

        // Try to wait for the listener task to complete
        try
        {
            listenerTask_.Wait(500);
        }
        catch
        {
            // ignored
        }
        finally
        {
            cts.Dispose();
        }

        // Suppress finalization.
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Static procedures that can be used
    /// <summary>
    /// Sends the provided command arguments (e.g. file paths) to the already running instance.
    /// </summary>
    /// <param name="pipeName">The name of the pipe to use for communication.</param>
    /// <param name="arguments">The arguments to send.</param>
    /// <param name="timeoutMs">The timeout in milliseconds.</param>
    /// <param name="PipeName">The name of the pipe to use for communication.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task representing the asynchronous operation,
    /// returning <c>true</c> if the arguments were sent successfully.
    /// </returns>
    /// <remarks>
    /// Typically, the sender will call this method, awaiting the result, and
    /// then exit the application.
    ///
    /// The sender can use a cancellation token to stop sending a message.
    /// This cancellation token is passed to the <see cref="ForwardFilesAsync"/>
    /// method.  If the cancellation token is signaled, the method will return
    /// <c>false</c>.
    /// </remarks>
    public static async Task<bool> ForwardFilesAsync( string PipeName
                                                    , messageType message
                                                    , int timeoutMs = 1000
                                                    , CancellationToken cancellationToken = default
                                                    )
    {
        try
        {
            // Create a new named pipe client stream
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            await client.ConnectAsync(timeoutMs, cancellationToken).ConfigureAwait(false);

            // The messages sent over the pipe are formattted as:
            // - a 4-byte length prefix (int32)
            // - the JSON payload (array of bytes)
            // This allows the receiver to know how much data to read first,
            // before reading the payload.

            // Serialize the arguments to a JSON payload and convert to an array of bytes.
            var payload = JsonSerializer.SerializeToUtf8Bytes(message, JSONDeserializer.JSONOptions);
            // Write the length prefix and the JSON payload to the pipe.
            await client.WriteAsync(BitConverter.GetBytes(payload.Length).Concat(payload).ToArray().AsMemory(), cancellationToken).ConfigureAwait(false);
            // Flush the pipe to ensure the data is sent.
            await client.FlushAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            // If the pipe isn't available or something else fails, swallow it.
            return false;
        }
    }
    #endregion

    #region Internal communciation methods
    /// <summary>
    /// Listens for incoming file-open requests and forwards them to the main instance.
    /// </summary>
    private async Task ListenAsync()
    {
        while (null != cts_ && !cts_.IsCancellationRequested)
        {
            try
            {
                // Create a new named pipe server to receive the file paths to open
                using NamedPipeServerStream server = new (
                    pipeName_,
                    PipeDirection.In,
                    1,
#if WINDOWS
                    PipeTransmissionMode.Message,
#else
                    PipeTransmissionMode.Byte,
#endif
                    PipeOptions.Asynchronous);

                // Wait for a connection to the pipe
                await server.WaitForConnectionAsync(cts_.Token).ConfigureAwait(false);


                // Read the length prefix (4 bytes)
                var lengthBuffer = new byte[sizeof(int)];
                await ReadExactlyAsync(server, lengthBuffer, lengthBuffer.Length, cts_.Token).ConfigureAwait(false);
                var payloadLength = BitConverter.ToInt32(lengthBuffer, 0);
                // If the payload length is less than or equal to 0, continue listening
                if (payloadLength <= 0)
                    continue;

                // Read the full payload
                var payloadBuffer = new byte[payloadLength];
                await ReadExactlyAsync(server, payloadBuffer, payloadBuffer.Length, cts_.Token).ConfigureAwait(false);

                var message = JsonSerializer.Deserialize<messageType>(payloadBuffer, JSONDeserializer.JSONOptions);
                if (null != message)
                {
                    // Call the action to handle the message
                    onMessage_(message);
                }
            }
            catch (OperationCanceledException)
            {
                // The listener task was cancelled, so break out of the loop
                break;
            }
            catch (Exception)
            {
                // Ignore malformed payloads or pipe issues and continue listening.
            }
        }
    }

    /// <summary>
    /// Reads exactly the specified number of bytes from the stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="buffer">The buffer to read into.</param>
    /// <param name="count">The number of bytes to read.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The number of bytes read.</returns>
    static async Task ReadExactlyAsync(Stream stream, byte[] buffer, int count, CancellationToken token)
    {
        var offset = 0;
        // Read until the buffer is filled or the stream is closed
        while (offset < count && !token.IsCancellationRequested)
            offset += await stream.ReadAsync(buffer.AsMemory(offset, count - offset), token).ConfigureAwait(false);
    }
    #endregion
}

