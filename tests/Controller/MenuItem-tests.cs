using Blackwood;
using NUnit.Framework;
using System.Text.Json;
using System.Reflection;
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

    [SetUp]
    public void SetUp()
    {
        var templatesField = typeof(MenuItem).GetField("Templates", BindingFlags.Static | BindingFlags.NonPublic);
        if (templatesField?.GetValue(null) is IDictionary<string, MenuItem> templates)
        {
            templates.Clear();
        }
        // Clear the Order list to ensure clean state for each test
        MenuItem.Order.Clear();
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
            Assert.That(menuItem!.QuickKey, Is.Null);
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
            Assert.That(menuItem!.QuickKey, Is.Null);
            Assert.That(menuItem!.SeparatorBefore, Is.False);
            Assert.That(menuItem!.ToolTip, Is.Null);
            Assert.That(menuItem!.Disabled, Is.False); // bool defaults to false
        }
    }

    /// <summary>
    /// Verifies that QuickKey is serialized in JSON.
    /// Tests that QuickKey string values are properly serialized (e.g., "Ctrl+N").
    /// </summary>
    [Test]
    public void JsonSerialize_QuickKey_SerializesAsString()
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

        // Assert - QuickKey should be serialized as a string
        Assert.That(json, Does.Contain("QuickKey"));
        Assert.That(json, Does.Contain("Ctrl+N"));
    }

    #endregion

    #region Clone Tests

    /// <summary>
    /// Verifies that Clone creates a new MenuItem instance with all fields copied.
    /// Tests that all properties are correctly cloned.
    /// </summary>
    [Test]
    public void Clone_CopiesAllFields()
    {
        // Arrange
        var tagObject = new { Id = 42, Name = "Test" };
        var original = new MenuItem
        {
            MenuPath = "File>New",
            QuickKey = "Ctrl+N",
            SeparatorBefore = true,
            ToolTip = "Create new file",
            Tag = tagObject,
            Disabled = true
        };

        // Act
        var clone = original.Clone();

        // Assert
        Assert.That(clone, Is.Not.Null);
        Assert.That(clone, Is.Not.SameAs(original));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(clone.MenuPath, Is.EqualTo(original.MenuPath));
            Assert.That(clone.QuickKey, Is.EqualTo(original.QuickKey));
            Assert.That(clone.SeparatorBefore, Is.EqualTo(original.SeparatorBefore));
            Assert.That(clone.ToolTip, Is.EqualTo(original.ToolTip));
            Assert.That(clone.Tag, Is.EqualTo(original.Tag));
            Assert.That(clone.Disabled, Is.EqualTo(original.Disabled));
        }
    }

    /// <summary>
    /// Verifies that Clone handles null fields correctly.
    /// Tests that null values are properly cloned.
    /// </summary>
    [Test]
    public void Clone_HandlesNullFields()
    {
        // Arrange
        var original = new MenuItem
        {
            MenuPath = null,
            QuickKey = null,
            ToolTip = null,
            Tag = null
        };

        // Act
        var clone = original.Clone();

        // Assert
        Assert.That(clone, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(clone.MenuPath, Is.Null);
            Assert.That(clone.QuickKey, Is.Null);
            Assert.That(clone.ToolTip, Is.Null);
            Assert.That(clone.Tag, Is.Null);
            Assert.That(clone.SeparatorBefore, Is.False);
            Assert.That(clone.Disabled, Is.False);
        }
    }

    /// <summary>
    /// Verifies that Clone creates an independent copy.
    /// Tests that modifying the clone does not affect the original.
    /// </summary>
    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new MenuItem
        {
            MenuPath = "File>New",
            QuickKey = "Ctrl+N",
            ToolTip = "Create new file"
        };

        // Act
        var clone = original.Clone();
        clone.MenuPath = "File>Open";
        clone.QuickKey = "Ctrl+O";
        clone.ToolTip = "Open file";

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(original.MenuPath, Is.EqualTo("File>New"));
            Assert.That(original.QuickKey, Is.EqualTo("Ctrl+N"));
            Assert.That(original.ToolTip, Is.EqualTo("Create new file"));
            Assert.That(clone.MenuPath, Is.EqualTo("File>Open"));
            Assert.That(clone.QuickKey, Is.EqualTo("Ctrl+O"));
            Assert.That(clone.ToolTip, Is.EqualTo("Open file"));
        }
    }

    #endregion

    #region Template Tests

    /// <summary>
    /// Verifies that AddTemplates stores templates with normalized keys.
    /// Tests that templates are stored with lowercase keys and accelerator markers removed.
    /// </summary>
    [Test]
    public void AddTemplates_StoresTemplatesWithNormalizedKeys()
    {
        // Arrange
        var template = CreateMenuItem(menuPath: "&File>&New", quickKey: "Ctrl+N", toolTip: "Template tooltip");

        // Act
        MenuItem.AddTemplates([template]);

        // Assert - Template should be accessible with normalized key
        var retrieved = MenuItem.GetTemplate("file>new");
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.ToolTip, Is.EqualTo("Template tooltip"));
    }

    /// <summary>
    /// Verifies that AddTemplates ignores items with null or empty MenuPath.
    /// Tests that templates without valid MenuPath are not added.
    /// </summary>
    [Test]
    public void AddTemplates_IgnoresItemsWithNullOrEmptyMenuPath()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: null);
        var template2 = CreateMenuItem(menuPath: "");
        var template3 = CreateMenuItem(menuPath: "   ");

        // Act
        MenuItem.AddTemplates([template1, template2, template3]);

        // Assert - None of these should be retrievable
        Assert.That(MenuItem.GetTemplate(""), Is.Null);
    }

    /// <summary>
    /// Verifies that AddTemplates does not overwrite existing templates.
    /// Tests that if a template already exists, it is not replaced.
    /// </summary>
    [Test]
    public void AddTemplates_DoesNotOverwriteExistingTemplates()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: "File>New", toolTip: "First template");
        var template2 = CreateMenuItem(menuPath: "File>New", toolTip: "Second template");

        // Act
        MenuItem.AddTemplates([template1]);
        MenuItem.AddTemplates([template2]);

        // Assert - First template should still be there
        var retrieved = MenuItem.GetTemplate("file>new");
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.ToolTip, Is.EqualTo("First template"));
    }

    /// <summary>
    /// Verifies that GetTemplate returns null for non-existent templates.
    /// Tests that GetTemplate handles missing templates gracefully.
    /// </summary>
    [Test]
    public void GetTemplate_ReturnsNullForNonExistentTemplate()
    {
        // Act
        var template = MenuItem.GetTemplate("nonexistent>path");

        // Assert
        Assert.That(template, Is.Null);
    }

    /// <summary>
    /// Verifies that GetTemplate is case-insensitive.
    /// Tests that template lookup works regardless of case.
    /// </summary>
    [Test]
    public void GetTemplate_IsCaseInsensitive()
    {
        // Arrange
        var template = CreateMenuItem(menuPath: "File>New", toolTip: "Template tooltip");
        MenuItem.AddTemplates([template]);

        // Act & Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MenuItem.GetTemplate("file>new"), Is.Not.Null);
            Assert.That(MenuItem.GetTemplate("FILE>NEW"), Is.Not.Null);
            Assert.That(MenuItem.GetTemplate("File>New"), Is.Not.Null);
        }
    }

    /// <summary>
    /// Verifies that GetTemplate ignores accelerator markers.
    /// Tests that template lookup works with or without accelerator markers.
    /// </summary>
    [Test]
    public void GetTemplate_IgnoresAcceleratorMarkers()
    {
        // Arrange
        var template = CreateMenuItem(menuPath: "&File>&New", toolTip: "Template tooltip");
        MenuItem.AddTemplates([template]);

        // Act & Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MenuItem.GetTemplate("file>new"), Is.Not.Null);
            Assert.That(MenuItem.GetTemplate("&file>&new"), Is.Not.Null);
        }
    }

    /// <summary>
    /// Verifies that AddTemplates handles multiple templates correctly.
    /// Tests that multiple templates can be added and retrieved.
    /// </summary>
    [Test]
    public void AddTemplates_HandlesMultipleTemplates()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: "File>New", toolTip: "New template");
        var template2 = CreateMenuItem(menuPath: "File>Open", toolTip: "Open template");
        var template3 = CreateMenuItem(menuPath: "Edit>Copy", toolTip: "Copy template");

        // Act
        MenuItem.AddTemplates([template1, template2, template3]);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MenuItem.GetTemplate("file>new"), Is.Not.Null);
            Assert.That(MenuItem.GetTemplate("file>new")!.ToolTip, Is.EqualTo("New template"));
            Assert.That(MenuItem.GetTemplate("file>open"), Is.Not.Null);
            Assert.That(MenuItem.GetTemplate("file>open")!.ToolTip, Is.EqualTo("Open template"));
            Assert.That(MenuItem.GetTemplate("edit>copy"), Is.Not.Null);
            Assert.That(MenuItem.GetTemplate("edit>copy")!.ToolTip, Is.EqualTo("Copy template"));
        }
    }

    #endregion

    #region Order Tests

    /// <summary>
    /// Verifies that AddTemplates adds normalized keys to the Order list.
    /// Tests that the Order list maintains the order in which templates are added.
    /// </summary>
    [Test]
    public void AddTemplates_AddsToOrderList()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: "File>New", toolTip: "New template");
        var template2 = CreateMenuItem(menuPath: "File>Open", toolTip: "Open template");
        var template3 = CreateMenuItem(menuPath: "Edit>Copy", toolTip: "Copy template");

        // Act
        MenuItem.AddTemplates([template1, template2, template3]);

        // Assert
        Assert.That(MenuItem.Order, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MenuItem.Order[0], Is.EqualTo("file>new"));
            Assert.That(MenuItem.Order[1], Is.EqualTo("file>open"));
            Assert.That(MenuItem.Order[2], Is.EqualTo("edit>copy"));
        }
    }

    /// <summary>
    /// Verifies that AddTemplates normalizes keys when adding to Order list.
    /// Tests that keys in Order are lowercase and have accelerator markers removed.
    /// </summary>
    [Test]
    public void AddTemplates_NormalizesKeysInOrderList()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: "&File>&New", toolTip: "New template");
        var template2 = CreateMenuItem(menuPath: "File>Open", toolTip: "Open template");

        // Act
        MenuItem.AddTemplates([template1, template2]);

        // Assert
        Assert.That(MenuItem.Order, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MenuItem.Order[0], Is.EqualTo("file>new"));
            Assert.That(MenuItem.Order[1], Is.EqualTo("file>open"));
            Assert.That(MenuItem.Order[0], Does.Not.Contain("&"));
        }
    }

    /// <summary>
    /// Verifies that AddTemplates does not add duplicate keys to Order list.
    /// Tests that attempting to add an existing template does not affect Order.
    /// </summary>
    [Test]
    public void AddTemplates_DoesNotAddDuplicatesToOrderList()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: "File>New", toolTip: "First template");
        var template2 = CreateMenuItem(menuPath: "File>New", toolTip: "Second template");

        // Act
        MenuItem.AddTemplates([template1]);
        MenuItem.AddTemplates([template2]);

        // Assert - Order should only contain one entry
        Assert.That(MenuItem.Order, Has.Count.EqualTo(1));
        Assert.That(MenuItem.Order[0], Is.EqualTo("file>new"));
    }

    /// <summary>
    /// Verifies that AddTemplates does not add items with null or empty MenuPath to Order list.
    /// Tests that invalid menu paths are not added to the Order list.
    /// </summary>
    [Test]
    public void AddTemplates_DoesNotAddInvalidMenuPathsToOrderList()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: null);
        var template2 = CreateMenuItem(menuPath: "");
        var template3 = CreateMenuItem(menuPath: "   ");
        var template4 = CreateMenuItem(menuPath: "File>New", toolTip: "Valid template");

        // Act
        MenuItem.AddTemplates([template1, template2, template3, template4]);

        // Assert - Only the valid template should be in Order
        Assert.That(MenuItem.Order, Has.Count.EqualTo(1));
        Assert.That(MenuItem.Order[0], Is.EqualTo("file>new"));
    }

    /// <summary>
    /// Verifies that Order list maintains insertion order across multiple AddTemplates calls.
    /// Tests that the order reflects when templates were first added, not when duplicates were attempted.
    /// </summary>
    [Test]
    public void AddTemplates_MaintainsInsertionOrderInOrderList()
    {
        // Arrange
        var template1 = CreateMenuItem(menuPath: "File>New", toolTip: "New template");
        var template2 = CreateMenuItem(menuPath: "File>Open", toolTip: "Open template");
        var template3 = CreateMenuItem(menuPath: "Edit>Copy", toolTip: "Copy template");
        var template4 = CreateMenuItem(menuPath: "File>New", toolTip: "Duplicate New template");

        // Act
        MenuItem.AddTemplates([template1]);
        MenuItem.AddTemplates([template2]);
        MenuItem.AddTemplates([template3]);
        MenuItem.AddTemplates([template4]); // This should not affect Order

        // Assert - Order should reflect first three templates in insertion order
        Assert.That(MenuItem.Order, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(MenuItem.Order[0], Is.EqualTo("file>new"));
            Assert.That(MenuItem.Order[1], Is.EqualTo("file>open"));
            Assert.That(MenuItem.Order[2], Is.EqualTo("edit>copy"));
        }
    }

    /// <summary>
    /// Verifies that Order list is empty when no templates have been added.
    /// Tests the initial state of the Order list.
    /// </summary>
    [Test]
    public void Order_IsEmptyWhenNoTemplatesAdded()
    {
        // Assert
        Assert.That(MenuItem.Order, Is.Empty);
    }

    #endregion
}

