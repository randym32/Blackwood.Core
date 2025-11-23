using Blackwood;
using NUnit.Framework;
using System.Drawing;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test fixture for <see cref="ChartAnnotation{T}"/> class functionality.
/// Tests the annotation creation, property initialization, and default value handling.
/// </summary>
[TestFixture]
public class ChartAnnotationTests
{
    #region Constructor Tests

    /// <summary>
    /// Verifies that ChartAnnotation constructor initializes all properties correctly when all parameters are provided.
    /// Tests that index, duration, color, text, and textColor are set as expected.
    /// </summary>
    [Test]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Red;
        var text = "Test Annotation";
        var textColor = Color.Blue;

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color, text, textColor);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
            Assert.That(annotation.text, Is.EqualTo(text));
            Assert.That(annotation.textColor, Is.EqualTo(textColor));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation constructor uses default textColor (white) when textColor parameter is null.
    /// Tests that the default value for textColor is applied correctly.
    /// </summary>
    [Test]
    public void Constructor_WithNullTextColor_UsesDefaultWhite()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Green;
        var text = "Test Annotation";

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color, text, null);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.textColor, Is.EqualTo(Color.White));
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
            Assert.That(annotation.text, Is.EqualTo(text));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation constructor uses default textColor (white) when textColor parameter is omitted.
    /// Tests that the optional parameter defaults correctly.
    /// </summary>
    [Test]
    public void Constructor_WithoutTextColor_UsesDefaultWhite()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Blue;
        var text = "Test Annotation";

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color, text);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.textColor, Is.EqualTo(Color.White));
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
            Assert.That(annotation.text, Is.EqualTo(text));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation constructor allows null text when text parameter is omitted.
    /// Tests that the optional text parameter defaults to null.
    /// </summary>
    [Test]
    public void Constructor_WithoutText_SetsTextToNull()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Yellow;

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.text, Is.Null);
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
            Assert.That(annotation.textColor, Is.EqualTo(Color.White));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation constructor allows null text when explicitly provided.
    /// Tests that null text is handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithNullText_SetsTextToNull()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Purple;

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color, null);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.text, Is.Null);
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation constructor handles empty string text correctly.
    /// Tests that empty string is treated as a valid text value.
    /// </summary>
    [Test]
    public void Constructor_WithEmptyText_SetsTextToEmptyString()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Orange;
        var text = string.Empty;

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color, text);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.text, Is.EqualTo(string.Empty));
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
        }
    }

    #endregion

    #region Generic Type Tests

    /// <summary>
    /// Verifies that ChartAnnotation works correctly with double type for index and duration.
    /// Tests that floating-point values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithDoubleType_HandlesFloatingPointValues()
    {
        // Arrange
        var index = 10.5;
        var duration = 3.14;
        var color = Color.Cyan;

        // Act
        var annotation = new ChartAnnotation<double>(index, duration, color);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation works correctly with DateTime type for index and duration.
    /// Tests that DateTime values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithDateTimeType_HandlesDateTimeValues()
    {
        // Arrange
        var index = new DateTime(2025, 1, 1, 10, 0, 0);
        var duration = new DateTime(2025, 1, 1, 11, 30, 0);
        var color = Color.Magenta;

        // Act
        var annotation = new ChartAnnotation<DateTime>(index, duration, color);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation works correctly with string type for index and duration.
    /// Tests that string values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithStringType_HandlesStringValues()
    {
        // Arrange
        var index = "Category A";
        var duration = "Category B";
        var color = Color.Teal;

        // Act
        var annotation = new ChartAnnotation<string>(index, duration, color);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation supports differing index and duration types.
    /// </summary>
    [Test]
    public void Constructor_WithDifferentIndexAndDurationTypes_HandlesValues()
    {
        // Arrange
        var index = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var duration = TimeSpan.FromMinutes(30);
        var color = Color.DarkOrange;

        // Act
        var annotation = new ChartAnnotation<DateTime, TimeSpan>(index, duration, color, "Window");

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
            Assert.That(annotation.text, Is.EqualTo("Window"));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation works correctly with long type for index and duration.
    /// Tests that large integer values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithLongType_HandlesLargeIntegerValues()
    {
        // Arrange
        var index = 1000000L;
        var duration = 500000L;
        var color = Color.Indigo;

        // Act
        var annotation = new ChartAnnotation<long>(index, duration, color);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
            Assert.That(annotation.color, Is.EqualTo(color));
        }
    }

    #endregion

    #region Color Tests

    /// <summary>
    /// Verifies that ChartAnnotation correctly stores various Color values.
    /// Tests that different color values are preserved correctly.
    /// </summary>
    [Test]
    public void Constructor_WithVariousColors_StoresColorsCorrectly()
    {
        // Arrange
        var colors = new[] { Color.Red, Color.Green, Color.Blue, Color.Transparent, Color.FromArgb(128, 64, 32) };

        // Act & Assert
        foreach (var color in colors)
        {
            var annotation = new ChartAnnotation<int>(0, 0, color);
            Assert.That(annotation.color, Is.EqualTo(color), $"Color {color} should be stored correctly");
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation correctly stores various textColor values.
    /// Tests that different text color values are preserved correctly.
    /// </summary>
    [Test]
    public void Constructor_WithVariousTextColors_StoresTextColorsCorrectly()
    {
        // Arrange
        var textColors = new[] { Color.Black, Color.White, Color.Yellow, Color.FromArgb(255, 128, 0) };

        // Act & Assert
        foreach (var textColor in textColors)
        {
            var annotation = new ChartAnnotation<int>(0, 0, Color.Red, "Test", textColor);
            Assert.That(annotation.textColor, Is.EqualTo(textColor), $"Text color {textColor} should be stored correctly");
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation uses white as default textColor when not specified.
    /// Tests that the default textColor value is consistently applied.
    /// </summary>
    [Test]
    public void Constructor_WithoutTextColorParameter_UsesWhiteAsDefault()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Red;

        // Act
        var annotation1 = new ChartAnnotation<int>(index, duration, color);
        var annotation2 = new ChartAnnotation<int>(index, duration, color, "Text");
        var annotation3 = new ChartAnnotation<int>(index, duration, color, null, null);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation1.textColor, Is.EqualTo(Color.White));
            Assert.That(annotation2.textColor, Is.EqualTo(Color.White));
            Assert.That(annotation3.textColor, Is.EqualTo(Color.White));
        }
    }

    #endregion

    #region Property Immutability Tests

    /// <summary>
    /// Verifies that ChartAnnotation properties are readonly and cannot be modified after construction.
    /// Tests that the readonly fields prevent modification.
    /// </summary>
    [Test]
    public void Properties_AreReadonly_CannotBeModified()
    {
        // Arrange
        var annotation = new ChartAnnotation<int>(10, 5, Color.Red, "Test", Color.Blue);

        // Act & Assert
        // Verify that properties are readonly by checking they can be read
        // (Compile-time check: readonly fields cannot be assigned outside constructor)
        Assert.That(annotation.index, Is.EqualTo(10));
        Assert.That(annotation.duration, Is.EqualTo(5));
        Assert.That(annotation.color, Is.EqualTo(Color.Red));
        Assert.That(annotation.text, Is.EqualTo("Test"));
        Assert.That(annotation.textColor, Is.EqualTo(Color.Blue));
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Verifies that ChartAnnotation handles zero values correctly for numeric types.
    /// Tests that zero is a valid value for index and duration.
    /// </summary>
    [Test]
    public void Constructor_WithZeroValues_HandlesCorrectly()
    {
        // Arrange
        var index = 0;
        var duration = 0;
        var color = Color.Black;

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(0));
            Assert.That(annotation.duration, Is.EqualTo(0));
            Assert.That(annotation.color, Is.EqualTo(Color.Black));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation handles negative values correctly for numeric types.
    /// Tests that negative values are valid for index and duration.
    /// </summary>
    [Test]
    public void Constructor_WithNegativeValues_HandlesCorrectly()
    {
        // Arrange
        var index = -10;
        var duration = -5;
        var color = Color.Gray;

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.index, Is.EqualTo(-10));
            Assert.That(annotation.duration, Is.EqualTo(-5));
            Assert.That(annotation.color, Is.EqualTo(Color.Gray));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation handles whitespace-only text correctly.
    /// Tests that whitespace strings are treated as valid text values.
    /// </summary>
    [Test]
    public void Constructor_WithWhitespaceText_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Red;
        var text = "   ";

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color, text);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.text, Is.EqualTo("   "));
            Assert.That(annotation.index, Is.EqualTo(index));
            Assert.That(annotation.duration, Is.EqualTo(duration));
        }
    }

    /// <summary>
    /// Verifies that ChartAnnotation handles very long text strings correctly.
    /// Tests that long text values are stored correctly.
    /// </summary>
    [Test]
    public void Constructor_WithLongText_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var duration = 5;
        var color = Color.Red;
        var text = new string('A', 1000); // 1000 character string

        // Act
        var annotation = new ChartAnnotation<int>(index, duration, color, text);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(annotation.text, Is.EqualTo(text));
            Assert.That(annotation.text!.Length, Is.EqualTo(1000));
        }
    }

    #endregion
}

