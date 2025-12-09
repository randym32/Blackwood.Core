// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Reflection;
using NUnit.Framework;
using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Tests for AssemblyExtensions.FindClasses extension method.
/// Verifies that the method correctly finds non-abstract subclasses of a specified type.
/// </summary>
public class AssemblyExtensionsTests
{
	/// <summary>
	/// Base class for testing subclass discovery.
	/// </summary>
	private class BaseTestClass { }

	/// <summary>
	/// Concrete subclass to verify discovery.
	/// </summary>
	private class ConcreteSubclass : BaseTestClass { }

	/// <summary>
	/// Another concrete subclass to verify multiple matches.
	/// </summary>
	private class AnotherConcreteSubclass : BaseTestClass { }

	/// <summary>
	/// Abstract subclass that should be excluded from results.
	/// </summary>
	private abstract class AbstractSubclass : BaseTestClass { }

	/// <summary>
	/// Concrete subclass of abstract class to verify nested inheritance.
	/// </summary>
	private class ConcreteSubclassOfAbstract : AbstractSubclass { }

	/// <summary>
	/// Base class for another inheritance hierarchy.
	/// </summary>
	private class AnotherBaseClass { }

	/// <summary>
	/// Class that doesn't inherit from BaseTestClass.
	/// </summary>
	private class UnrelatedClass : AnotherBaseClass { }

	/// <summary>
	/// FindClasses returns concrete subclasses from the assembly.
	/// </summary>
	[Test]
	public void FindClasses_ReturnsConcreteSubclasses()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(BaseTestClass)).ToList();

		Assert.That(results, Does.Contain(typeof(ConcreteSubclass)));
		Assert.That(results, Does.Contain(typeof(AnotherConcreteSubclass)));
		Assert.That(results, Does.Contain(typeof(ConcreteSubclassOfAbstract)));
	}

	/// <summary>
	/// FindClasses excludes abstract classes from results.
	/// </summary>
	[Test]
	public void FindClasses_ExcludesAbstractClasses()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(BaseTestClass)).ToList();

		Assert.That(results, Does.Not.Contain(typeof(AbstractSubclass)));
		Assert.That(results.All(t => !t.IsAbstract), Is.True);
	}

	/// <summary>
	/// FindClasses excludes the base type itself from results.
	/// </summary>
	[Test]
	public void FindClasses_ExcludesBaseType()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(BaseTestClass)).ToList();

		Assert.That(results, Does.Not.Contain(typeof(BaseTestClass)));
	}

	/// <summary>
	/// FindClasses excludes unrelated classes that don't inherit from the base type.
	/// </summary>
	[Test]
	public void FindClasses_ExcludesUnrelatedClasses()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(BaseTestClass)).ToList();

		Assert.That(results, Does.Not.Contain(typeof(UnrelatedClass)));
		Assert.That(results, Does.Not.Contain(typeof(AnotherBaseClass)));
	}

	/// <summary>
	/// FindClasses returns empty collection when no matching subclasses exist.
	/// </summary>
	[Test]
	public void FindClasses_NoMatches_ReturnsEmpty()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(string)).ToList();

		// string is sealed, so no subclasses should exist in this assembly
		Assert.That(results, Is.Empty);
	}

	/// <summary>
	/// FindClasses handles object as base type (should find all classes).
	/// </summary>
	[Test]
	public void FindClasses_WithObjectBaseType_ReturnsAllConcreteClasses()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(object)).ToList();

		// Should find all concrete classes in the assembly
		Assert.That(results, Is.Not.Empty);
		Assert.That(results.All(t => !t.IsAbstract), Is.True);
		Assert.That(results.All(t => t != typeof(object)), Is.True);
		Assert.That(results, Does.Contain(typeof(ConcreteSubclass)));
		Assert.That(results, Does.Contain(typeof(AnotherConcreteSubclass)));
	}

	/// <summary>
	/// FindClasses handles sealed base types correctly.
	/// </summary>
	[Test]
	public void FindClasses_WithSealedBaseType_ReturnsEmpty()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(int)).ToList();

		// int is sealed, so no subclasses should exist
		Assert.That(results, Is.Empty);
	}

	/// <summary>
	/// FindClasses works with nested inheritance hierarchies.
	/// </summary>
	[Test]
	public void FindClasses_WithNestedInheritance_FindsAllLevels()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(BaseTestClass)).ToList();

		// Should find both direct and indirect subclasses
		Assert.That(results, Does.Contain(typeof(ConcreteSubclass))); // Direct subclass
		Assert.That(results, Does.Contain(typeof(ConcreteSubclassOfAbstract))); // Indirect subclass
	}

	/// <summary>
	/// FindClasses returns results as IEnumerable (lazy evaluation).
	/// </summary>
	[Test]
	public void FindClasses_ReturnsIEnumerable()
	{
		var assembly = Assembly.GetExecutingAssembly();
		var results = assembly.FindClasses(typeof(BaseTestClass));

		// Verify it's an IEnumerable and can be enumerated multiple times
		var firstEnumeration = results.ToList();
		var secondEnumeration = results.ToList();

		Assert.That(firstEnumeration.Count, Is.EqualTo(secondEnumeration.Count));
		Assert.That(firstEnumeration, Is.EquivalentTo(secondEnumeration));
	}
}

