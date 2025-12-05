using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test class for Utils functionality.
/// This class contains comprehensive tests for the Utils static class, covering its methods
/// for converting C# names to labels and fitting text to specified widths.
/// </summary>
public class AccessorTests
{

    #region GetMemberAccessor Tests

    /// <summary>
    /// Test class with various properties and fields for testing GetMemberAccessor.
    /// </summary>
    public class TestClassForAccessor
    {
        public string PublicProperty { get; set; } = "PublicPropertyValue";
        public int PublicField = 42;

        private string _privateProperty = "PrivatePropertyValue";
        private int _privateField = 24;

        // Instance-backed values to avoid CA1822 (methods referencing instance data)
        private readonly string _methodValue = "MethodValue";
        private readonly int _numberValue = 123;
        private readonly string _privateDataValue = "PrivateDataValue";
        private readonly bool _booleanValue = true;

        protected string ProtectedProperty { get; set; } = "ProtectedPropertyValue";
        protected int ProtectedField = 84;

        internal string InternalProperty { get; set; } = "InternalPropertyValue";
        internal int InternalField = 48;

        public string PrivateProperty
        {
            get => _privateProperty;
            set => _privateProperty = value;
        }

        public int PrivateField
        {
            get => _privateField;
            set => _privateField = value;
        }

        public string ProtectedPropertyAccessor
        {
            get => ProtectedProperty;
            set => ProtectedProperty = value;
        }

        public int ProtectedFieldAccessor
        {
            get => ProtectedField;
            set => ProtectedField = value;
        }

        public string InternalPropertyAccessor
        {
            get => InternalProperty;
            set => InternalProperty = value;
        }

        public int InternalFieldAccessor
        {
            get => InternalField;
            set => InternalField = value;
        }

        // Methods for testing method-based accessors
        public string GetMethodValue() => _methodValue;
        public int GetNumber() => _numberValue;
        public string GetPrivateData() => _privateDataValue;
        public bool GetBooleanValue() => _booleanValue;

        // Method with parameters (should not be found)
        public static string GetValueWithParam(string param) => param;

        // Method that doesn't start with "Get" (should be found by GetMemberAccessor)
        public string RetrieveValue() => "RetrieveValue";
    }

    /// <summary>
    /// Test class with nested properties for testing GetAccesorForPath.
    /// </summary>
    public class TestNestedClass
    {
        public TestClassForAccessor NestedObject { get; set; } = new();
        public string DirectProperty { get; set; } = "DirectValue";
        public int DirectField = 100;

        // Instance-backed value to avoid CA1822
        private readonly string _nestedMethodValue = "NestedMethodValue";

        // Method for testing method-based path access
        public string GetNestedValue() => _nestedMethodValue;

        // Property that returns null for testing null intermediate objects
        public TestClassForAccessor? NullNestedObject { get; set; } = null;
    }

    /// <summary>
    /// A static holder to validate static member access via the type-based API.
    /// </summary>
    public static class StaticHolder
    {
        public static string StaticProperty { get; set; } = "StaticValue";
        public static int StaticField = 7;
        public static string GetStaticThing() => "StaticThing";
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly retrieves public properties.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithPublicProperty_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string propertyName = "PublicProperty";
        const string expectedValue = "PublicPropertyValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, propertyName, out var setter);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
        Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const string newValue = "NewValue";
        setter!(newValue);
        Assert.Multiple(() =>
        {
            Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
        });
    }

