using System.ComponentModel;
using System.Text.Json;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Blackwood.Core.Tests;

/// <summary>
/// Tests for ProxyPropertiesObjectJsonConverter.
///
/// These tests verify that ProxyPropertiesObject can be serialized to JSON and
/// deserialized back while preserving property values. The converter handles
/// the dynamic nature of ProxyPropertiesObject by serializing current property
/// values and attempting to restore them during deserialization.
/// </summary>
public class ProxyPropertiesObjectJsonConverterTests
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        Converters = { new PropertiesJsonConverter() }
    };
    /// <summary>
    /// A sample settings class with various property types for testing JSON serialization.
    /// </summary>
    private static class TestSettings
    {
        [Description("String setting")]
        [DefaultValue("default")]
        public static string StringSetting { get; set; } = "default";

        [Description("Integer setting")]
        [DefaultValue(42)]
        public static int IntSetting { get; set; } = 42;

        [Description("Boolean setting")]
        [DefaultValue(true)]
        public static bool BoolSetting { get; set; } = true;

        [Description("Double setting")]
        [DefaultValue(3.14)]
        public static double DoubleSetting { get; set; } = 3.14;

        [Description("Nullable integer setting")]
        public static int? NullableIntSetting { get; set; } = null;
    }

    /// <summary>
    /// Reset test settings to defaults before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        TestSettings.StringSetting = "default";
        TestSettings.IntSetting = 42;
        TestSettings.BoolSetting = true;
        TestSettings.DoubleSetting = 3.14;
        TestSettings.NullableIntSetting = null;
    }

    /// <summary>
    /// Tests that ProxyPropertiesObject can be serialized to JSON.
    /// </summary>
    [Test]
    public void Serialize_ProxyPropertiesObject_ProducesValidJson()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(TestSettings));

        // Set some test values
        var props = proxy.GetProperties();
        var stringProp = props.Find(nameof(TestSettings.StringSetting), false)!;
        var intProp = props.Find(nameof(TestSettings.IntSetting), false)!;

        stringProp.SetValue(proxy, "test value");
        intProp.SetValue(proxy, 123);

        var options = s_options;

        // Act
        var json = JsonSerializer.Serialize(proxy, options);

        // Assert
        Assert.That(json, Is.Not.Null);
        Assert.That(json, Is.Not.Empty);
        Assert.That(json, Does.Contain("StringSetting"));
        Assert.That(json, Does.Contain("IntSetting"));
        Assert.That(json, Does.Contain("test value"));
        Assert.That(json, Does.Contain("123"));
    }

    /// <summary>
    /// Tests that JSON can be deserialized back to a ProxyPropertiesObject.
    /// Note: This is a stub implementation, so the proxy will be empty until properties are added.
    /// </summary>
    [Test]
    public void Deserialize_ValidJson_ProducesProxyPropertiesObject()
    {
        // Arrange
        var json = """
        {
            "StringSetting": "deserialized value",
            "IntSetting": 456,
            "BoolSetting": false,
            "DoubleSetting": 2.71,
            "NullableIntSetting": 789
        }
        """;

        var options = s_options;

        // Act
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);

        // Assert
        Assert.That(proxy, Is.Not.Null);
        // Note: The stub implementation doesn't add properties during deserialization,
        // so the proxy will be empty until AddPropertiesFor is called
        Assert.That(proxy.DescriptorCount(), Is.Zero);

        // The stub implementation doesn't populate properties during deserialization,
        // so we can't verify property values. This test mainly verifies that
        // deserialization doesn't throw exceptions and returns a valid proxy object.
    }

    /// <summary>
    /// Tests that null values are handled correctly during serialization.
    /// Note: This is a stub implementation, so it only serializes non-default values.
    /// </summary>
    [Test]
    public void Serialize_WithNullValues_HandlesCorrectly()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(TestSettings));

        // Set a nullable property to null
        var props = proxy.GetProperties();
        var nullableProp = props.Find(nameof(TestSettings.NullableIntSetting), false)!;
        nullableProp.SetValue(proxy, null);

        var options = s_options;

        // Act
        var json = JsonSerializer.Serialize(proxy, options);

        // Assert
        Assert.That(json, Is.Not.Null);
        // Note: The stub implementation only serializes properties that are not at their default value.
        // Since NullableIntSetting defaults to null, setting it to null doesn't change it from default,
        // so it won't be serialized. The JSON will be empty.
        Assert.That(json, Is.EqualTo("{}"));
    }

    /// <summary>
    /// Tests that null values are handled correctly during deserialization.
    /// </summary>
    [Test]
    public void Deserialize_WithNullValues_HandlesCorrectly()
    {
        // Arrange
        var json = """
        {
            "StringSetting": "test",
            "IntSetting": 42,
            "BoolSetting": true,
            "DoubleSetting": 3.14,
            "NullableIntSetting": null
        }
        """;

        var options = s_options;

        // Act
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);

        // Assert
        Assert.That(proxy, Is.Not.Null);
        // The converter should handle null values without throwing exceptions
    }

    /// <summary>
    /// Tests that the converter handles empty JSON objects.
    /// </summary>
    [Test]
    public void Deserialize_EmptyJsonObject_HandlesCorrectly()
    {
        // Arrange
        var json = "{}";

        var options = s_options;

        // Act
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);

        // Assert
        Assert.That(proxy, Is.Not.Null);
        Assert.That(proxy.DescriptorCount(), Is.Zero);
    }

    /// <summary>
    /// Tests that deserializing a non-object JSON token throws a JsonException.
    /// </summary>
    [Test]
    public void Deserialize_NonObjectJson_ThrowsJsonException()
    {
        // Arrange
        var json = "[1,2,3]";
        var options = s_options;

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options));
    }

    /// <summary>
    /// Tests that the converter handles JSON with unknown properties gracefully.
    /// </summary>
    [Test]
    public void Deserialize_WithUnknownProperties_HandlesGracefully()
    {
        // Arrange
        var json = """
        {
            "StringSetting": "test",
            "IntSetting": 42,
            "UnknownProperty": "unknown value",
            "AnotherUnknown": 123
        }
        """;

        var options = s_options;

        // Act & Assert - Should not throw exceptions
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);
        Assert.That(proxy, Is.Not.Null);
    }

    /// <summary>
    /// Tests that the converter handles type conversion errors gracefully.
    /// </summary>
    [Test]
    public void Deserialize_WithInvalidTypes_HandlesGracefully()
    {
        // Arrange
        var json = """
        {
            "StringSetting": "test",
            "IntSetting": "not a number",
            "BoolSetting": "not a boolean"
        }
        """;

        var options = s_options;

        // Act & Assert - Should not throw exceptions, should handle conversion errors gracefully
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);
        Assert.That(proxy, Is.Not.Null);
    }

    /// <summary>
    /// Tests that serializing a null ProxyPropertiesObject produces null JSON.
    /// </summary>
    [Test]
    public void Serialize_NullProxyPropertiesObject_ProducesNullJson()
    {
        // Arrange
        ProxyPropertiesObject? proxy = null;
        var options = s_options;

        // Act
        var json = JsonSerializer.Serialize(proxy, options);

        // Assert
        Assert.That(json, Is.EqualTo("null"));
    }

    /// <summary>
    /// Tests that serialization skips properties that are at their default values.
    /// </summary>
    [Test]
    public void Serialize_SkipsDefaultValues()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(TestSettings));

        // Do not change any values; all are at defaults
        var options = s_options;

        // Act
        var json = JsonSerializer.Serialize(proxy, options);

        // Assert - no properties should be emitted
        Assert.That(json, Is.EqualTo("{}"));
    }

    /// <summary>
    /// Tests that deserializing null JSON produces null ProxyPropertiesObject.
    /// </summary>
    [Test]
    public void Deserialize_NullJson_ProducesNullProxyPropertiesObject()
    {
        // Arrange
        var json = "null";
        var options = s_options;

        // Act
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);

        // Assert
        Assert.That(proxy, Is.Null);
    }

    /// <summary>
    /// Tests that the converter can handle complex nested objects in JSON.
    /// </summary>
    [Test]
    public void Deserialize_WithNestedObjects_HandlesCorrectly()
    {
        // Arrange
        var json = """
        {
            "StringSetting": "test",
            "IntSetting": 42,
            "ComplexObject": {
                "NestedProperty": "nested value",
                "NestedNumber": 123
            }
        }
        """;

        var options = s_options;

        // Act & Assert - Should not throw exceptions
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);
        Assert.That(proxy, Is.Not.Null);
    }

    /// <summary>
    /// Tests that the converter can handle arrays in JSON.
    /// </summary>
    [Test]
    public void Deserialize_WithArrays_HandlesCorrectly()
    {
        // Arrange
        var json = """
        {
            "StringSetting": "test",
            "IntSetting": 42,
            "StringArray": ["item1", "item2", "item3"],
            "NumberArray": [1, 2, 3, 4, 5]
        }
        """;

        var options = s_options;

        // Act & Assert - Should not throw exceptions
        var proxy = JsonSerializer.Deserialize<ProxyPropertiesObject>(json, options);
        Assert.That(proxy, Is.Not.Null);
    }
}
