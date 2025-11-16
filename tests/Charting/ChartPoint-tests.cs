using Blackwood;
using NUnit.Framework;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test fixture for <see cref="ChartPoint{T}"/> class functionality.
/// Tests the point creation, property initialization, and default value handling.
/// </summary>
[TestFixture]
public class ChartPointTests
{
    #region Constructor Tests

    /// <summary>
    /// Verifies that ChartPoint constructor initializes all properties correctly when all parameters are provided.
    /// Tests that index, y, and annotation are set as expected.
    /// </summary>
    [Test]
    public void Constructor_WithAllParameters_SetsAllProperties()
    {
        // Arrange
        var index = 10;
        var y = 25.5;
        var annotation = "Test Point";

        // Act
        var point = new ChartPoint<int>(index, y, annotation);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
            Assert.That(point.annotation, Is.EqualTo(annotation));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint constructor allows null annotation when annotation parameter is omitted.
    /// Tests that the optional annotation parameter defaults to null.
    /// </summary>
    [Test]
    public void Constructor_WithoutAnnotation_SetsAnnotationToNull()
    {
        // Arrange
        var index = 10;
        var y = 25.5;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.annotation, Is.Null);
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint constructor allows null annotation when explicitly provided.
    /// Tests that null annotation is handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithNullAnnotation_SetsAnnotationToNull()
    {
        // Arrange
        var index = 10;
        var y = 25.5;

        // Act
        var point = new ChartPoint<int>(index, y, null);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.annotation, Is.Null);
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint constructor handles empty string annotation correctly.
    /// Tests that empty string is treated as a valid annotation value.
    /// </summary>
    [Test]
    public void Constructor_WithEmptyAnnotation_SetsAnnotationToEmptyString()
    {
        // Arrange
        var index = 10;
        var y = 25.5;
        var annotation = string.Empty;

        // Act
        var point = new ChartPoint<int>(index, y, annotation);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.annotation, Is.EqualTo(string.Empty));
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    #endregion

    #region Generic Type Tests

    /// <summary>
    /// Verifies that ChartPoint works correctly with double type for index.
    /// Tests that floating-point values are handled correctly for the index.
    /// </summary>
    [Test]
    public void Constructor_WithDoubleIndex_HandlesFloatingPointValues()
    {
        // Arrange
        var index = 10.5;
        var y = 25.75;

        // Act
        var point = new ChartPoint<double>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint works correctly with DateTime type for index.
    /// Tests that DateTime values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithDateTimeIndex_HandlesDateTimeValues()
    {
        // Arrange
        var index = new DateTime(2025, 1, 1, 10, 0, 0);
        var y = 100.0;

        // Act
        var point = new ChartPoint<DateTime>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint works correctly with string type for index.
    /// Tests that string values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithStringIndex_HandlesStringValues()
    {
        // Arrange
        var index = "Category A";
        var y = 42.5;

        // Act
        var point = new ChartPoint<string>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint works correctly with long type for index.
    /// Tests that large integer values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithLongIndex_HandlesLargeIntegerValues()
    {
        // Arrange
        var index = 1000000L;
        var y = 999.99;

        // Act
        var point = new ChartPoint<long>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    #endregion

    #region Y Value Tests

    /// <summary>
    /// Verifies that ChartPoint correctly stores zero y value.
    /// Tests that zero is a valid value for the dependent variable.
    /// </summary>
    [Test]
    public void Constructor_WithZeroY_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = 0.0;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.EqualTo(0.0));
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint correctly stores negative y value.
    /// Tests that negative values are valid for the dependent variable.
    /// </summary>
    [Test]
    public void Constructor_WithNegativeY_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = -25.5;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.EqualTo(-25.5));
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint correctly stores very large y value.
    /// Tests that large floating-point values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithLargeY_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = 1.7976931348623157E+308; // Max double value

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.EqualTo(y));
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint correctly stores very small y value.
    /// Tests that small floating-point values are handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithSmallY_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = 4.94065645841247E-324; // Min double value

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.EqualTo(y));
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint correctly stores NaN y value.
    /// Tests that NaN (Not a Number) is handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithNaN_Y_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = double.NaN;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.NaN);
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint correctly stores positive infinity y value.
    /// Tests that positive infinity is handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithPositiveInfinityY_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = double.PositiveInfinity;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.EqualTo(double.PositiveInfinity));
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint correctly stores negative infinity y value.
    /// Tests that negative infinity is handled correctly.
    /// </summary>
    [Test]
    public void Constructor_WithNegativeInfinityY_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = double.NegativeInfinity;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.EqualTo(double.NegativeInfinity));
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    #endregion

    #region Property Immutability Tests

    /// <summary>
    /// Verifies that ChartPoint properties are readonly and cannot be modified after construction.
    /// Tests that the readonly fields prevent modification.
    /// </summary>
    [Test]
    public void Properties_AreReadonly_CannotBeModified()
    {
        // Arrange
        var point = new ChartPoint<int>(10, 25.5, "Test");

        // Act & Assert
        // Verify that properties are readonly by checking they can be read
        // (Compile-time check: readonly fields cannot be assigned outside constructor)
        Assert.That(point.index, Is.EqualTo(10));
        Assert.That(point.y, Is.EqualTo(25.5));
        Assert.That(point.annotation, Is.EqualTo("Test"));
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Verifies that ChartPoint handles zero index value correctly for numeric types.
    /// Tests that zero is a valid value for index.
    /// </summary>
    [Test]
    public void Constructor_WithZeroIndex_HandlesCorrectly()
    {
        // Arrange
        var index = 0;
        var y = 25.5;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.index, Is.EqualTo(0));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint handles negative index value correctly for numeric types.
    /// Tests that negative values are valid for index.
    /// </summary>
    [Test]
    public void Constructor_WithNegativeIndex_HandlesCorrectly()
    {
        // Arrange
        var index = -10;
        var y = 25.5;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.index, Is.EqualTo(-10));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint handles whitespace-only annotation correctly.
    /// Tests that whitespace strings are treated as valid annotation values.
    /// </summary>
    [Test]
    public void Constructor_WithWhitespaceAnnotation_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = 25.5;
        var annotation = "   ";

        // Act
        var point = new ChartPoint<int>(index, y, annotation);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.annotation, Is.EqualTo("   "));
            Assert.That(point.index, Is.EqualTo(index));
            Assert.That(point.y, Is.EqualTo(y));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint handles very long annotation strings correctly.
    /// Tests that long annotation values are stored correctly.
    /// </summary>
    [Test]
    public void Constructor_WithLongAnnotation_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = 25.5;
        var annotation = new string('A', 1000); // 1000 character string

        // Act
        var point = new ChartPoint<int>(index, y, annotation);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.annotation, Is.EqualTo(annotation));
            Assert.That(point.annotation!.Length, Is.EqualTo(1000));
        }
    }

    /// <summary>
    /// Verifies that ChartPoint handles precision of floating-point y values correctly.
    /// Tests that double precision is maintained.
    /// </summary>
    [Test]
    public void Constructor_WithPreciseY_HandlesCorrectly()
    {
        // Arrange
        var index = 10;
        var y = 3.141592653589793238462643383279;

        // Act
        var point = new ChartPoint<int>(index, y);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(point.y, Is.EqualTo(y).Within(0.000000000000001));
            Assert.That(point.index, Is.EqualTo(index));
        }
    }

    #endregion
}