    /// <summary>
    /// Ensures type-based overload can access instance properties when type and target are supplied.
    /// </summary>
    [Test]
    public void GetMemberAccessor_TypeOverload_Instance_Works()
    {
        var obj = new TestClassForAccessor();
        var getter = AssemblyUtils.GetMemberAccessor(typeof(TestClassForAccessor), obj, "PublicProperty", out var setter);
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null);
            Assert.That(setter, Is.Not.Null);
            Assert.That(getter!(), Is.EqualTo("PublicPropertyValue"));
        });
        setter!("X");
        Assert.Multiple(() =>
        {
            Assert.That(getter!(), Is.EqualTo("X"));
        });
    }

    /// <summary>
    /// Ensures type-based overload can access static members when target is null.
    /// </summary>
    [Test]
    public void GetMemberAccessor_TypeOverload_Static_Works()
    {
        var getterProp = AssemblyUtils.GetMemberAccessor(typeof(StaticHolder), null!, nameof(StaticHolder.StaticProperty), out var setterProp);
        Assert.Multiple(() =>
        {
            Assert.That(getterProp, Is.Not.Null);
            Assert.That(setterProp, Is.Not.Null);
        Assert.That(getterProp!(), Is.EqualTo("StaticValue"));
        });
        setterProp!("NewStatic");
        Assert.That(getterProp!(), Is.EqualTo("NewStatic"));

        var getterField = AssemblyUtils.GetMemberAccessor(typeof(StaticHolder), null!, nameof(StaticHolder.StaticField), out var setterField);
        Assert.Multiple(() =>
        {
            Assert.That(getterField, Is.Not.Null);
            Assert.That(setterField, Is.Not.Null);
        Assert.That(getterField!(), Is.EqualTo(7));
        });
        setterField!(9);
        Assert.That(getterField!(), Is.EqualTo(9));

        var getterMethod = AssemblyUtils.GetMemberAccessor(typeof(StaticHolder), null!, "StaticThing", out var setterMethod);
        Assert.Multiple(() =>
        {
            Assert.That(getterMethod, Is.Not.Null);
            Assert.That(setterMethod, Is.Null);
            Assert.That(getterMethod!(), Is.EqualTo("StaticThing"));
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly retrieves public fields.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithPublicField_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string fieldName = "PublicField";
        const int expectedValue = 42;

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, fieldName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const int newValue = 100;
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly retrieves private properties through accessors.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithPrivatePropertyAccessor_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string propertyName = "PrivateProperty";
        const string expectedValue = "PrivatePropertyValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, propertyName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
        Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const string newValue = "NewPrivateValue";
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly retrieves protected properties through accessors.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithProtectedPropertyAccessor_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string propertyName = "ProtectedPropertyAccessor";
        const string expectedValue = "ProtectedPropertyValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, propertyName, out var setter);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
        Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const string newValue = "NewProtectedValue";
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly retrieves internal properties through accessors.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithInternalPropertyAccessor_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string propertyName = "InternalPropertyAccessor";
        const string expectedValue = "InternalPropertyValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, propertyName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const string newValue = "NewInternalValue";
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
    }

    /// <summary>
    /// Tests that GetMemberAccessor returns null for non-existent members.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithNonExistentMember_ReturnsNull()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string nonExistentMember = "NonExistentMember";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, nonExistentMember, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for non-existent member");
            Assert.That(setter, Is.Null, "Setter should be null for non-existent member");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor handles null target correctly.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithNullTarget_ReturnsNull()
    {
        // Arrange
        object? nullTarget = null;
        const string memberName = "SomeMember";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(nullTarget!, memberName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for null target");
            Assert.That(setter, Is.Null, "Setter should be null for null target");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly finds methods with exact name match.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithExactMethodName_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string methodName = "GetMethodValue";
        const string expectedValue = "MethodValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, methodName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for exact method name");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly finds methods with "Get" prefix.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithGetPrefixMethod_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string methodName = "Number"; // Should find GetNumber method
        const int expectedValue = 123;

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, methodName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for Get-prefixed method");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly finds methods with case-insensitive matching.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithCaseInsensitiveMethodName_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string methodName = "methodvalue"; // Should find GetMethodValue method
        const string expectedValue = "MethodValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, methodName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for case-insensitive method name");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor correctly finds methods with "Get" prefix and case-insensitive matching.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithGetPrefixAndCaseInsensitive_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string methodName = "privatedata"; // Should find GetPrivateData method
        const string expectedValue = "PrivateDataValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, methodName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for Get-prefixed case-insensitive method");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor does not find methods with parameters.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithMethodHavingParameters_ReturnsNull()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string methodName = "ValueWithParam"; // Should not find GetValueWithParam method (has parameters)

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, methodName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for method with parameters");
            Assert.That(setter, Is.Null, "Setter should be null for method with parameters");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor finds methods with exact name match even if they don't start with "Get".
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithNonGetMethod_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string methodName = "RetrieveValue"; // Should find RetrieveValue method (exact name match)
        const string expectedValue = "RetrieveValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, methodName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for exact method name match");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor returns null for non-existent methods.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithNonExistentMethod_ReturnsNull()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string methodName = "NonExistentMethod"; // Method that doesn't exist

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, methodName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for non-existent method");
            Assert.That(setter, Is.Null, "Setter should be null for non-existent method");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor prioritizes properties over methods.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithPropertyAndMethod_ReturnsPropertyAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string memberName = "PublicProperty"; // Has both property and GetPublicProperty method
        const string expectedValue = "PublicPropertyValue";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, memberName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null for property");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return property value, not method value");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor prioritizes fields over methods.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithFieldAndMethod_ReturnsFieldAccessor()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string memberName = "PublicField"; // Has both field and GetPublicField method
        const int expectedValue = 42;

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, memberName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null for field");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return field value, not method value");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor handles empty member name correctly.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithEmptyMemberName_ReturnsNull()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        const string emptyMemberName = "";

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, emptyMemberName, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for empty member name");
            Assert.That(setter, Is.Null, "Setter should be null for empty member name");
        });
    }

    /// <summary>
    /// Tests that GetMemberAccessor handles null member name correctly.
    /// </summary>
    [Test]
    public void GetMemberAccessor_WithNullMemberName_ReturnsNull()
    {
        // Arrange
        var testObject = new TestClassForAccessor();
        string? nullMemberName = null;

        // Act
        var getter = AssemblyUtils.GetMemberAccessor(testObject, nullMemberName!, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for null member name");
            Assert.That(setter, Is.Null, "Setter should be null for null member name");
        });
    }

    #endregion

    #region GetAccesorForPath Tests

    /// <summary>
    /// Tests that GetAccesorForPath correctly navigates a simple path.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithSimplePath_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] path = ["DirectProperty"];
        const string expectedValue = "DirectValue";

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const string newValue = "NewDirectValue";
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
    }

    /// <summary>
    /// Tests that GetAccesorForPath correctly navigates a nested path.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithNestedPath_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] path = ["NestedObject", "PublicProperty"];
        const string expectedValue = "PublicPropertyValue";

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const string newValue = "NewNestedValue";
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
    }

    /// <summary>
    /// Tests that GetAccesorForPath correctly navigates a deeper nested path.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithDeeperNestedPath_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass
        {
            NestedObject = new TestClassForAccessor()
        };
        string[] path = ["NestedObject", "PublicField"];
        const int expectedValue = 42;

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null");
            Assert.That(setter, Is.Not.Null, "Setter should not be null");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct value");
        });

        // Test setter
        const int newValue = 999;
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the value");
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles empty path correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithEmptyPath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] emptyPath = [];

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, emptyPath, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for empty path");
            Assert.That(setter, Is.Null, "Setter should be null for empty path");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles null path correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithNullPath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[]? nullPath = null;

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, nullPath!, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for null path");
            Assert.That(setter, Is.Null, "Setter should be null for null path");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles null target correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithNullTarget_ReturnsNull()
    {
        // Arrange
        object? nullTarget = null;
        string[] path = ["SomeProperty"];

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(nullTarget!, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for null target");
            Assert.That(setter, Is.Null, "Setter should be null for null target");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles non-existent path correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithNonExistentPath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] nonExistentPath = ["NonExistentProperty"];

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, nonExistentPath, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for non-existent path");
            Assert.That(setter, Is.Null, "Setter should be null for non-existent path");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles partial non-existent path correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithPartialNonExistentPath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] partialNonExistentPath = ["NestedObject", "NonExistentProperty"];

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, partialNonExistentPath, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for partial non-existent path");
            Assert.That(setter, Is.Null, "Setter should be null for partial non-existent path");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles null intermediate object correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithNullIntermediateObject_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass
        {
            NestedObject = null! // Set nested object to null
        };
        string[] path = ["NestedObject", "PublicProperty"];

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple (() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null when intermediate object is null");
            Assert.That(setter, Is.Null, "Setter should be null when intermediate object is null");
        });
    }

    #region ParseClassMemberString tests
    /// <summary>
    /// Verifies ParseClassMemberString resolves by full name and returns the trailing member path.
    /// </summary>
    [Test]
    public void ParseClassMemberString_ByFullName_Parses()
    {
        var fullName = typeof(TestClassForAccessor).FullName; // e.g., ST.UI.Tests.AccessorTests+TestClassForAccessor
        var input = fullName + ".PublicProperty";
        var t = AssemblyUtils.ParseClassMemberString(input, out var memberRef);
        Assert.Multiple(() =>
        {
            Assert.That(t, Is.EqualTo(typeof(TestClassForAccessor)));
            Assert.That(memberRef, Is.EqualTo("PublicProperty"));
        });
    }

    /// <summary>
    /// Verifies ParseClassMemberString can resolve by short Name as well.
    /// </summary>
    [Test]
    public void ParseClassMemberString_ByShortName_Parses()
    {
        var shortName = typeof(TestClassForAccessor).Name; // TestClassForAccessor
        var input = shortName + ".PublicField";
        var t = AssemblyUtils.ParseClassMemberString(input, out var memberRef);
        Assert.Multiple(() =>
        {
            Assert.That(t, Is.EqualTo(typeof(TestClassForAccessor)));
            Assert.That(memberRef, Is.EqualTo("PublicField"));
        });
    }
    #endregion

    /// <summary>
    /// Tests that GetAccesorForPath works with different data types.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithDifferentDataTypes_WorksCorrectly()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] intPath = ["NestedObject", "PublicField"];
        string[] stringPath = ["NestedObject", "PublicProperty"];

        // Act & Assert for integer
        var intGetter = AssemblyUtils.GetAccesorForPath(testObject, intPath, out var intSetter);
        Assert.Multiple(() =>
        {
            Assert.That(intGetter, Is.Not.Null, "Integer getter should not be null");
            Assert.That(intSetter, Is.Not.Null, "Integer setter should not be null");
            Assert.That(intGetter!(), Is.TypeOf<int>(), "Integer getter should return int type");
        });

        // Act & Assert for string
        var stringGetter = AssemblyUtils.GetAccesorForPath(testObject, stringPath, out var stringSetter);
        Assert.Multiple(() =>
        {
            Assert.That(stringGetter, Is.Not.Null, "String getter should not be null");
            Assert.That(stringSetter, Is.Not.Null, "String setter should not be null");
            Assert.That(stringGetter!(), Is.TypeOf<string>(), "String getter should return string type");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath correctly navigates to method-based accessors.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithMethodBasedAccessor_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] path = ["NestedObject", "GetMethodValue"];
        const string expectedValue = "MethodValue";

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for method-based accessor");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath correctly navigates to Get-prefixed method accessors.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithGetPrefixedMethodAccessor_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] path = ["NestedObject", "Number"]; // Should find GetNumber method
        const int expectedValue = 123;

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for Get-prefixed method accessor");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath correctly navigates to direct method accessors.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithDirectMethodAccessor_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] path = ["GetNestedValue"];
        const string expectedValue = "NestedMethodValue";

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for direct method accessor");
            Assert.That(setter, Is.Null, "Setter should be null for method-based accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct method value");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath correctly navigates to direct field accessors.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithDirectFieldAccessor_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] path = ["DirectField"];
        const int expectedValue = 100;

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for direct field accessor");
            Assert.That(setter, Is.Not.Null, "Setter should not be null for field accessor");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct field value");
        });

        // Test setter
        const int newValue = 200;
        setter!(newValue);
        Assert.That(getter!(), Is.EqualTo(newValue), "Setter should update the field value");
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles path with null intermediate object correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithNullIntermediateObjectProperty_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] path = ["NullNestedObject", "PublicProperty"];

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, path, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null when intermediate object is null");
            Assert.That(setter, Is.Null, "Setter should be null when intermediate object is null");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles path with whitespace in member names correctly.
    /// The method trims whitespace from path elements, so it should work with trimmed names.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithWhitespaceInPath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] pathWithWhitespace = ["DirectProperty "]; // Extra space
        string[] pathWithTab = ["DirectProperty\t"]; // Tab character

        // Act & Assert for whitespace - should work because whitespace is trimmed
        var getter1 = AssemblyUtils.GetAccesorForPath(testObject, pathWithWhitespace, out var setter1);
        Assert.Multiple(() =>
        {
            Assert.That(getter1, Is.Not.Null, "Getter should work for path with whitespace (trimmed)");
            Assert.That(setter1, Is.Not.Null, "Setter should work for path with whitespace (trimmed)");
            Assert.That(getter1!(), Is.EqualTo("DirectValue"), "Getter should return correct value");
        });

        // Act & Assert for tab - should work because whitespace is trimmed
        var getter2 = AssemblyUtils.GetAccesorForPath(testObject, pathWithTab, out var setter2);
        Assert.Multiple(() =>
        {
            Assert.That(getter2, Is.Not.Null, "Getter should work for path with tab (trimmed)");
            Assert.That(setter2, Is.Not.Null, "Setter should work for path with tab (trimmed)");
            Assert.That(getter2!(), Is.EqualTo("DirectValue"), "Getter should return correct value");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles valid nested paths correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithValidNestedPath_ReturnsCorrectAccessor()
    {
        // Arrange
        var testObject = new TestNestedClass();
        // Test a valid nested path: NestedObject.PublicField (simpler path)
        string[] validPath = ["NestedObject", "PublicField"];
        const int expectedValue = 42; // PublicField value

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, validPath, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Not.Null, "Getter should not be null for valid nested path");
            Assert.That(setter, Is.Not.Null, "Setter should not be null for valid nested path");
            Assert.That(getter!(), Is.EqualTo(expectedValue), "Getter should return correct field value");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles invalid nested paths correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithInvalidNestedPath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        // Test an invalid nested path: NestedObject.PublicProperty.NonExistentProperty
        string[] invalidPath = ["NestedObject", "PublicProperty", "NonExistentProperty"];

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, invalidPath, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for invalid nested path");
            Assert.That(setter, Is.Null, "Setter should be null for invalid nested path");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles path with special characters correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithSpecialCharactersInPath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] pathWithSpecialChars = ["Direct@Property"]; // Special character
        string[] pathWithNumbers = ["DirectProperty123"]; // Numbers

        // Act & Assert for special characters
        var getter1 = AssemblyUtils.GetAccesorForPath(testObject, pathWithSpecialChars, out var setter1);
        Assert.That(getter1, Is.Null, "Getter should be null for path with special characters");

        // Act & Assert for numbers
        var getter2 = AssemblyUtils.GetAccesorForPath(testObject, pathWithNumbers, out var setter2);
        Assert.That(getter2, Is.Null, "Getter should be null for path with numbers");
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles case sensitivity correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithCaseSensitivePath_ReturnsNull()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] caseSensitivePath = ["directproperty"]; // Wrong case

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, caseSensitivePath, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for case-sensitive path");
            Assert.That(setter, Is.Null, "Setter should be null for case-sensitive path");
        });
    }

    /// <summary>
    /// Tests that GetAccesorForPath handles path with duplicate elements correctly.
    /// </summary>
    [Test]
    public void GetAccesorForPath_WithDuplicatePathElements_HandlesCorrectly()
    {
        // Arrange
        var testObject = new TestNestedClass();
        string[] duplicatePath = ["DirectProperty", "DirectProperty"]; // Duplicate element

        // Act
        var getter = AssemblyUtils.GetAccesorForPath(testObject, duplicatePath, out var setter);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getter, Is.Null, "Getter should be null for path with duplicate elements");
            Assert.That(setter, Is.Null, "Setter should be null for path with duplicate elements");
        });
    }

    #endregion
}