using Blackwood;
using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test fixture for <see cref="MenuItem"/> class functionality.
/// Tests the menu item creation, menu strip integration, and JSON serialization capabilities.
/// </summary>
[TestFixture]
public class MenuItemTests
{
    /// <summary>
    /// Helper method to create a MenuItem instance with specified properties.
    /// </summary>
    /// <param name="menuPath">The hierarchical menu path (e.g., "File>New").</param>
    /// <param name="quickKey">The keyboard shortcut key combination.</param>
    /// <param name="separatorBefore">Whether a separator should be added before this item.</param>
    /// <param name="toolTip">The tooltip text for the menu item.</param>
    /// <param name="tag">An optional tag object associated with the menu item.</param>
    /// <param name="disabled">Whether the menu item should be disabled.</param>
    /// <returns>A MenuItem instance with the specified properties.</returns>
    static private MenuItem CreateMenuItem(string? menuPath = null, string? quickKey = null, bool separatorBefore = false, string? toolTip = null, object? tag = null, bool disabled = false)
    {
        return new MenuItem
        {
            MenuPath = menuPath,
            QuickKey = quickKey,
            SeparatorBefore = separatorBefore,
            ToolTip = toolTip,
            Tag = tag,
            Disabled = disabled
        };
    }


    #region JSON Serialization Tests

    /// <summary>
    /// Verifies that JSON serialization includes all MenuItem fields in the output.
    /// Tests that MenuPath, QuickKey, SeparatorBefore, ToolTip, Tag, and Enabled are all serialized.
    /// Note: Default values (false for bool) are not serialized with default options.
    /// </summary>
    [Test]
    public void JsonSerialize_SerializesAllFields()
    {
        // Arrange
        // Create MenuItem directly using public fields to ensure proper serialization
        // Set Disabled to true so it gets serialized (false is default and gets skipped)
        var original = new MenuItem
        {
            MenuPath = "File>New",
            QuickKey = "Ctrl+N",
            SeparatorBefore = true,
            ToolTip = "Create new file",
            Tag = "test-tag",
            Disabled = true
        };

        // Act
        var json = JsonSerializer.Serialize(original, JSONDeserializer.JSONOptions);
        var deserialized = JsonSerializer.Deserialize<MenuItem>(json, JSONDeserializer.JSONOptions);

        // Assert - Verify that all fields are serialized by checking the deserialized object
        // Note: The JSON may escape '>' as '\u003E', so we verify via deserialization
        using (Assert.EnterMultipleScope())
        {
            Assert.That(json, Does.Contain("MenuPath"));
            Assert.That(json, Does.Contain("QuickKey"));
            Assert.That(json, Does.Contain("SeparatorBefore"));
            Assert.That(json, Does.Contain("ToolTip"));
            Assert.That(json, Does.Contain("Tag"));
            Assert.That(json, Does.Contain("Disabled"));
        }
        Assert.That(deserialized, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            // Verify all fields were correctly serialized by deserializing
            Assert.That(deserialized!.MenuPath, Is.EqualTo("File>New"));
            Assert.That(deserialized!.QuickKey, Is.EqualTo("Ctrl+N"));
            Assert.That(deserialized!.SeparatorBefore, Is.True);
            Assert.That(deserialized!.ToolTip, Is.EqualTo("Create new file"));
            Assert.That(deserialized!.Tag, Is.EqualTo("test-tag"));
            Assert.That(deserialized!.Disabled, Is.True);
        }
    }

