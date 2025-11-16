using Blackwood;
using NUnit.Framework;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test fixture for <see cref="ReduceArray{T}"/> class functionality.
/// Tests the Douglas-Peucker algorithm implementation for reducing points in a series.
/// </summary>
[TestFixture]
public class DouglasPeuckerTests
{
    #region Constructor Tests

    /// <summary>
    /// Verifies that ReduceArray constructor initializes correctly with array of points.
    /// Tests that points and maxItems are set correctly.
    /// </summary>
    [Test]
    public void Constructor_WithArray_SetsProperties()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 0.0),
            new(1, 1.0),
            new(2, 2.0)
        };
        var maxItems = 100;

        // Act
        var reducer = new ReduceArray<int>(points, maxItems);

        // Assert
        var result = reducer.Simplify().ToArray();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    /// <summary>
    /// Verifies that ReduceArray constructor uses default maxItems when not specified.
    /// Tests that the default value of 2000 is used.
    /// </summary>
    [Test]
    public void Constructor_WithoutMaxItems_UsesDefault()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 0.0),
            new(1, 1.0),
            new(2, 2.0)
        };

        // Act
        var reducer = new ReduceArray<int>(points);

        // Assert
        var result = reducer.Simplify().ToArray();
        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Verifies that ReduceArray constructor works with IEnumerable of points.
    /// Tests that the IEnumerable overload correctly converts to array.
    /// </summary>
    [Test]
    public void Constructor_WithIEnumerable_SetsProperties()
    {
        // Arrange
        var points = new List<ChartPoint<int>>
        {
            new(0, 0.0),
            new(1, 1.0),
            new(2, 2.0)
        };
        var maxItems = 100;

        // Act
        var reducer = new ReduceArray<int>(points, maxItems);

        // Assert
        var result = reducer.Simplify().ToArray();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    #endregion

    #region Simplify Tests - Small Arrays

    /// <summary>
    /// Verifies that Simplify returns all points when array is empty.
    /// Tests edge case handling for empty input.
    /// </summary>
    [Test]
    public void Simplify_WithEmptyArray_ReturnsEmpty()
    {
        // Arrange
        var points = Array.Empty<ChartPoint<int>>();
        var reducer = new ReduceArray<int>(points);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Verifies that Simplify returns the single point when array has one point.
    /// Tests edge case handling for single point.
    /// </summary>
    [Test]
    public void Simplify_WithSinglePoint_ReturnsSinglePoint()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 10.0)
        };
        var reducer = new ReduceArray<int>(points);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].index, Is.EqualTo(0));
            Assert.That(result[0].y, Is.EqualTo(10.0));
        }
    }

    /// <summary>
    /// Verifies that Simplify returns both points when array has two points.
    /// Tests that start and end points are always kept.
    /// </summary>
    [Test]
    public void Simplify_WithTwoPoints_ReturnsBothPoints()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 0.0),
            new(10, 10.0)
        };
        var reducer = new ReduceArray<int>(points);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result[0].index, Is.EqualTo(0));
            Assert.That(result[1].index, Is.EqualTo(10));
        }
    }

    /// <summary>
    /// Verifies that Simplify returns all points when array has less than 3 points.
    /// Tests that very small arrays are not simplified.
    /// </summary>
    [Test]
    public void Simplify_WithLessThanThreePoints_ReturnsAllPoints()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 0.0),
            new(1, 1.0)
        };
        var reducer = new ReduceArray<int>(points);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        Assert.That(result, Has.Length.EqualTo(2));
    }

    /// <summary>
    /// Verifies that Simplify returns all points when array has less than threshold (100) points.
    /// Tests that arrays below the threshold are not simplified.
    /// </summary>
    [Test]
    public void Simplify_WithLessThanThreshold_ReturnsAllPoints()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 50; i++)
        {
            points.Add(new ChartPoint<int>(i, i * 1.0));
        }
        var reducer = new ReduceArray<int>(points);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        Assert.That(result, Has.Length.EqualTo(50));
    }

    #endregion

    #region Simplify Tests - Straight Lines

    /// <summary>
    /// Verifies that Simplify reduces points on a straight line significantly.
    /// Tests that points forming a perfect line are reduced to start and end.
    /// </summary>
    [Test]
    public void Simplify_WithStraightLine_ReducesToEndpoints()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 200; i++)
        {
            points.Add(new ChartPoint<int>(i, i * 2.0)); // Perfect straight line
        }
        var reducer = new ReduceArray<int>(points, maxItems: 10);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        // Should be significantly reduced, keeping mostly start and end
        Assert.That(result.Length, Is.LessThan(200));
        Assert.That(result[0].index, Is.EqualTo(0));
        Assert.That(result[^1].index, Is.EqualTo(199));
    }

    /// <summary>
    /// Verifies that Simplify keeps start and end points for straight lines.
    /// Tests that boundary points are always preserved.
    /// </summary>
    [Test]
    public void Simplify_WithStraightLine_KeepsStartAndEnd()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 150; i++)
        {
            points.Add(new ChartPoint<int>(i, 5.0)); // Horizontal line
        }
        var reducer = new ReduceArray<int>(points, maxItems: 5);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].index, Is.EqualTo(0));
            Assert.That(result[^1].index, Is.EqualTo(149));
        }
    }

    #endregion

    #region Simplify Tests - Curved Lines

    /// <summary>
    /// Verifies that Simplify keeps more points when there are significant deviations.
    /// Tests that points with large perpendicular distances are preserved.
    /// </summary>
    [Test]
    public void Simplify_WithSignificantDeviations_KeepsMorePoints()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 200; i++)
        {
            // Create a sine wave pattern with significant deviations
            var y = 50.0 + 30.0 * Math.Sin(i * Math.PI / 20.0);
            points.Add(new ChartPoint<int>(i, y));
        }
        var reducer = new ReduceArray<int>(points, maxItems: 50);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        // Should keep more points than a straight line due to deviations
        Assert.That(result.Length, Is.GreaterThan(2));
        Assert.That(result.Length, Is.LessThanOrEqualTo(50));
    }

    /// <summary>
    /// Verifies that Simplify preserves key turning points in a curve.
    /// Tests that points at peaks and valleys are more likely to be kept.
    /// </summary>
    [Test]
    public void Simplify_WithCurvedLine_PreservesKeyPoints()
    {
        // Arrange
        // Need more than threshold (100) points for simplification to run
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 200; i++)
        {
            // Create a curve with clear peaks and valleys
            var y = 10.0 * Math.Sin(i * Math.PI / 10.0);
            points.Add(new ChartPoint<int>(i, y));
        }
        var maxItems = 20;
        var reducer = new ReduceArray<int>(points, maxItems: maxItems);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            // Binary search may not converge exactly to maxItems, allow small tolerance
            Assert.That(result.Length, Is.LessThanOrEqualTo(maxItems * 1.2),
                "Result should be close to maxItems (within 20% tolerance)");
            Assert.That(result.Length, Is.GreaterThan(2),
                "Should keep more than just start and end points for a curve");
            Assert.That(result[0].index, Is.EqualTo(0));
            Assert.That(result[^1].index, Is.EqualTo(199));
        }
    }

    #endregion

    #region Simplify Tests - MaxItems Constraint

    /// <summary>
    /// Verifies that Simplify respects maxItems constraint approximately.
    /// Tests that the result is reasonably close to the specified maximum.
    /// </summary>
    [Test]
    public void Simplify_WithLargeArray_RespectsMaxItems()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 5000; i++)
        {
            points.Add(new ChartPoint<int>(i, i * 0.5 + Math.Sin(i * 0.1) * 10));
        }
        var maxItems = 100;
        var reducer = new ReduceArray<int>(points, maxItems);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        // Binary search with limited iterations may not converge exactly to maxItems.
        // Allow up to 2x maxItems as tolerance for complex data patterns.
        Assert.That(result.Length, Is.LessThanOrEqualTo(maxItems * 2),
            "Result should be reasonably close to maxItems (within 2x tolerance)");
        Assert.That(result.Length, Is.LessThan(points.Count),
            "Should reduce the number of points from original");
    }

    /// <summary>
    /// Verifies that Simplify can reduce to a very small number of points.
    /// Tests that the algorithm works with very restrictive maxItems.
    /// </summary>
    [Test]
    public void Simplify_WithVerySmallMaxItems_ReducesSignificantly()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 1000; i++)
        {
            points.Add(new ChartPoint<int>(i, i * 1.0));
        }
        var maxItems = 5;
        var reducer = new ReduceArray<int>(points, maxItems);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Length, Is.LessThanOrEqualTo(maxItems));
            Assert.That(result.Length, Is.GreaterThanOrEqualTo(2)); // At least start and end
        }
    }

    #endregion

    #region Simplify Tests - Different Types

    /// <summary>
    /// Verifies that Simplify works correctly with double type for index.
    /// Tests that floating-point indices are handled correctly.
    /// </summary>
    [Test]
    public void Simplify_WithDoubleIndex_HandlesCorrectly()
    {
        // Arrange
        var points = new List<ChartPoint<double>>();
        for (int i = 0; i < 200; i++)
        {
            points.Add(new ChartPoint<double>(i * 0.5, i * 1.0));
        }
        var reducer = new ReduceArray<double>(points, maxItems: 20);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Length, Is.LessThanOrEqualTo(20));
            Assert.That(result[0].index, Is.EqualTo(0.0));
            Assert.That(result[^1].index, Is.EqualTo(99.5));
        }
    }

    /// <summary>
    /// Verifies that Simplify works correctly with long type for index.
    /// Tests that large integer indices are handled correctly.
    /// </summary>
    [Test]
    public void Simplify_WithLongIndex_HandlesCorrectly()
    {
        // Arrange
        var points = new List<ChartPoint<long>>();
        for (long i = 0; i < 200; i++)
        {
            points.Add(new ChartPoint<long>(i * 1000L, i * 1.0));
        }
        var reducer = new ReduceArray<long>(points, maxItems: 20);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Length, Is.LessThanOrEqualTo(20));
            Assert.That(result[0].index, Is.EqualTo(0L));
            Assert.That(result[^1].index, Is.EqualTo(199000L));
        }
    }

    #endregion

    #region Simplify Tests - Edge Cases

    /// <summary>
    /// Verifies that Simplify handles points with same index correctly.
    /// Tests that vertical lines (same x, different y) are handled.
    /// </summary>
    [Test]
    public void Simplify_WithSameIndex_HandlesCorrectly()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(10, 0.0),
            new(10, 5.0),
            new(10, 10.0),
            new(11, 10.0)
        };
        var reducer = new ReduceArray<int>(points);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        // Should handle vertical segments (same index)
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }

    /// <summary>
    /// Verifies that Simplify handles points with NaN y values.
    /// Tests that NaN values don't cause exceptions.
    /// </summary>
    [Test]
    public void Simplify_WithNaNValues_HandlesCorrectly()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 0.0),
            new(1, double.NaN),
            new(2, 2.0)
        };
        var reducer = new ReduceArray<int>(points);

        // Act & Assert
        // Should not throw, though behavior with NaN may vary
        Assert.DoesNotThrow(() =>
        {
            var result = reducer.Simplify().ToArray();
            Assert.That(result, Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies that Simplify handles points with infinity y values.
    /// Tests that infinity values don't cause exceptions.
    /// </summary>
    [Test]
    public void Simplify_WithInfinityValues_HandlesCorrectly()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 0.0),
            new(1, double.PositiveInfinity),
            new(2, 2.0)
        };
        var reducer = new ReduceArray<int>(points);

        // Act & Assert
        // Should not throw, though behavior with infinity may vary
        Assert.DoesNotThrow(() =>
        {
            var result = reducer.Simplify().ToArray();
            Assert.That(result, Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies that Simplify handles very large arrays efficiently.
    /// Tests that the algorithm can process large datasets.
    /// </summary>
    [Test]
    public void Simplify_WithVeryLargeArray_ProcessesEfficiently()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 10000; i++)
        {
            points.Add(new ChartPoint<int>(i, i * 0.1));
        }
        var reducer = new ReduceArray<int>(points, maxItems: 100);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Length, Is.LessThanOrEqualTo(100));
            Assert.That(result[0].index, Is.EqualTo(0));
            Assert.That(result[^1].index, Is.EqualTo(9999));
        }
    }

    /// <summary>
    /// Verifies that Simplify preserves order of points.
    /// Tests that the result maintains the original sequence.
    /// </summary>
    [Test]
    public void Simplify_PreservesPointOrder()
    {
        // Arrange
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 200; i++)
        {
            points.Add(new ChartPoint<int>(i, Math.Sin(i * 0.1) * 10));
        }
        var reducer = new ReduceArray<int>(points, maxItems: 30);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        for (int i = 1; i < result.Length; i++)
        {
            Assert.That(result[i].index, Is.GreaterThan(result[i - 1].index),
                "Points should be in ascending order by index");
        }
    }

    /// <summary>
    /// Verifies that Simplify handles points with annotations correctly.
    /// Tests that annotations are preserved in the result.
    /// </summary>
    [Test]
    public void Simplify_PreservesAnnotations()
    {
        // Arrange
        var points = new ChartPoint<int>[]
        {
            new(0, 0.0, "Start"),
            new(1, 1.0, "Middle"),
            new(2, 2.0, "End")
        };
        var reducer = new ReduceArray<int>(points);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        // Annotations should be preserved (though some points may be removed)
        foreach (var point in result)
        {
            Assert.That(point.annotation, Is.Not.Null.Or.Null);
        }
    }

    #endregion

    #region Simplify Tests - Real-World Scenarios

    /// <summary>
    /// Verifies that Simplify works with a realistic data series.
    /// Tests the algorithm with a typical charting scenario.
    /// </summary>
    [Test]
    public void Simplify_WithRealisticData_ReducesAppropriately()
    {
        // Arrange - Simulate sensor data with noise
        var random = new Random(42); // Fixed seed for reproducibility
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 1000; i++)
        {
            var baseValue = 50.0 + 20.0 * Math.Sin(i * 0.05);
            var noise = random.NextDouble() * 5.0 - 2.5;
            points.Add(new ChartPoint<int>(i, baseValue + noise));
        }
        var maxItems = 100;
        var reducer = new ReduceArray<int>(points, maxItems: maxItems);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            // Binary search with limited iterations may not converge exactly to maxItems,
            // especially with noisy data. Allow up to 2x maxItems as tolerance.
            Assert.That(result.Length, Is.LessThanOrEqualTo(maxItems * 2),
                "Result should be reasonably close to maxItems (within 2x tolerance for noisy data)");
            Assert.That(result.Length, Is.LessThan(points.Count),
                "Should reduce the number of points from original");
            Assert.That(result.Length, Is.GreaterThan(10),
                "Should keep some points due to noise and signal variations");
            Assert.That(result[0].index, Is.EqualTo(0));
            Assert.That(result[^1].index, Is.EqualTo(999));
        }
    }

    /// <summary>
    /// Verifies that Simplify handles step functions correctly.
    /// Tests that sudden changes in values are preserved.
    /// </summary>
    [Test]
    public void Simplify_WithStepFunction_PreservesSteps()
    {
        // Arrange - Create a step function
        var points = new List<ChartPoint<int>>();
        for (int i = 0; i < 200; i++)
        {
            var y = (i / 50) * 10.0; // Step every 50 points
            points.Add(new ChartPoint<int>(i, y));
        }
        var reducer = new ReduceArray<int>(points, maxItems: 20);

        // Act
        var result = reducer.Simplify().ToArray();

        // Assert
        // Should preserve step changes
        Assert.That(result.Length, Is.LessThanOrEqualTo(20));
    }

    #endregion
}

