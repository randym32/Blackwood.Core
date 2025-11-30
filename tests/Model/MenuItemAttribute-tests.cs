using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test suite for <see cref="MenuItemAttribute"/> ensuring constructor validation,
/// property behavior, and helper conversion logic function as expected.
/// </summary>
[TestFixture]
public class MenuItemAttributeTests
{
    /// <summary>
    /// Verifies that the constructor populates all read-only properties when valid values are provided.
    /// </summary>
    [Test]
    public void Constructor_WithValidArguments_SetsProperties()
    {
        // Arrange & Act
        var attribute = new MenuItemAttribute(
            "File>New",
            quickKey: "Ctrl+N",
            toolTip: "Create a new file",
            separatorBefore: true,
            disabled: true);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attribute.MenuPath, Is.EqualTo("File>New"));
            Assert.That(attribute.QuickKey, Is.EqualTo("Ctrl+N"));
            Assert.That(attribute.ToolTip, Is.EqualTo("Create a new file"));
            Assert.That(attribute.SeparatorBefore, Is.True);
            Assert.That(attribute.Disabled, Is.True);
        }
    }

    /// <summary>
    /// Ensures that invalid menu path values (null/empty/whitespace) throw an <see cref="ArgumentException"/>.
    /// </summary>
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Constructor_WithInvalidMenuPath_ThrowsArgumentException(string? menuPath)
    {
        // Act & Assert
        Assert.That(() => new MenuItemAttribute(menuPath!), Throws.TypeOf<ArgumentException>());
    }

    /// <summary>
    /// Verifies that the mutable Tag property can be assigned after construction.
    /// </summary>
    [Test]
    public void TagProperty_CanBeSetAndRetrieved()
    {
        // Arrange
        var attribute = new MenuItemAttribute("File>Open");

        // Act
        attribute.Tag = "custom-tag";

        // Assert
        Assert.That(attribute.Tag, Is.EqualTo("custom-tag"));
    }

    /// <summary>
    /// Ensures the <see cref="MenuItemAttribute.MenuItem"/> helper produces a <see cref="MenuItem"/> with matching metadata.
    /// </summary>
    [Test]
    public void MenuItem_ReturnsMenuItemWithMatchingMetadata()
    {
        // Arrange
        var attribute = new MenuItemAttribute(
            "File>Open",
            quickKey: "Ctrl+O",
            toolTip: "Open an existing file",
            separatorBefore: true,
            disabled: true)
        {
            Tag = "file-open"
        };

        // Act
        var menuItem = attribute.MenuItem();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(menuItem.MenuPath, Is.EqualTo("File>Open"));
            Assert.That(menuItem.QuickKey, Is.EqualTo("Ctrl+O"));
            Assert.That(menuItem.ToolTip, Is.EqualTo("Open an existing file"));
            Assert.That(menuItem.SeparatorBefore, Is.True);
            Assert.That(menuItem.Disabled, Is.True);
            Assert.That(menuItem.Tag, Is.EqualTo("file-open"));
        });
    }

    /// <summary>
    /// Confirms that optional values default to empty strings when converted to a <see cref="MenuItem"/>.
    /// </summary>
    [Test]
    public void MenuItem_WithNullOptionalValues_UsesEmptyStrings()
    {
        // Arrange
        var attribute = new MenuItemAttribute("Help>About");

        // Act
        var menuItem = attribute.MenuItem();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(menuItem.QuickKey, Is.EqualTo(string.Empty));
            Assert.That(menuItem.ToolTip, Is.EqualTo(string.Empty));
            Assert.That(menuItem.Tag, Is.EqualTo(string.Empty));
        });
    }
}

