using NUnit.Framework;
using System.Drawing;
using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test class for CoreExtensions functionality.
/// Tests the UnionWith extension method for RectangleF.
/// </summary>
[TestFixture]
public class CoreExtensionsTests
{
    #region Test Data

    /// <summary>
    /// Tolerance for floating-point comparisons to account for precision differences.
    /// </summary>
    private const float Tolerance = 0.001f;

    #endregion

    #region Basic Union Tests

    /// <summary>
    /// Tests that UnionWith correctly unions two non-overlapping rectangles.
    /// </summary>
    [Test]
    public void UnionWith_NonOverlappingRectangles_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 10, 10);
        var rect2 = new RectangleF(20, 20, 10, 10);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(30f).Within(Tolerance), "Width should span from 0 to 30");
            Assert.That(result.Height, Is.EqualTo(30f).Within(Tolerance), "Height should span from 0 to 30");
        }
    }

    /// <summary>
    /// Tests that UnionWith correctly unions two overlapping rectangles.
    /// </summary>
    [Test]
    public void UnionWith_OverlappingRectangles_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 20, 20);
        var rect2 = new RectangleF(10, 10, 20, 20);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(30f).Within(Tolerance), "Width should span from 0 to 30");
            Assert.That(result.Height, Is.EqualTo(30f).Within(Tolerance), "Height should span from 0 to 30");
        }
    }

    /// <summary>
    /// Tests that UnionWith correctly unions when one rectangle is completely inside another.
    /// </summary>
    [Test]
    public void UnionWith_OneRectangleInsideAnother_ShouldReturnOuterRectangle()
    {
        // Arrange
        var outer = new RectangleF(0, 0, 100, 100);
        var inner = new RectangleF(25, 25, 50, 50);

        // Act
        var result = outer.UnionWith(inner);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should match outer rectangle");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should match outer rectangle");
            Assert.That(result.Width, Is.EqualTo(100f).Within(Tolerance), "Width should match outer rectangle");
            Assert.That(result.Height, Is.EqualTo(100f).Within(Tolerance), "Height should match outer rectangle");
        }
    }

    /// <summary>
    /// Tests that UnionWith correctly unions when rectangles are identical.
    /// </summary>
    [Test]
    public void UnionWith_IdenticalRectangles_ShouldReturnSameRectangle()
    {
        // Arrange
        var rect = new RectangleF(10, 20, 30, 40);

        // Act
        var result = rect.UnionWith(rect);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(rect.X).Within(Tolerance));
            Assert.That(result.Y, Is.EqualTo(rect.Y).Within(Tolerance));
            Assert.That(result.Width, Is.EqualTo(rect.Width).Within(Tolerance));
            Assert.That(result.Height, Is.EqualTo(rect.Height).Within(Tolerance));
        }
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests that UnionWith handles rectangles with negative coordinates.
    /// </summary>
    [Test]
    public void UnionWith_NegativeCoordinates_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(-10, -10, 10, 10);
        var rect2 = new RectangleF(0, 0, 10, 10);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(-10f).Within(Tolerance), "X should be the leftmost coordinate (-10)");
            Assert.That(result.Y, Is.EqualTo(-10f).Within(Tolerance), "Y should be the topmost coordinate (-10)");
            Assert.That(result.Width, Is.EqualTo(20f).Within(Tolerance), "Width should span from -10 to 10");
            Assert.That(result.Height, Is.EqualTo(20f).Within(Tolerance), "Height should span from -10 to 10");
        }
    }

    /// <summary>
    /// Tests that UnionWith handles rectangles with zero width or height.
    /// </summary>
    [Test]
    public void UnionWith_ZeroWidthOrHeight_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 0, 10);  // Zero width
        var rect2 = new RectangleF(10, 0, 10, 0); // Zero height

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(20f).Within(Tolerance), "Width should span from 0 to 20");
            Assert.That(result.Height, Is.EqualTo(10f).Within(Tolerance), "Height should span from 0 to 10");
        }
    }

    /// <summary>
    /// Tests that UnionWith handles empty rectangles.
    /// </summary>
    [Test]
    public void UnionWith_EmptyRectangles_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 0, 0);
        var rect2 = new RectangleF(10, 10, 0, 0);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(10f).Within(Tolerance), "Width should span from 0 to 10");
            Assert.That(result.Height, Is.EqualTo(10f).Within(Tolerance), "Height should span from 0 to 10");
        }
    }

    /// <summary>
    /// Tests that UnionWith handles rectangles that only touch at edges.
    /// </summary>
    [Test]
    public void UnionWith_TouchingRectangles_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 10, 10);
        var rect2 = new RectangleF(10, 0, 10, 10); // Touches at right edge

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(20f).Within(Tolerance), "Width should span from 0 to 20");
            Assert.That(result.Height, Is.EqualTo(10f).Within(Tolerance), "Height should match both rectangles");
        }
    }

    #endregion

    #region Position Tests

    /// <summary>
    /// Tests that UnionWith correctly handles rectangles at different positions.
    /// </summary>
    [Test]
    public void UnionWith_DifferentPositions_ShouldCreateCorrectUnion()
    {
        // Arrange
        var rect1 = new RectangleF(5, 10, 15, 20);
        var rect2 = new RectangleF(25, 35, 10, 15);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(5f).Within(Tolerance), "X should be the leftmost coordinate (5)");
            Assert.That(result.Y, Is.EqualTo(10f).Within(Tolerance), "Y should be the topmost coordinate (10)");
            Assert.That(result.Width, Is.EqualTo(30f).Within(Tolerance), "Width should span from 5 to 35");
            Assert.That(result.Height, Is.EqualTo(40f).Within(Tolerance), "Height should span from 10 to 50");
        }
    }

    /// <summary>
    /// Tests that UnionWith correctly handles when second rectangle is to the left.
    /// </summary>
    [Test]
    public void UnionWith_SecondRectangleToLeft_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(20, 10, 10, 10);
        var rect2 = new RectangleF(0, 10, 10, 10);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate (0)");
            Assert.That(result.Y, Is.EqualTo(10f).Within(Tolerance), "Y should be the topmost coordinate (10)");
            Assert.That(result.Width, Is.EqualTo(30f).Within(Tolerance), "Width should span from 0 to 30");
            Assert.That(result.Height, Is.EqualTo(10f).Within(Tolerance), "Height should match both rectangles");
        }
    }

    /// <summary>
    /// Tests that UnionWith correctly handles when second rectangle is above.
    /// </summary>
    [Test]
    public void UnionWith_SecondRectangleAbove_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(10, 20, 10, 10);
        var rect2 = new RectangleF(10, 0, 10, 10);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(10f).Within(Tolerance), "X should match both rectangles");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate (0)");
            Assert.That(result.Width, Is.EqualTo(10f).Within(Tolerance), "Width should match both rectangles");
            Assert.That(result.Height, Is.EqualTo(30f).Within(Tolerance), "Height should span from 0 to 30");
        }
    }

    #endregion

    #region Size Tests

    /// <summary>
    /// Tests that UnionWith correctly handles rectangles of different sizes.
    /// </summary>
    [Test]
    public void UnionWith_DifferentSizes_ShouldCreateUnion()
    {
        // Arrange
        var small = new RectangleF(10, 10, 5, 5);
        var large = new RectangleF(0, 0, 50, 50);

        // Act
        var result = small.UnionWith(large);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(50f).Within(Tolerance), "Width should match large rectangle");
            Assert.That(result.Height, Is.EqualTo(50f).Within(Tolerance), "Height should match large rectangle");
        }
    }

    /// <summary>
    /// Tests that UnionWith correctly handles very large rectangles.
    /// </summary>
    [Test]
    public void UnionWith_VeryLargeRectangles_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 10000, 10000);
        var rect2 = new RectangleF(5000, 5000, 10000, 10000);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(15000f).Within(Tolerance), "Width should span from 0 to 15000");
            Assert.That(result.Height, Is.EqualTo(15000f).Within(Tolerance), "Height should span from 0 to 15000");
        }
    }

    /// <summary>
    /// Tests that UnionWith correctly handles very small rectangles.
    /// </summary>
    [Test]
    public void UnionWith_VerySmallRectangles_ShouldCreateUnion()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 0.1f, 0.1f);
        var rect2 = new RectangleF(0.2f, 0.2f, 0.1f, 0.1f);

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.X, Is.EqualTo(0f).Within(Tolerance), "X should be the leftmost coordinate");
            Assert.That(result.Y, Is.EqualTo(0f).Within(Tolerance), "Y should be the topmost coordinate");
            Assert.That(result.Width, Is.EqualTo(0.3f).Within(Tolerance), "Width should span from 0 to 0.3");
            Assert.That(result.Height, Is.EqualTo(0.3f).Within(Tolerance), "Height should span from 0 to 0.3");
        }
    }

    #endregion

    #region Commutativity Tests

    /// <summary>
    /// Tests that UnionWith is commutative (A.UnionWith(B) == B.UnionWith(A)).
    /// </summary>
    [Test]
    public void UnionWith_ShouldBeCommutative()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 10, 10);
        var rect2 = new RectangleF(20, 20, 10, 10);

        // Act
        var result1 = rect1.UnionWith(rect2);
        var result2 = rect2.UnionWith(rect1);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1.X, Is.EqualTo(result2.X).Within(Tolerance), "X should be the same regardless of order");
            Assert.That(result1.Y, Is.EqualTo(result2.Y).Within(Tolerance), "Y should be the same regardless of order");
            Assert.That(result1.Width, Is.EqualTo(result2.Width).Within(Tolerance), "Width should be the same regardless of order");
            Assert.That(result1.Height, Is.EqualTo(result2.Height).Within(Tolerance), "Height should be the same regardless of order");
        }
    }

    #endregion

    #region Boundary Tests

    /// <summary>
    /// Tests that UnionWith correctly calculates Right boundary.
    /// </summary>
    [Test]
    public void UnionWith_ShouldCalculateRightBoundaryCorrectly()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 10, 10);   // Right = 10
        var rect2 = new RectangleF(15, 0, 10, 10);  // Right = 25

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        Assert.That(result.Right, Is.EqualTo(25f).Within(Tolerance), "Right should be the rightmost boundary (25)");
    }

    /// <summary>
    /// Tests that UnionWith correctly calculates Bottom boundary.
    /// </summary>
    [Test]
    public void UnionWith_ShouldCalculateBottomBoundaryCorrectly()
    {
        // Arrange
        var rect1 = new RectangleF(0, 0, 10, 10);    // Bottom = 10
        var rect2 = new RectangleF(0, 15, 10, 10);  // Bottom = 25

        // Act
        var result = rect1.UnionWith(rect2);

        // Assert
        Assert.That(result.Bottom, Is.EqualTo(25f).Within(Tolerance), "Bottom should be the bottommost boundary (25)");
    }

    #endregion
}