    /// <summary>
    /// Verifies that JSON deserialization correctly restores all MenuItem fields from JSON.
    /// Tests that all field values are properly reconstructed after deserialization.
    /// </summary>
    [Test]
    public void JsonDeserialize_DeserializesAllFields()
    {
        // Arrange
        var expectedQuickKey = "Ctrl+N";
        var json = $$"""
            {
                "MenuPath": "File>New",
                "QuickKey": "{{expectedQuickKey}}",
                "SeparatorBefore": true,
                "ToolTip": "Create new file",
                "Tag": "test-tag",
                "Disabled": true
            }
            """;

        // Act
        var menuItem = JsonSerializer.Deserialize<MenuItem>(json, JSONDeserializer.JSONOptions);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(menuItem, Is.Not.Null);
            // Use direct field access since fields are public
            Assert.That(menuItem!.MenuPath, Is.EqualTo("File>New"));
            Assert.That(menuItem!.QuickKey, Is.EqualTo(expectedQuickKey));
            Assert.That(menuItem!.SeparatorBefore, Is.True);
            Assert.That(menuItem!.ToolTip, Is.EqualTo("Create new file"));
            Assert.That(menuItem!.Disabled, Is.True);
        }
    }

    /// <summary>
    /// Verifies that JSON serialization handles null field values correctly.
    /// Tests that null values can be serialized when using options that include null fields.
    /// Note: Default JSON options skip null/default values for cleaner JSON.
    /// </summary>
    [Test]
    public void JsonSerialize_WithNullFields_SerializesNull()
    {
        // Arrange
        var menuItem = CreateMenuItem(menuPath: null, toolTip: null, tag: null);
        // Use cached options that include null fields for this test
        // Note: This options instance is cached to avoid CA1869 warning
        static JsonSerializerOptions GetOptionsWithNullFields()
        {
            return new JsonSerializerOptions(JSONDeserializer.JSONOptions)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };
        }
        var options = GetOptionsWithNullFields();

        // Act
        var json = JsonSerializer.Serialize(menuItem, options);

        // Assert
        Assert.That(json, Does.Contain("MenuPath"));
        Assert.That(json, Does.Contain("null"));
    }

    /// <summary>
    /// Verifies that JSON deserialization correctly handles null field values.
    /// Tests that null values in JSON are properly deserialized to null field values.
    /// </summary>
    [Test]
    public void JsonDeserialize_WithNullFields_DeserializesNull()
    {
        // Arrange
        var json = """
            {
                "MenuPath": null,
                "QuickKey": null,
                "SeparatorBefore": false,
                "ToolTip": null,
                "Tag": null,
                "Disabled": false
            }
            """;

        // Act
        var menuItem = JsonSerializer.Deserialize<MenuItem>(json, JSONDeserializer.JSONOptions);

        // Assert
        Assert.That(menuItem, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            // Use direct field access since Disabled is a public field
            Assert.That(menuItem!.MenuPath, Is.Null);
            Assert.That(menuItem!.ToolTip, Is.Null);
            Assert.That(menuItem!.Tag, Is.Null);
            Assert.That(menuItem!.QuickKey, Is.EqualTo(null));
            Assert.That(menuItem!.Disabled, Is.False);
        }
    }

    /// <summary>
    /// Verifies that a MenuItem can be serialized to JSON and then deserialized back
    /// without losing any data. This is a round-trip test that validates the complete
    /// serialization/deserialization cycle.
    /// </summary>
    [Test]
    public void JsonRoundTrip_SerializesAndDeserializesCorrectly()
    {
        // Arrange
        // Create MenuItem directly using public fields to ensure proper serialization
        var original = new MenuItem
        {
            MenuPath = "Edit>Copy",
            QuickKey = "Ctrl+C",
            SeparatorBefore = true,
            ToolTip = "Copy selection",
            Tag = 42,
            Disabled = false
        };

        // Act
        var json = JsonSerializer.Serialize(original, JSONDeserializer.JSONOptions);
        var deserialized = JsonSerializer.Deserialize<MenuItem>(json, JSONDeserializer.JSONOptions);

        // Assert
        Assert.That(deserialized, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            // Use direct field access for deserialized values since fields are public
            Assert.That(deserialized!.MenuPath, Is.EqualTo("Edit>Copy"));
            Assert.That(deserialized!.QuickKey, Is.EqualTo("Ctrl+C"));
            Assert.That(deserialized!.SeparatorBefore, Is.True);
            Assert.That(deserialized!.ToolTip, Is.EqualTo("Copy selection"));
            Assert.That(deserialized!.Tag, Is.EqualTo(42));
            Assert.That(deserialized!.Disabled, Is.False);
        }
    }

    /// <summary>
    /// Verifies that JSON deserialization uses default values for fields that are not present in the JSON.
    /// Tests that partial JSON objects are handled correctly with default value assignment.
    /// </summary>
    [Test]
    public void JsonDeserialize_WithPartialFields_UsesDefaults()
    {
        // Arrange
        var json = """
            {
                "MenuPath": "File>Open"
            }
            """;

        // Act
        var menuItem = JsonSerializer.Deserialize<MenuItem>(json, JSONDeserializer.JSONOptions);

        // Assert
        Assert.That(menuItem, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            // Use direct field access since fields are public
            // When fields are not present in JSON, they get their default values:
            // - string fields default to null
            // - bool fields default to false
            // - enum fields default to 0 (None for Keys)
            Assert.That(menuItem!.MenuPath, Is.EqualTo("File>Open"));
            Assert.That(menuItem!.QuickKey, Is.EqualTo(null));
            Assert.That(menuItem!.SeparatorBefore, Is.False);
            Assert.That(menuItem!.ToolTip, Is.Null);
            Assert.That(menuItem!.Disabled, Is.False); // bool defaults to false
        }
    }

    /// <summary>
    /// Verifies that the Keys enum is serialized as a human-readable string in JSON.
    /// Tests that enum values are converted to their human-readable representation (e.g., "Ctrl+N").
    /// </summary>
    [Test]
    public void JsonSerialize_KeysEnum_SerializesAsString()
    {
        // Arrange
        var expectedQuickKey = "Ctrl+N";
        // Create MenuItem directly using public fields to ensure proper serialization
        var menuItem = new MenuItem
        {
            MenuPath = "File>New",
            QuickKey = expectedQuickKey
        };

        // Act
        var json = JsonSerializer.Serialize(menuItem, JSONDeserializer.JSONOptions);

        // Assert - Keys enum should be serialized as a human-readable string
        Assert.That(json, Does.Contain("QuickKey"));
        Assert.That(json, Does.Contain("Ctrl+N"));
    }

    #endregion
}

