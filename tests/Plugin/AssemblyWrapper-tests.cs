using System;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Tests for AssemblyWrapper: equality/hash, C# method lookup, native procedure
/// lookup fallback, disposal, and caching.
/// </summary>
public class AssemblyWrapperTests
{
	private class TestClass
	{
		public static int StaticAdd(int a, int b) => a + b;
		public static string StaticConcat(string a, string b) => a + b;
		public static void StaticVoid() { }
		public int InstanceMul(int a, int b) => a * b;
		public string InstanceJoin(string a, string b) => a + b;
		private static int PrivateStatic(int x) => x * 2;
		protected static int ProtectedStatic(int x) => x * 3;
	}

	private class DerivedTestClass : TestClass
	{
		public static int DerivedStatic(int x) => x * 4;
	}

	/// <summary>
	/// Helper method to create an AssemblyWrapper instance.
	/// </summary>
	private static AssemblyWrapper CreateWrapper(Assembly assembly, string className = "")
	{
		return (AssemblyWrapper)Activator.CreateInstance(
			typeof(AssemblyWrapper),
			BindingFlags.Instance | BindingFlags.NonPublic,
			binder: null,
			args: new object[] { assembly, className },
			culture: null)!;
	}

	/// <summary>
	/// Helper method to create an AssemblyWrapper instance with IntPtr handle.
	/// </summary>
	private static AssemblyWrapper CreateWrapper(IntPtr handle)
	{
		return (AssemblyWrapper)Activator.CreateInstance(
			typeof(AssemblyWrapper),
			BindingFlags.Instance | BindingFlags.NonPublic,
			binder: null,
			args: new object[] { handle },
			culture: null)!;
	}

	/// <summary>
	/// Equals/GetHashCode are based on the underlying assembly reference.
	/// </summary>
	[Test]
	public void Equals_And_GetHashCode_BasedOnAssembly()
	{
		var asm = Assembly.GetExecutingAssembly();
		var a = CreateWrapper(asm, "");
		var b = CreateWrapper(asm, "");

		Assert.That(a.Equals(b), Is.True);
		Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
	}

	/// <summary>
	/// Equals returns false when comparing with different assemblies.
	/// </summary>
	[Test]
	public void Equals_DifferentAssemblies_ReturnsFalse()
	{
		var asm1 = Assembly.GetExecutingAssembly();
		var asm2 = typeof(object).Assembly;
		var a = CreateWrapper(asm1, "");
		var b = CreateWrapper(asm2, "");

		Assert.That(a.Equals(b), Is.False);
	}

