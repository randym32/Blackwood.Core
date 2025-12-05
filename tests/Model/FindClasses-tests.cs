using System.Reflection;
using NUnit.Framework;
using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Tests for Utils.FindClassesInAssembly and Utils.FindClasses (updated API).
/// Verifies ReturnStatus-based error reporting and successful discovery/filtering.
/// </summary>
public class FindClassesTests
{
	private class BaseClass { }
	/// <summary>
	/// Concrete subclass to verify discovery and filtering of non-abstract types.
	/// </summary>
	private class SampleClass : BaseClass { }

	/// <summary>
	/// FindClasses(Type) returns non-abstract subclasses from loaded assemblies.
	/// </summary>
	[Test]
	public void FindClasses_ReturnsLoadedSubclasses()
	{
		var results = AssemblyUtils.FindClasses(typeof(BaseClass)).ToList();
		Assert.That(results, Does.Contain(typeof(SampleClass)));
		Assert.That(results, Does.Not.Contain(typeof(BaseClass)));
		Assert.That(results.All(t => !t.IsAbstract), Is.True);
	}

	/// <summary>
	/// Missing file should return a failed ReturnStatus with a helpful message.
	/// </summary>
	[Test]
	public void FindClassesInAssembly_MissingFile_ReturnsFailed()
	{
		var status = AssemblyUtils.FindClassesInAssembly("nonexistent.dll", typeof(BaseClass));
		using var scope = Assert.EnterMultipleScope();
		Assert.That(status.HasError, Is.True);
		Assert.That(status.Error, Does.Contain("Assembly file not found"));
	}

	/// <summary>
	/// Unsupported extension should return a failed ReturnStatus with a helpful message.
	/// </summary>
	[Test]
	public void FindClassesInAssembly_UnsupportedExtension_ReturnsFailed()
	{
		var tmp = Path.ChangeExtension(Path.GetTempFileName(), ".txt");
		try
		{
			File.WriteAllText(tmp, "not an assembly");
			var status = AssemblyUtils.FindClassesInAssembly(tmp, typeof(BaseClass));
			using var scope = Assert.EnterMultipleScope();
			Assert.That(status.HasError, Is.True);
			Assert.That(status.Error, Does.Contain("Unsupported file type"));
		}
		finally
		{
			try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
		}
	}

	/// <summary>
	/// Valid managed assembly returns a successful ReturnStatus with filtered types.
	/// Uses the current test assembly as the input.
	/// </summary>
	[Test]
	public void FindClassesInAssembly_LoadsAndFiltersTypes_ReturnsSuccess()
	{
		var asmPath = Assembly.GetExecutingAssembly().Location;
		var status = AssemblyUtils.FindClassesInAssembly(asmPath, typeof(BaseClass));
		Assert.That(status.HasError, Is.False, () => status.Error ?? string.Empty);
        IEnumerable<Type> results = status.Value;
		Assert.That(results, Does.Contain(typeof(SampleClass)));
		Assert.That(results.Any(t => t.IsAbstract), Is.False);
	}
}
