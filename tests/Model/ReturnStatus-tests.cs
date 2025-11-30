using System;
using NUnit.Framework;
using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Tests for ReturnStatus<T> success/error semantics and implicit conversion operator.
/// </summary>
public class ReturnStatusTests
{
	/// <summary>
	/// Construct with a value: HasError should be false, Error null, and implicit conversion yields value.
	/// </summary>
	[Test]
	public void ValueCtor_SetsValue_ImplicitConversionReturnsValue()
	{
		var status = new ReturnStatus<int>(42);
		Assert.That(status.HasError, Is.False, "HasError should be false when constructed with a value");
		Assert.That(status.Error, Is.Null, "Error should be null when constructed with a value");

		int val = status; // implicit conversion
		Assert.That(val, Is.EqualTo(42), "Implicit conversion should return the stored value");
	}

	/// <summary>
	/// Error case is created via ReturnStatus<T>.Failed(string). HasError true, implicit conversion throws with the message.
	/// </summary>
	[Test]
	public void FailedFactory_SetsError_ImplicitConversionThrows()
	{
		var status = ReturnStatus<string>.Failed("Something went wrong");
		Assert.That(status.HasError, Is.True, "HasError should be true when created with Failed()");
		Assert.That(status.Error, Is.EqualTo("Something went wrong"), "Error should match the provided message");

		var ex = Assert.Throws<InvalidOperationException>(() => { string _ = status; });
		Assert.That(ex!.Message, Is.EqualTo("Something went wrong"), "Exception message should be the error string");
	}

	/// <summary>
	/// For reference types (non-string), implicit conversion should return the same instance reference.
	/// </summary>
	[Test]
	public void ImplicitConversion_ReturnsSameReference_ForObject()
	{
		var obj = new string("Hello");
		var status = new ReturnStatus<string>(obj);
		string result = status; // implicit conversion
		Assert.That(result, Is.SameAs(obj), "Implicit conversion should return the same reference instance");
	}
}