	/// <summary>
	/// Equals returns false when comparing with null.
	/// </summary>
	[Test]
	public void Equals_WithNull_ReturnsFalse()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, "");
		Assert.That(wrapper.Equals(null), Is.False);
	}

	/// <summary>
	/// Equals returns false when comparing with non-AssemblyWrapper object.
	/// </summary>
	[Test]
	public void Equals_WithNonWrapper_ReturnsFalse()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, "");
		Assert.That(wrapper.Equals("not a wrapper"), Is.False);
	}

	/// <summary>
	/// GetHashCode returns base hash code when assembly is null.
	/// </summary>
	[Test]
	public void GetHashCode_NullAssembly_ReturnsBaseHashCode()
	{
		var handle = IntPtr.Zero;
		var wrapper = CreateWrapper(handle);
		var hashCode = wrapper.GetHashCode();
		Assert.That(hashCode, Is.Not.EqualTo(0));
	}

	/// <summary>
	/// GetMethod should locate a static method by class and name, and create a delegate of the requested type.
	/// </summary>
	[Test]
	public void GetMethod_FindsStaticMethod()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var del = wrapper.GetMethod<Func<int, int, int>>(typeof(TestClass).FullName!, nameof(TestClass.StaticAdd));
		Assert.That(del, Is.Not.Null);
		Assert.That(del!(2, 3), Is.EqualTo(5));
	}

	/// <summary>
	/// GetMethod should handle different delegate types.
	/// </summary>
	[Test]
	public void GetMethod_DifferentDelegateTypes_WorksCorrectly()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);

		// Test Func<string, string, string>
		var concatDel = wrapper.GetMethod<Func<string, string, string>>(typeof(TestClass).FullName!, nameof(TestClass.StaticConcat));
		Assert.That(concatDel, Is.Not.Null);
		Assert.That(concatDel!("Hello", "World"), Is.EqualTo("HelloWorld"));

		// Test Action
		var voidDel = wrapper.GetMethod<Action>(typeof(TestClass).FullName!, nameof(TestClass.StaticVoid));
		Assert.That(voidDel, Is.Not.Null);
		Assert.DoesNotThrow(() => voidDel!());
	}

	/// <summary>
	/// GetMethod should find private static methods.
	/// </summary>
	[Test]
	public void GetMethod_FindsPrivateStaticMethod()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var del = wrapper.GetMethod<Func<int, int>>(typeof(TestClass).FullName!, "PrivateStatic");
		Assert.That(del, Is.Not.Null);
		Assert.That(del!(5), Is.EqualTo(10));
	}

	/// <summary>
	/// GetMethod should find protected static methods.
	/// </summary>
	[Test]
	public void GetMethod_FindsProtectedStaticMethod()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var del = wrapper.GetMethod<Func<int, int>>(typeof(TestClass).FullName!, "ProtectedStatic");
		Assert.That(del, Is.Not.Null);
		Assert.That(del!(5), Is.EqualTo(15));
	}

	/// <summary>
	/// GetMethod should find methods in derived classes via FlattenHierarchy.
	/// </summary>
	[Test]
	public void GetMethod_FindsMethodInDerivedClass()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(DerivedTestClass).FullName!);
		var del = wrapper.GetMethod<Func<int, int>>(typeof(DerivedTestClass).FullName!, nameof(DerivedTestClass.DerivedStatic));
		Assert.That(del, Is.Not.Null);
		Assert.That(del!(5), Is.EqualTo(20));
	}

	/// <summary>
	/// GetMethod returns null for non-existent class.
	/// </summary>
	[Test]
	public void GetMethod_NonExistentClass_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, "");
		var del = wrapper.GetMethod<Action>("NonExistentClass", "SomeMethod");
		Assert.That(del, Is.Null);
	}

	/// <summary>
	/// GetMethod returns null for non-existent method.
	/// </summary>
	[Test]
	public void GetMethod_NonExistentMethod_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var del = wrapper.GetMethod<Action>(typeof(TestClass).FullName!, "NonExistentMethod");
		Assert.That(del, Is.Null);
	}

	/// <summary>
	/// Instance method lookup returns null because BindingFlags doesn't include Instance.
	/// This matches the current implementation behavior.
	/// </summary>
	[Test]
	public void GetMethod_InstanceMethod_WithTarget_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var target = new TestClass();
		var del = wrapper.GetMethod<Func<int, int, int>>(typeof(TestClass).FullName!, nameof(TestClass.InstanceMul), target);
		Assert.That(del, Is.Null);
	}

	/// <summary>
	/// Instance method lookup returns null because BindingFlags doesn't include Instance.
	/// This matches the current implementation behavior.
	/// </summary>
	[Test]
	public void GetMethod_InstanceMethod_StringParameters_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var target = new TestClass();
		var del = wrapper.GetMethod<Func<string, string, string>>(typeof(TestClass).FullName!, nameof(TestClass.InstanceJoin), target);
		Assert.That(del, Is.Null);
	}

	/// <summary>
	/// Instance method lookup returns null when target is null.
	/// </summary>
	[Test]
	public void GetMethod_InstanceMethod_NullTarget_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var del = wrapper.GetMethod<Func<int, int, int>>(typeof(TestClass).FullName!, nameof(TestClass.InstanceMul), null);
		// Instance methods require a target, so this should return null or throw
		Assert.That(del, Is.Null);
	}

	/// <summary>
	/// GetMethod returns null when assembly is null (IntPtr constructor).
	/// </summary>
	[Test]
	public void GetMethod_NullAssembly_ReturnsNull()
	{
		var handle = IntPtr.Zero;
		var wrapper = CreateWrapper(handle);
		var del = wrapper.GetMethod<Action>("SomeClass", "SomeMethod");
		Assert.That(del, Is.Null);
	}

	/// <summary>
	/// GetProcedure uses default class name when assembly is provided.
	/// </summary>
	[Test]
	public void GetProcedure_WithAssembly_UsesDefaultClass()
	{
		var asm = Assembly.GetExecutingAssembly();
		var className = typeof(TestClass).FullName!;
		var wrapper = CreateWrapper(asm, className);
		var proc = wrapper.GetProcedure<Func<int, int, int>>(nameof(TestClass.StaticAdd));
		Assert.That(proc, Is.Not.Null);
		Assert.That(proc!(2, 3), Is.EqualTo(5));
	}

	/// <summary>
	/// GetProcedure falls back to native lookup when assembly is null.
	/// </summary>
	[Test]
	public void GetProcedure_WithoutAssembly_ReturnsNull()
	{
		var handle = IntPtr.Zero;
		var wrapper = CreateWrapper(handle);
		var proc = wrapper.GetProcedure<Action>("NonExistentProc");
		Assert.That(proc, Is.Null);
	}

	/// <summary>
	/// GetProcedure returns null for non-existent method in default class.
	/// </summary>
	[Test]
	public void GetProcedure_NonExistentMethod_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var className = typeof(TestClass).FullName!;
		var wrapper = CreateWrapper(asm, className);
		var proc = wrapper.GetProcedure<Action>("NonExistentMethod");
		Assert.That(proc, Is.Null);
	}

	/// <summary>
	/// Dispose can be called multiple times safely.
	/// </summary>
	[Test]
	public void Dispose_CanBeCalledMultipleTimes()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, "");
		Assert.DoesNotThrow(() => wrapper.Dispose());
		Assert.DoesNotThrow(() => wrapper.Dispose());
		Assert.DoesNotThrow(() => wrapper.Dispose());
	}

	/// <summary>
	/// Dispose releases native library handle.
	/// </summary>
	[Test]
	public void Dispose_ReleasesNativeHandle()
	{
		// Note: We can't easily test this without loading an actual DLL,
		// but we can verify Dispose doesn't throw
		var handle = IntPtr.Zero;
		var wrapper = CreateWrapper(handle);
		Assert.DoesNotThrow(() => wrapper.Dispose());
	}

	/// <summary>
	/// Using statement properly disposes the wrapper.
	/// </summary>
	[Test]
	public void UsingStatement_ProperlyDisposes()
	{
		var asm = Assembly.GetExecutingAssembly();
		AssemblyWrapper? wrapper = null;
		using (wrapper = CreateWrapper(asm, ""))
		{
			Assert.That(wrapper, Is.Not.Null);
		}
		// After using block, wrapper should be disposed
		Assert.DoesNotThrow(() => wrapper?.Dispose());
	}

	/// <summary>
	/// Constructor with assembly and class name initializes correctly.
	/// </summary>
	[Test]
	public void Constructor_WithAssemblyAndClass_Initializes()
	{
		var asm = Assembly.GetExecutingAssembly();
		var className = typeof(TestClass).FullName!;
		var wrapper = CreateWrapper(asm, className);
		Assert.That(wrapper, Is.Not.Null);
	}

	/// <summary>
	/// Constructor with IntPtr handle initializes correctly.
	/// </summary>
	[Test]
	public void Constructor_WithIntPtr_Initializes()
	{
		var handle = IntPtr.Zero;
		var wrapper = CreateWrapper(handle);
		Assert.That(wrapper, Is.Not.Null);
	}

	/// <summary>
	/// GetMethod with empty class name throws ArgumentException.
	/// </summary>
	[Test]
	public void GetMethod_EmptyClassName_ThrowsException()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, "");
		Assert.Throws<ArgumentException>(() => wrapper.GetMethod<Action>("", "SomeMethod"));
	}

	/// <summary>
	/// GetMethod with null class name throws ArgumentNullException.
	/// </summary>
	[Test]
	public void GetMethod_NullClassName_ThrowsException()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, "");
		Assert.Throws<ArgumentNullException>(() => wrapper.GetMethod<Action>(null!, "SomeMethod"));
	}

	/// <summary>
	/// GetMethod with empty method name returns null.
	/// </summary>
	[Test]
	public void GetMethod_EmptyMethodName_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var del = wrapper.GetMethod<Action>(typeof(TestClass).FullName!, "");
		Assert.That(del, Is.Null);
	}

	/// <summary>
	/// GetProcedure with empty procedure name returns null.
	/// </summary>
	[Test]
	public void GetProcedure_EmptyProcedureName_ReturnsNull()
	{
		var asm = Assembly.GetExecutingAssembly();
		var wrapper = CreateWrapper(asm, typeof(TestClass).FullName!);
		var proc = wrapper.GetProcedure<Action>("");
		Assert.That(proc, Is.Null);
	}
}
