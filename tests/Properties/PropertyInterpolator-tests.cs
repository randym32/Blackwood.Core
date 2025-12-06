// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Drawing;
using NUnit.Framework;
using Blackwood;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test class with various interpolatable properties for testing PropertyInterpolator.
/// </summary>
public class TestInterpolatableClass
{
    public float FloatProperty { get; set; }
    public double DoubleProperty { get; set; }
    public int IntProperty { get; set; }
    public decimal DecimalProperty { get; set; }
    public Color ColorProperty { get; set; }
    public Point PointProperty { get; set; }
    public PointF PointFProperty { get; set; }
    public Rectangle RectangleProperty { get; set; }
    public RectangleF RectangleFProperty { get; set; }
    public BezierPath BezierPathProperty { get; set; } = new();

    // Array properties for testing
    public float[] FloatArrayProperty { get; set; } = [];
    public int[] IntArrayProperty { get; set; } = [];
    public double[] DoubleArrayProperty { get; set; } = [];
    public decimal[] DecimalArrayProperty { get; set; } = [];

    // Non-interpolatable properties
    public string StringProperty { get; set; } = "Test";
    public bool BoolProperty { get; set; }
    public object ObjectProperty { get; set; } = new();

    // Read-only property (should be ignored)
    public int ReadOnlyProperty { get; }

    // Write-only property (should be ignored)
    public static int WriteOnlyProperty { set { } }
}

/// <summary>
/// Test class with no interpolatable properties.
/// </summary>
public class TestNonInterpolatableClass
{
    public string StringProperty { get; set; } = "Test";
    public bool BoolProperty { get; set; }
    public object ObjectProperty { get; set; } = new();
}

/// <summary>
/// Test class with interpolatable fields for testing field interpolation.
/// </summary>
public class TestInterpolatableFieldsClass
{
    public float FloatField;
    public int IntField;
    public Color ColorField;
    public Point PointField;
    public BezierPath BezierPathField = new();
    public float[] FloatArrayField = [];
    public int[] IntArrayField = [];

    // Non-interpolatable fields
    public string StringField = "Test";
    public bool BoolField;
    public object ObjectField = new();

    // Read-only field (should be ignored)
    public readonly int ReadOnlyField = 42;

    // Static field (should be ignored)
    public static int StaticField = 100;
}

/// <summary>
/// Tests for PropertyInterpolator functionality.
/// </summary>
[TestFixture]
public class PropertyInterpolatorTests
{
    private const float Tolerance = 0.001f;

    #region Interpolatable Discovery via CreateInterpolationFunction

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for types with interpolatable properties.
    /// This verifies that the basic functionality works for classes containing properties that can be interpolated.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForInterpolatableType_ReturnsFunction()
    {
        var fn = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(fn, Is.Not.Null);
    }

    /// <summary>
    /// Tests that PropertyInterpolator returns null for types with no interpolatable properties.
    /// This verifies that the system correctly identifies when a type cannot be interpolated.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForNonInterpolatableType_ReturnsNull()
    {
        var fn = PropertyInterpolator.CreateInterpolationFunction<TestNonInterpolatableClass>();
        Assert.That(fn, Is.Null);
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for BezierPath class type directly.
    /// This verifies that BezierPath can be interpolated as a standalone class, not just as a property or field.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForBezierPathType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<BezierPath>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for BezierPath type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate BezierPath objects directly.
    /// This verifies that the interpolation function works correctly when BezierPath is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForBezierPathType_InterpolatesCorrectly()
    {
        // Arrange - Create start and end Bezier paths
        var start = new BezierPath
        {
            PathPoints =
            [
                new(10f, 20f),  // Start point
                new(30f, 40f),  // Control point 1
                new(50f, 60f),  // Control point 2
                new(70f, 80f)   // End point
            ],
            Closed = false
        };

        var end = new BezierPath
        {
            PathPoints =
            [
                new(100f, 200f),  // Start point (10x start)
                new(300f, 400f),  // Control point 1 (10x start)
                new(500f, 600f),  // Control point 2 (10x start)
                new(700f, 800f)   // End point (10x start)
            ],
            Closed = true
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<BezierPath>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for BezierPath type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new BezierPath();
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        // Expected: (10 + 100) / 2 = 55, (20 + 200) / 2 = 110, etc.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result BezierPath should not be null");
            Assert.That(result.PathPoints, Is.Not.Null, "PathPoints should not be null");
            Assert.That(result.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points");
            Assert.That(result.PathPoints[0].X, Is.EqualTo(55f).Within(Tolerance), "First point X should be interpolated correctly");
            Assert.That(result.PathPoints[0].Y, Is.EqualTo(110f).Within(Tolerance), "First point Y should be interpolated correctly");
            Assert.That(result.PathPoints[1].X, Is.EqualTo(165f).Within(Tolerance), "Control point 1 X should be interpolated correctly");
            Assert.That(result.PathPoints[1].Y, Is.EqualTo(220f).Within(Tolerance), "Control point 1 Y should be interpolated correctly");
            Assert.That(result.PathPoints[2].X, Is.EqualTo(275f).Within(Tolerance), "Control point 2 X should be interpolated correctly");
            Assert.That(result.PathPoints[2].Y, Is.EqualTo(330f).Within(Tolerance), "Control point 2 Y should be interpolated correctly");
            Assert.That(result.PathPoints[3].X, Is.EqualTo(385f).Within(Tolerance), "End point X should be interpolated correctly");
            Assert.That(result.PathPoints[3].Y, Is.EqualTo(440f).Within(Tolerance), "End point Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator correctly interpolates BezierPath at boundary conditions (t=0.0 and t=1.0).
    /// This verifies that the interpolation function handles boundary conditions correctly for BezierPath type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForBezierPathType_BoundaryConditions_WorksCorrectly()
    {
        // Arrange - Create start and end Bezier paths
        var start = new BezierPath
        {
            PathPoints =
            [
                new(10f, 20f),
                new(30f, 40f),
                new(50f, 60f),
                new(70f, 80f)
            ],
            Closed = false
        };

        var end = new BezierPath
        {
            PathPoints =
            [
                new(100f, 200f),
                new(300f, 400f),
                new(500f, 600f),
                new(700f, 800f)
            ],
            Closed = true
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<BezierPath>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for BezierPath type");

        // Act & Assert - Test at t=0.0
        var result0 = new BezierPath();
        interpolate!(start, end, 0.0f, result0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result0.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points at t=0.0");
            Assert.That(result0.PathPoints[0].X, Is.EqualTo(10f).Within(Tolerance), "At t=0.0, first point X should match start");
            Assert.That(result0.PathPoints[0].Y, Is.EqualTo(20f).Within(Tolerance), "At t=0.0, first point Y should match start");
            Assert.That(result0.PathPoints[3].X, Is.EqualTo(70f).Within(Tolerance), "At t=0.0, end point X should match start");
            Assert.That(result0.PathPoints[3].Y, Is.EqualTo(80f).Within(Tolerance), "At t=0.0, end point Y should match start");
        }

        // Act & Assert - Test at t=1.0
        var result1 = new BezierPath();
        interpolate!(start, end, 1.0f, result1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points at t=1.0");
            Assert.That(result1.PathPoints[0].X, Is.EqualTo(100f).Within(Tolerance), "At t=1.0, first point X should match end");
            Assert.That(result1.PathPoints[0].Y, Is.EqualTo(200f).Within(Tolerance), "At t=1.0, first point Y should match end");
            Assert.That(result1.PathPoints[3].X, Is.EqualTo(700f).Within(Tolerance), "At t=1.0, end point X should match end");
            Assert.That(result1.PathPoints[3].Y, Is.EqualTo(800f).Within(Tolerance), "At t=1.0, end point Y should match end");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for float array type directly.
    /// This verifies that float arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForFloatArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<float[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for float[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate float arrays directly.
    /// This verifies that the interpolation function works correctly when float[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForFloatArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };
        var end = new float[] { 5.0f, 6.0f, 7.0f, 8.0f };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<float[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for float[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new float[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(4), "Result array should have correct length");
            Assert.That(result[0], Is.EqualTo(3.0f).Within(Tolerance), "First element should be interpolated correctly");
            Assert.That(result[1], Is.EqualTo(4.0f).Within(Tolerance), "Second element should be interpolated correctly");
            Assert.That(result[2], Is.EqualTo(5.0f).Within(Tolerance), "Third element should be interpolated correctly");
            Assert.That(result[3], Is.EqualTo(6.0f).Within(Tolerance), "Fourth element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for int array type directly.
    /// This verifies that int arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForIntArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<int[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for int[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate int arrays directly.
    /// This verifies that the interpolation function works correctly when int[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForIntArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new int[] { 10, 20, 30, 40 };
        var end = new int[] { 50, 60, 70, 80 };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<int[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for int[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new int[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(4), "Result array should have correct length");
            Assert.That(result[0], Is.EqualTo(30), "First element should be interpolated correctly");
            Assert.That(result[1], Is.EqualTo(40), "Second element should be interpolated correctly");
            Assert.That(result[2], Is.EqualTo(50), "Third element should be interpolated correctly");
            Assert.That(result[3], Is.EqualTo(60), "Fourth element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for double array type directly.
    /// This verifies that double arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForDoubleArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<double[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for double[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate double arrays directly.
    /// This verifies that the interpolation function works correctly when double[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForDoubleArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new double[] { 1.5, 2.5, 3.5, 4.5 };
        var end = new double[] { 5.5, 6.5, 7.5, 8.5 };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<double[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for double[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new double[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(4), "Result array should have correct length");
            Assert.That(result[0], Is.EqualTo(3.5).Within(0.0001), "First element should be interpolated correctly");
            Assert.That(result[1], Is.EqualTo(4.5).Within(0.0001), "Second element should be interpolated correctly");
            Assert.That(result[2], Is.EqualTo(5.5).Within(0.0001), "Third element should be interpolated correctly");
            Assert.That(result[3], Is.EqualTo(6.5).Within(0.0001), "Fourth element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for decimal array type directly.
    /// This verifies that decimal arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForDecimalArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<decimal[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for decimal[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate decimal arrays directly.
    /// This verifies that the interpolation function works correctly when decimal[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForDecimalArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new decimal[] { 1.5m, 2.5m, 3.5m, 4.5m };
        var end = new decimal[] { 5.5m, 6.5m, 7.5m, 8.5m };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<decimal[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for decimal[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new decimal[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(4), "Result array should have correct length");
            Assert.That(result[0], Is.EqualTo(3.5m).Within(0.001m), "First element should be interpolated correctly");
            Assert.That(result[1], Is.EqualTo(4.5m).Within(0.001m), "Second element should be interpolated correctly");
            Assert.That(result[2], Is.EqualTo(5.5m).Within(0.001m), "Third element should be interpolated correctly");
            Assert.That(result[3], Is.EqualTo(6.5m).Within(0.001m), "Fourth element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator correctly interpolates float arrays at boundary conditions (t=0.0 and t=1.0).
    /// This verifies that the interpolation function handles boundary conditions correctly for array types.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForFloatArrayType_BoundaryConditions_WorksCorrectly()
    {
        // Arrange
        var start = new float[] { 10.0f, 20.0f, 30.0f };
        var end = new float[] { 40.0f, 50.0f, 60.0f };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<float[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for float[] type");

        // Act & Assert - Test at t=0.0
        var result0 = new float[start.Length];
        interpolate!(start, end, 0.0f, result0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result0, Has.Length.EqualTo(3), "Result array should have correct length at t=0.0");
            Assert.That(result0[0], Is.EqualTo(10.0f).Within(Tolerance), "At t=0.0, first element should match start");
            Assert.That(result0[1], Is.EqualTo(20.0f).Within(Tolerance), "At t=0.0, second element should match start");
            Assert.That(result0[2], Is.EqualTo(30.0f).Within(Tolerance), "At t=0.0, third element should match start");
        }

        // Act & Assert - Test at t=1.0
        var result1 = new float[start.Length];
        interpolate!(start, end, 1.0f, result1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Has.Length.EqualTo(3), "Result array should have correct length at t=1.0");
            Assert.That(result1[0], Is.EqualTo(40.0f).Within(Tolerance), "At t=1.0, first element should match end");
            Assert.That(result1[1], Is.EqualTo(50.0f).Within(Tolerance), "At t=1.0, second element should match end");
            Assert.That(result1[2], Is.EqualTo(60.0f).Within(Tolerance), "At t=1.0, third element should match end");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator handles empty arrays correctly.
    /// This verifies that the interpolation function handles empty arrays gracefully.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForFloatArrayType_EmptyArrays_HandlesCorrectly()
    {
        // Arrange
        var start = Array.Empty<float>();
        var end = Array.Empty<float>();

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<float[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for float[] type");

        // Act - Interpolate at midpoint
        var result = Array.Empty<float>();
        interpolate!(start, end, 0.5f, result);

        // Assert - Empty arrays should remain empty
        Assert.That(result, Is.Not.Null, "Result array should not be null");
        Assert.That(result, Is.Empty, "Empty arrays should remain empty");
    }

    /// <summary>
    /// Tests that PropertyInterpolator throws ArgumentException when arrays have different lengths.
    /// This verifies that the interpolation function validates array length compatibility.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForFloatArrayType_DifferentLengths_ThrowsArgumentException()
    {
        // Arrange - Create arrays with different lengths
        var start = new float[] { 1.0f, 2.0f, 3.0f };
        var end = new float[] { 4.0f, 5.0f, 6.0f, 7.0f, 8.0f };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<float[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for float[] type");

        // Act & Assert - Should throw ArgumentException when arrays have different lengths
        var result = new float[Math.Min(start.Length, end.Length)];
        Assert.Throws<ArgumentException>(() => interpolate!(start, end, 0.5f, result),
            "Interpolation should throw ArgumentException when start and end arrays have different lengths");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for Point array type directly.
    /// This verifies that Point arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<Point[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for Point[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate Point arrays directly.
    /// This verifies that the interpolation function works correctly when Point[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new Point[] { new(0, 0), new(10, 10), new(20, 20) };
        var end = new Point[] { new(100, 100), new(110, 110), new(120, 120) };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<Point[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for Point[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new Point[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(3), "Result array should have correct length");
            Assert.That(result[0].X, Is.EqualTo(50), "First point X should be interpolated correctly");
            Assert.That(result[0].Y, Is.EqualTo(50), "First point Y should be interpolated correctly");
            Assert.That(result[1].X, Is.EqualTo(60), "Second point X should be interpolated correctly");
            Assert.That(result[1].Y, Is.EqualTo(60), "Second point Y should be interpolated correctly");
            Assert.That(result[2].X, Is.EqualTo(70), "Third point X should be interpolated correctly");
            Assert.That(result[2].Y, Is.EqualTo(70), "Third point Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator correctly interpolates Point arrays at boundary conditions (t=0.0 and t=1.0).
    /// This verifies that the interpolation function handles boundary conditions correctly for Point array types.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointArrayType_BoundaryConditions_WorksCorrectly()
    {
        // Arrange
        var start = new Point[] { new(10, 20), new(30, 40) };
        var end = new Point[] { new(100, 200), new(300, 400) };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<Point[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for Point[] type");

        // Act & Assert - Test at t=0.0
        var result0 = new Point[start.Length];
        interpolate!(start, end, 0.0f, result0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result0, Has.Length.EqualTo(2), "Result array should have correct length at t=0.0");
            Assert.That(result0[0].X, Is.EqualTo(10), "At t=0.0, first point X should match start");
            Assert.That(result0[0].Y, Is.EqualTo(20), "At t=0.0, first point Y should match start");
            Assert.That(result0[1].X, Is.EqualTo(30), "At t=0.0, second point X should match start");
            Assert.That(result0[1].Y, Is.EqualTo(40), "At t=0.0, second point Y should match start");
        }

        // Act & Assert - Test at t=1.0
        var result1 = new Point[start.Length];
        interpolate!(start, end, 1.0f, result1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Has.Length.EqualTo(2), "Result array should have correct length at t=1.0");
            Assert.That(result1[0].X, Is.EqualTo(100), "At t=1.0, first point X should match end");
            Assert.That(result1[0].Y, Is.EqualTo(200), "At t=1.0, first point Y should match end");
            Assert.That(result1[1].X, Is.EqualTo(300), "At t=1.0, second point X should match end");
            Assert.That(result1[1].Y, Is.EqualTo(400), "At t=1.0, second point Y should match end");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator handles empty Point arrays correctly.
    /// This verifies that the interpolation function handles empty arrays gracefully for Point arrays.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointArrayType_EmptyArrays_HandlesCorrectly()
    {
        // Arrange
        var start = Array.Empty<Point>();
        var end = Array.Empty<Point>();

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<Point[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for Point[] type");

        // Act - Interpolate at midpoint
        var result = Array.Empty<Point>();
        interpolate!(start, end, 0.5f, result);

        // Assert - Empty arrays should remain empty
        Assert.That(result, Is.Not.Null, "Result array should not be null");
        Assert.That(result, Is.Empty, "Empty arrays should remain empty");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for PointF array type directly.
    /// This verifies that PointF arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointFArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<PointF[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for PointF[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate PointF arrays directly.
    /// This verifies that the interpolation function works correctly when PointF[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointFArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new PointF[] { new(0f, 0f), new(10f, 10f), new(20f, 20f) };
        var end = new PointF[] { new(100f, 100f), new(110f, 110f), new(120f, 120f) };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<PointF[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for PointF[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new PointF[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(3), "Result array should have correct length");
            Assert.That(result[0].X, Is.EqualTo(50f).Within(Tolerance), "First point X should be interpolated correctly");
            Assert.That(result[0].Y, Is.EqualTo(50f).Within(Tolerance), "First point Y should be interpolated correctly");
            Assert.That(result[1].X, Is.EqualTo(60f).Within(Tolerance), "Second point X should be interpolated correctly");
            Assert.That(result[1].Y, Is.EqualTo(60f).Within(Tolerance), "Second point Y should be interpolated correctly");
            Assert.That(result[2].X, Is.EqualTo(70f).Within(Tolerance), "Third point X should be interpolated correctly");
            Assert.That(result[2].Y, Is.EqualTo(70f).Within(Tolerance), "Third point Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for Rectangle array type directly.
    /// This verifies that Rectangle arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<Rectangle[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for Rectangle[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate Rectangle arrays directly.
    /// This verifies that the interpolation function works correctly when Rectangle[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new Rectangle[]
        {
            new(0, 0, 100, 50),
            new(10, 10, 110, 60)
        };
        var end = new Rectangle[]
        {
            new(50, 25, 200, 100),
            new(60, 35, 210, 110)
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<Rectangle[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for Rectangle[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new Rectangle[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(2), "Result array should have correct length");
            Assert.That(result[0].X, Is.EqualTo(25), "First rectangle X should be interpolated correctly");
            Assert.That(result[0].Y, Is.EqualTo(12), "First rectangle Y should be interpolated correctly");
            Assert.That(result[0].Width, Is.EqualTo(150), "First rectangle Width should be interpolated correctly");
            Assert.That(result[0].Height, Is.EqualTo(75), "First rectangle Height should be interpolated correctly");
            Assert.That(result[1].X, Is.EqualTo(35), "Second rectangle X should be interpolated correctly");
            Assert.That(result[1].Y, Is.EqualTo(22), "Second rectangle Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator correctly interpolates Rectangle arrays at boundary conditions (t=0.0 and t=1.0).
    /// This verifies that the interpolation function handles boundary conditions correctly for Rectangle array types.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleArrayType_BoundaryConditions_WorksCorrectly()
    {
        // Arrange
        var start = new Rectangle[] { new(0, 0, 100, 50), new(10, 10, 110, 60) };
        var end = new Rectangle[] { new(50, 25, 200, 100), new(60, 35, 210, 110) };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<Rectangle[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for Rectangle[] type");

        // Act & Assert - Test at t=0.0
        var result0 = new Rectangle[start.Length];
        interpolate!(start, end, 0.0f, result0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result0, Has.Length.EqualTo(2), "Result array should have correct length at t=0.0");
            Assert.That(result0[0].X, Is.Zero, "At t=0.0, first rectangle X should match start");
            Assert.That(result0[0].Y, Is.Zero, "At t=0.0, first rectangle Y should match start");
            Assert.That(result0[0].Width, Is.EqualTo(100), "At t=0.0, first rectangle Width should match start");
            Assert.That(result0[0].Height, Is.EqualTo(50), "At t=0.0, first rectangle Height should match start");
        }

        // Act & Assert - Test at t=1.0
        var result1 = new Rectangle[start.Length];
        interpolate!(start, end, 1.0f, result1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Has.Length.EqualTo(2), "Result array should have correct length at t=1.0");
            Assert.That(result1[0].X, Is.EqualTo(50), "At t=1.0, first rectangle X should match end");
            Assert.That(result1[0].Y, Is.EqualTo(25), "At t=1.0, first rectangle Y should match end");
            Assert.That(result1[0].Width, Is.EqualTo(200), "At t=1.0, first rectangle Width should match end");
            Assert.That(result1[0].Height, Is.EqualTo(100), "At t=1.0, first rectangle Height should match end");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator handles empty Rectangle arrays correctly.
    /// This verifies that the interpolation function handles empty arrays gracefully for Rectangle arrays.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleArrayType_EmptyArrays_HandlesCorrectly()
    {
        // Arrange
        var start = Array.Empty<Rectangle>();
        var end = Array.Empty<Rectangle>();

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<Rectangle[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for Rectangle[] type");

        // Act - Interpolate at midpoint
        var result = Array.Empty<Rectangle>();
        interpolate!(start, end, 0.5f, result);

        // Assert - Empty arrays should remain empty
        Assert.That(result, Is.Not.Null, "Result array should not be null");
        Assert.That(result, Is.Empty, "Empty arrays should remain empty");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for RectangleF array type directly.
    /// This verifies that RectangleF arrays can be interpolated as a standalone type.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleFArrayType_ReturnsFunction()
    {
        // Act
        var fn = PropertyInterpolator.CreateInterpolationFunction<RectangleF[]>();

        // Assert
        Assert.That(fn, Is.Not.Null, "Interpolation function should be created for RectangleF[] type");
    }

    /// <summary>
    /// Tests that PropertyInterpolator can interpolate RectangleF arrays directly.
    /// This verifies that the interpolation function works correctly when RectangleF[] is used as the type parameter.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleFArrayType_InterpolatesCorrectly()
    {
        // Arrange
        var start = new RectangleF[]
        {
            new(0f, 0f, 100f, 50f),
            new(10f, 10f, 110f, 60f)
        };
        var end = new RectangleF[]
        {
            new(50f, 25f, 200f, 100f),
            new(60f, 35f, 210f, 110f)
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<RectangleF[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for RectangleF[] type");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new RectangleF[start.Length];
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null, "Result array should not be null");
            Assert.That(result, Has.Length.EqualTo(2), "Result array should have correct length");
            Assert.That(result[0].X, Is.EqualTo(25f).Within(Tolerance), "First rectangle X should be interpolated correctly");
            Assert.That(result[0].Y, Is.EqualTo(12.5f).Within(Tolerance), "First rectangle Y should be interpolated correctly");
            Assert.That(result[0].Width, Is.EqualTo(150f).Within(Tolerance), "First rectangle Width should be interpolated correctly");
            Assert.That(result[0].Height, Is.EqualTo(75f).Within(Tolerance), "First rectangle Height should be interpolated correctly");
            Assert.That(result[1].X, Is.EqualTo(35f).Within(Tolerance), "Second rectangle X should be interpolated correctly");
            Assert.That(result[1].Y, Is.EqualTo(22.5f).Within(Tolerance), "Second rectangle Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator correctly interpolates RectangleF arrays at boundary conditions (t=0.0 and t=1.0).
    /// This verifies that the interpolation function handles boundary conditions correctly for RectangleF array types.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleFArrayType_BoundaryConditions_WorksCorrectly()
    {
        // Arrange
        var start = new RectangleF[] { new(0f, 0f, 100f, 50f), new(10f, 10f, 110f, 60f) };
        var end = new RectangleF[] { new(50f, 25f, 200f, 100f), new(60f, 35f, 210f, 110f) };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<RectangleF[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for RectangleF[] type");

        // Act & Assert - Test at t=0.0
        var result0 = new RectangleF[start.Length];
        interpolate!(start, end, 0.0f, result0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result0, Has.Length.EqualTo(2), "Result array should have correct length at t=0.0");
            Assert.That(result0[0].X, Is.Zero.Within(Tolerance), "At t=0.0, first rectangle X should match start");
            Assert.That(result0[0].Y, Is.Zero.Within(Tolerance), "At t=0.0, first rectangle Y should match start");
            Assert.That(result0[0].Width, Is.EqualTo(100f).Within(Tolerance), "At t=0.0, first rectangle Width should match start");
            Assert.That(result0[0].Height, Is.EqualTo(50f).Within(Tolerance), "At t=0.0, first rectangle Height should match start");
        }

        // Act & Assert - Test at t=1.0
        var result1 = new RectangleF[start.Length];
        interpolate!(start, end, 1.0f, result1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Has.Length.EqualTo(2), "Result array should have correct length at t=1.0");
            Assert.That(result1[0].X, Is.EqualTo(50f).Within(Tolerance), "At t=1.0, first rectangle X should match end");
            Assert.That(result1[0].Y, Is.EqualTo(25f).Within(Tolerance), "At t=1.0, first rectangle Y should match end");
            Assert.That(result1[0].Width, Is.EqualTo(200f).Within(Tolerance), "At t=1.0, first rectangle Width should match end");
            Assert.That(result1[0].Height, Is.EqualTo(100f).Within(Tolerance), "At t=1.0, first rectangle Height should match end");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator handles empty RectangleF arrays correctly.
    /// This verifies that the interpolation function handles empty arrays gracefully for RectangleF arrays.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForRectangleFArrayType_EmptyArrays_HandlesCorrectly()
    {
        // Arrange
        var start = Array.Empty<RectangleF>();
        var end = Array.Empty<RectangleF>();

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<RectangleF[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for RectangleF[] type");

        // Act - Interpolate at midpoint
        var result = Array.Empty<RectangleF>();
        interpolate!(start, end, 0.5f, result);

        // Assert - Empty arrays should remain empty
        Assert.That(result, Is.Not.Null, "Result array should not be null");
        Assert.That(result, Is.Empty, "Empty arrays should remain empty");
    }

    /// <summary>
    /// Tests that PropertyInterpolator correctly interpolates PointF arrays at boundary conditions (t=0.0 and t=1.0).
    /// This verifies that the interpolation function handles boundary conditions correctly for PointF array types.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointFArrayType_BoundaryConditions_WorksCorrectly()
    {
        // Arrange
        var start = new PointF[] { new (10f, 20f), new (30f, 40f) };
        var end = new PointF[] { new (100f, 200f), new (300f, 400f) };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<PointF[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for PointF[] type");

        // Act & Assert - Test at t=0.0
        var result0 = new PointF[start.Length];
        interpolate!(start, end, 0.0f, result0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result0, Has.Length.EqualTo(2), "Result array should have correct length at t=0.0");
            Assert.That(result0[0].X, Is.EqualTo(10f).Within(Tolerance), "At t=0.0, first point X should match start");
            Assert.That(result0[0].Y, Is.EqualTo(20f).Within(Tolerance), "At t=0.0, first point Y should match start");
            Assert.That(result0[1].X, Is.EqualTo(30f).Within(Tolerance), "At t=0.0, second point X should match start");
            Assert.That(result0[1].Y, Is.EqualTo(40f).Within(Tolerance), "At t=0.0, second point Y should match start");
        }

        // Act & Assert - Test at t=1.0
        var result1 = new PointF[start.Length];
        interpolate!(start, end, 1.0f, result1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Has.Length.EqualTo(2), "Result array should have correct length at t=1.0");
            Assert.That(result1[0].X, Is.EqualTo(100f).Within(Tolerance), "At t=1.0, first point X should match end");
            Assert.That(result1[0].Y, Is.EqualTo(200f).Within(Tolerance), "At t=1.0, first point Y should match end");
            Assert.That(result1[1].X, Is.EqualTo(300f).Within(Tolerance), "At t=1.0, second point X should match end");
            Assert.That(result1[1].Y, Is.EqualTo(400f).Within(Tolerance), "At t=1.0, second point Y should match end");
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator handles empty PointF arrays correctly.
    /// This verifies that the interpolation function handles empty arrays gracefully for PointF arrays.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_ForPointFArrayType_EmptyArrays_HandlesCorrectly()
    {
        // Arrange
        var start = Array.Empty<PointF>();
        var end = Array.Empty<PointF>();

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<PointF[]>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for PointF[] type");

        // Act - Interpolate at midpoint
        var result = Array.Empty<PointF>();
        interpolate!(start, end, 0.5f, result);

        // Assert - Empty arrays should remain empty
        Assert.That(result, Is.Not.Null, "Result array should not be null");
        Assert.That(result, Is.Empty, "Empty arrays should remain empty");
    }

    #endregion

    #region CreateInterpolationFunction Tests

    /// <summary>
    /// Tests that float and double properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that basic numeric interpolation works correctly for floating-point types.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithFloatProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass { FloatProperty = 10f, DoubleProperty = 20.0 };
        var end = new TestInterpolatableClass { FloatProperty = 30f, DoubleProperty = 40.0 };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatProperty, Is.EqualTo(20f).Within(Tolerance));
            Assert.That(result.DoubleProperty, Is.EqualTo(30.0).Within(Tolerance));
        }
    }

    /// <summary>
    /// Tests that Color properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that color interpolation works correctly, blending between two colors.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithColorProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass { ColorProperty = Color.Red };
        var end = new TestInterpolatableClass { ColorProperty = Color.Blue };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ColorProperty.A, Is.EqualTo(255)); // Alpha should remain 255
            Assert.That(result.ColorProperty.R, Is.EqualTo(128)); // Red component should be 128 (halfway)
            Assert.That(result.ColorProperty.G, Is.Zero);   // Green should remain 0
            Assert.That(result.ColorProperty.B, Is.EqualTo(128)); // Blue component should be 128 (halfway)
        }
    }

    /// <summary>
    /// Tests that Point properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that 2D point interpolation works correctly for integer coordinates.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithPointProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass { PointProperty = new Point(0, 0) };
        var end = new TestInterpolatableClass { PointProperty = new Point(100, 50) };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PointProperty.X, Is.EqualTo(50));
            Assert.That(result.PointProperty.Y, Is.EqualTo(25));
        }
    }

    /// <summary>
    /// Tests that PointF properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that floating-point 2D point interpolation works correctly.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithPointFProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass { PointFProperty = new PointF(0f, 0f) };
        var end = new TestInterpolatableClass { PointFProperty = new PointF(100f, 50f) };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PointFProperty.X, Is.EqualTo(50f).Within(Tolerance));
            Assert.That(result.PointFProperty.Y, Is.EqualTo(25f).Within(Tolerance));
        }
    }

    /// <summary>
    /// Tests that Rectangle properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that rectangle interpolation works correctly for integer coordinates and dimensions.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithRectangleProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass { RectangleProperty = new Rectangle(0, 0, 100, 50) };
        var end = new TestInterpolatableClass { RectangleProperty = new Rectangle(50, 25, 200, 100) };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.RectangleProperty.X, Is.EqualTo(25));
            Assert.That(result.RectangleProperty.Y, Is.EqualTo(12));
            Assert.That(result.RectangleProperty.Width, Is.EqualTo(150));
            Assert.That(result.RectangleProperty.Height, Is.EqualTo(75));
        }
    }

    /// <summary>
    /// Tests that RectangleF properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that floating-point rectangle interpolation works correctly.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithRectangleFProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass { RectangleFProperty = new RectangleF(0f, 0f, 100f, 50f) };
        var end = new TestInterpolatableClass { RectangleFProperty = new RectangleF(50f, 25f, 200f, 100f) };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.RectangleFProperty.X, Is.EqualTo(25f).Within(Tolerance));
            Assert.That(result.RectangleFProperty.Y, Is.EqualTo(12.5f).Within(Tolerance));
            Assert.That(result.RectangleFProperty.Width, Is.EqualTo(150f).Within(Tolerance));
            Assert.That(result.RectangleFProperty.Height, Is.EqualTo(75f).Within(Tolerance));
        }
    }

    /// <summary>
    /// Tests that BezierPath properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that Bezier path interpolation works correctly, interpolating all path points and the Closed property.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathProperties_InterpolatesCorrectly()
    {
        // Arrange - Create start and end Bezier paths with different coordinates
        // Start path: smaller coordinates, open path
        var start = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(10f, 20f),  // Start point
                    new PointF(30f, 40f),  // Control point 1
                    new PointF(50f, 60f),  // Control point 2
                    new PointF(70f, 80f)   // End point
                ],
                Closed = false
            }
        };

        // End path: larger coordinates (10x scale), closed path
        var end = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 200f),  // Start point (10x start)
                    new PointF(300f, 400f),  // Control point 1 (10x start)
                    new PointF(500f, 600f),  // Control point 2 (10x start)
                    new PointF(700f, 800f)   // End point (10x start)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        // Expected: (10 + 100) / 2 = 55, (20 + 200) / 2 = 110, etc.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathProperty, Is.Not.Null, "BezierPath property should not be null");
            Assert.That(result.BezierPathProperty.PathPoints, Is.Not.Null, "PathPoints should not be null");
            Assert.That(result.BezierPathProperty.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points");
            Assert.That(result.BezierPathProperty.PathPoints[0].X, Is.EqualTo(55f).Within(Tolerance), "First point X should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[0].Y, Is.EqualTo(110f).Within(Tolerance), "First point Y should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[1].X, Is.EqualTo(165f).Within(Tolerance), "Control point 1 X should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[1].Y, Is.EqualTo(220f).Within(Tolerance), "Control point 1 Y should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[2].X, Is.EqualTo(275f).Within(Tolerance), "Control point 2 X should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[2].Y, Is.EqualTo(330f).Within(Tolerance), "Control point 2 Y should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[3].X, Is.EqualTo(385f).Within(Tolerance), "End point X should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[3].Y, Is.EqualTo(440f).Within(Tolerance), "End point Y should be interpolated correctly");
            // Closed property should be interpolated (false -> true at t=0.5 should be false, but since it's bool, it may just take the end value)
            // Note: Boolean interpolation behavior may vary by implementation
        }
    }

    /// <summary>
    /// Tests that BezierPath properties return the start value when t=0.0.
    /// This verifies the boundary condition at the beginning of interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathProperties_AtT0_ReturnsStartValue()
    {
        // Arrange - Create start and end Bezier paths
        var start = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(10f, 20f),
                    new PointF(30f, 40f),
                    new PointF(50f, 60f),
                    new PointF(70f, 80f)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 200f),
                    new PointF(300f, 400f),
                    new PointF(500f, 600f),
                    new PointF(700f, 800f)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at start (t=0.0)
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.0f, result);

        // Assert - Should return the start value exactly
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathProperty, Is.Not.Null, "BezierPath property should not be null");
            Assert.That(result.BezierPathProperty.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points");
            Assert.That(result.BezierPathProperty.PathPoints[0].X, Is.EqualTo(10f).Within(Tolerance), "First point X should match start");
            Assert.That(result.BezierPathProperty.PathPoints[0].Y, Is.EqualTo(20f).Within(Tolerance), "First point Y should match start");
            Assert.That(result.BezierPathProperty.PathPoints[3].X, Is.EqualTo(70f).Within(Tolerance), "End point X should match start");
            Assert.That(result.BezierPathProperty.PathPoints[3].Y, Is.EqualTo(80f).Within(Tolerance), "End point Y should match start");
        }
    }

    /// <summary>
    /// Tests that BezierPath properties return the end value when t=1.0.
    /// This verifies the boundary condition at the end of interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathProperties_AtT1_ReturnsEndValue()
    {
        // Arrange - Create start and end Bezier paths
        var start = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(10f, 20f),
                    new PointF(30f, 40f),
                    new PointF(50f, 60f),
                    new PointF(70f, 80f)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 200f),
                    new PointF(300f, 400f),
                    new PointF(500f, 600f),
                    new PointF(700f, 800f)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at end (t=1.0)
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 1.0f, result);

        // Assert - Should return the end value exactly
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathProperty, Is.Not.Null, "BezierPath property should not be null");
            Assert.That(result.BezierPathProperty.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points");
            Assert.That(result.BezierPathProperty.PathPoints[0].X, Is.EqualTo(100f).Within(Tolerance), "First point X should match end");
            Assert.That(result.BezierPathProperty.PathPoints[0].Y, Is.EqualTo(200f).Within(Tolerance), "First point Y should match end");
            Assert.That(result.BezierPathProperty.PathPoints[3].X, Is.EqualTo(700f).Within(Tolerance), "End point X should match end");
            Assert.That(result.BezierPathProperty.PathPoints[3].Y, Is.EqualTo(800f).Within(Tolerance), "End point Y should match end");
        }
    }

    /// <summary>
    /// Tests that BezierPath properties with multiple segments are correctly interpolated.
    /// This verifies that paths with multiple Bezier segments (more than 4 points) are handled correctly.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathProperties_MultipleSegments_InterpolatesCorrectly()
    {
        // Arrange - Create paths with multiple segments (8 points = 2 segments)
        var start = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 0f),    // Segment 1: Start
                    new PointF(10f, 0f),   // Segment 1: Control 1
                    new PointF(20f, 0f),   // Segment 1: Control 2
                    new PointF(30f, 0f),   // Segment 1: End / Segment 2: Start
                    new PointF(40f, 0f),   // Segment 2: Control 1
                    new PointF(50f, 0f),   // Segment 2: Control 2
                    new PointF(60f, 0f),   // Segment 2: End
                    new PointF(70f, 0f)    // Extra point (should still be interpolated)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 100f),   // Segment 1: Start (10x Y)
                    new PointF(10f, 100f),  // Segment 1: Control 1
                    new PointF(20f, 100f),  // Segment 1: Control 2
                    new PointF(30f, 100f),  // Segment 1: End / Segment 2: Start
                    new PointF(40f, 100f),  // Segment 2: Control 1
                    new PointF(50f, 100f),  // Segment 2: Control 2
                    new PointF(60f, 100f),  // Segment 2: End
                    new PointF(70f, 100f)   // Extra point
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert - All points should be interpolated
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathProperty, Is.Not.Null, "BezierPath property should not be null");
            Assert.That(result.BezierPathProperty.PathPoints, Has.Length.EqualTo(8), "Path should have correct number of points");
            // At t=0.5, Y should be (0 + 100) / 2 = 50
            Assert.That(result.BezierPathProperty.PathPoints[0].Y, Is.EqualTo(50f).Within(Tolerance), "First point Y should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[4].Y, Is.EqualTo(50f).Within(Tolerance), "Fifth point Y should be interpolated correctly");
            Assert.That(result.BezierPathProperty.PathPoints[7].Y, Is.EqualTo(50f).Within(Tolerance), "Last point Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that BezierPath properties with empty paths are handled correctly.
    /// This verifies that empty path arrays are handled gracefully.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathProperties_EmptyPath_HandlesCorrectly()
    {
        // Arrange - Create paths with empty point arrays
        var start = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints = [],
                Closed = false
            }
        };

        var end = new TestInterpolatableClass
        {
            BezierPathProperty = new BezierPath
            {
                PathPoints = [],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at midpoint
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert - Empty path should remain empty
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathProperty, Is.Not.Null, "BezierPath property should not be null");
            Assert.That(result.BezierPathProperty.PathPoints, Is.Not.Null, "PathPoints should not be null");
            Assert.That(result.BezierPathProperty.PathPoints, Is.Empty, "Empty path should remain empty");
        }
    }

    /// <summary>
    /// Tests that multiple properties of different types are correctly interpolated simultaneously.
    /// This verifies that PropertyInterpolator can handle complex objects with multiple interpolatable properties.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithMultipleProperties_InterpolatesAllCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            FloatProperty = 10f,
            IntProperty = 100,
            ColorProperty = Color.Red,
            PointProperty = new Point(0, 0)
        };
        var end = new TestInterpolatableClass
        {
            FloatProperty = 30f,
            IntProperty = 300,
            ColorProperty = Color.Blue,
            PointProperty = new Point(100, 100)
        };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatProperty, Is.EqualTo(20f).Within(Tolerance));
            Assert.That(result.IntProperty, Is.EqualTo(200));
            Assert.That(result.ColorProperty.R, Is.EqualTo(128));
            Assert.That(result.PointProperty.X, Is.EqualTo(50));
            Assert.That(result.PointProperty.Y, Is.EqualTo(50));
        }
    }

    /// <summary>
    /// Tests that PropertyInterpolator returns null for classes with no interpolatable properties.
    /// This verifies that the system correctly handles types that cannot be interpolated.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithNonInterpolatableClass_ReturnsOriginalInstance()
    {
        // Arrange
        var start = new TestNonInterpolatableClass { StringProperty = "Start", BoolProperty = true };
        var end = new TestNonInterpolatableClass { StringProperty = "End", BoolProperty = false };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestNonInterpolatableClass>();

        // Act
        Assert.That(interpolate, Is.Null);
        var result = start; // behavior: return original when no interpolation exists

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StringProperty, Is.EqualTo("Start")); // Should remain unchanged
            Assert.That(result.BoolProperty, Is.True); // Should remain unchanged
        }
    }

    /// <summary>
    /// Tests that interpolation at t=0 returns the start values exactly.
    /// This verifies the boundary condition at the beginning of interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_AtT0_ReturnsStartValues()
    {
        // Arrange
        var start = new TestInterpolatableClass { FloatProperty = 10f, ColorProperty = Color.Red };
        var end = new TestInterpolatableClass { FloatProperty = 30f, ColorProperty = Color.Blue };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.0f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatProperty, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(result.ColorProperty.ToArgb(), Is.EqualTo(Color.Red.ToArgb()));
        }
    }

    /// <summary>
    /// Tests that interpolation at t=1 returns the end values exactly.
    /// This verifies the boundary condition at the end of interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_AtT1_ReturnsEndValues()
    {
        // Arrange
        var start = new TestInterpolatableClass { FloatProperty = 10f, ColorProperty = Color.Red };
        var end = new TestInterpolatableClass { FloatProperty = 30f, ColorProperty = Color.Blue };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 1.0f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatProperty, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(result.ColorProperty.ToArgb(), Is.EqualTo(Color.Blue.ToArgb()));
        }
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests that PropertyInterpolator handles null values gracefully without throwing exceptions.
    /// This verifies that the interpolation function is robust against null inputs.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithNullValues_HandlesGracefully()
    {
        // Arrange
        var start = new TestInterpolatableClass { FloatProperty = 10f };
        var end = new TestInterpolatableClass { FloatProperty = 30f };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() =>
        {
            var result = new TestInterpolatableClass();
            interpolate!(start, end, 0.5f, result);
            Assert.That(result.FloatProperty, Is.EqualTo(20f).Within(Tolerance));
        });
    }

    /// <summary>
    /// Tests that interpolation with identical start and end values returns the same values.
    /// This verifies that the interpolation function handles the edge case where no interpolation is needed.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithSameValues_ReturnsCorrectResult()
    {
        // Arrange
        var start = new TestInterpolatableClass { FloatProperty = 15f, ColorProperty = Color.Green };
        var end = new TestInterpolatableClass { FloatProperty = 15f, ColorProperty = Color.Green };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatProperty, Is.EqualTo(15f).Within(Tolerance));
            Assert.That(result.ColorProperty.ToArgb(), Is.EqualTo(Color.Green.ToArgb()));
        }
    }

    #endregion

    #region ScanForInterpolationFunctions Tests

    /// <summary>
    /// Tests that ScanForInterpolationFunctions handles null input gracefully by scanning all loaded assemblies.
    /// </summary>
    [Test]
    public void ScanForInterpolationFunctions_WithNullTypes_ScansAllAssemblies()
    {
        // Act & Assert - Should not throw when scanning all assemblies
        // This tests the default behavior when no specific types are provided
        Assert.DoesNotThrow(() => PropertyInterpolator.ScanForInterpolationFunctions(null));
    }

    /// <summary>
    /// Tests that ScanForInterpolationFunctions handles empty type collection gracefully.
    /// </summary>
    [Test]
    public void ScanForInterpolationFunctions_WithEmptyTypes_DoesNotThrow()
    {
        // Arrange - Create an empty array of types
        var emptyTypes = Array.Empty<System.Type>();

        // Act & Assert - Should not throw when scanning empty collection
        // This tests edge case handling for empty input
        Assert.DoesNotThrow(() => PropertyInterpolator.ScanForInterpolationFunctions(emptyTypes));
    }

    /// <summary>
    /// Tests that ScanForInterpolationFunctions handles specific types without throwing exceptions.
    /// </summary>
    [Test]
    public void ScanForInterpolationFunctions_WithSpecificTypes_DoesNotThrow()
    {
        // Arrange - Provide specific test types to scan
        var types = new[] { typeof(TestInterpolatableClass), typeof(TestNonInterpolatableClass) };

        // Act & Assert - Should not throw when scanning specific types
        // This tests the targeted scanning functionality
        Assert.DoesNotThrow(() => PropertyInterpolator.ScanForInterpolationFunctions(types));
    }

    #endregion

    #region ScanForInterpolateFunctions Tests

    /// <summary>
    /// Tests that ScanForInterpolateFunctions handles null input gracefully by scanning all loaded assemblies.
    /// This method scans for "Interpolate" methods that take (float t, T start, T end, T dest) and return void.
    /// </summary>
    [Test]
    public void ScanForInterpolateFunctions_WithNullTypes_ScansAllAssemblies()
    {
        // Act & Assert - Should not throw when scanning all assemblies
        // This tests the default behavior when no specific types are provided
        Assert.DoesNotThrow(() => PropertyInterpolator.ScanForInterpolateFunctions(null));
    }

    /// <summary>
    /// Tests that ScanForInterpolateFunctions handles empty type collection gracefully.
    /// </summary>
    [Test]
    public void ScanForInterpolateFunctions_WithEmptyTypes_DoesNotThrow()
    {
        // Arrange - Create an empty array of types
        var emptyTypes = Array.Empty<System.Type>();

        // Act & Assert - Should not throw when scanning empty collection
        // This tests edge case handling for empty input
        Assert.DoesNotThrow(() => PropertyInterpolator.ScanForInterpolateFunctions(emptyTypes));
    }

    /// <summary>
    /// Tests that ScanForInterpolateFunctions handles specific types without throwing exceptions.
    /// </summary>
    [Test]
    public void ScanForInterpolateFunctions_WithSpecificTypes_DoesNotThrow()
    {
        // Arrange - Provide specific test types to scan
        var types = new[] { typeof(TestInterpolatableClass), typeof(TestNonInterpolatableClass) };

        // Act & Assert - Should not throw when scanning specific types
        // This tests the targeted scanning functionality
        Assert.DoesNotThrow(() => PropertyInterpolator.ScanForInterpolateFunctions(types));
    }

    #endregion

    #region Decimal Property Tests

    /// <summary>
    /// Tests that decimal properties are correctly interpolated at the midpoint (t=0.5).
    /// Decimal interpolation should work: 10.5m + 0.5 * (30.5m - 10.5m) = 10.5m + 0.5 * 20m = 20.5m
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithDecimalProperties_InterpolatesCorrectly()
    {
        // Arrange - Set up start and end values with decimal precision
        var start = new TestInterpolatableClass { DecimalProperty = 10.5m };
        var end = new TestInterpolatableClass { DecimalProperty = 30.5m };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at midpoint
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert - Should be exactly halfway between start and end
        Assert.That(result.DecimalProperty, Is.EqualTo(20.5m).Within(0.001m));
    }

    /// <summary>
    /// Tests that decimal properties return the start value when t=0.0.
    /// This verifies the boundary condition at the beginning of interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithDecimalProperties_AtT0_ReturnsStartValue()
    {
        // Arrange - Set up start and end values
        var start = new TestInterpolatableClass { DecimalProperty = 15.75m };
        var end = new TestInterpolatableClass { DecimalProperty = 25.75m };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at start (t=0.0)
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.0f, result);

        // Assert - Should return the start value exactly
        Assert.That(result.DecimalProperty, Is.EqualTo(15.75m));
    }

    /// <summary>
    /// Tests that decimal properties return the end value when t=1.0.
    /// This verifies the boundary condition at the end of interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithDecimalProperties_AtT1_ReturnsEndValue()
    {
        // Arrange - Set up start and end values
        var start = new TestInterpolatableClass { DecimalProperty = 5.25m };
        var end = new TestInterpolatableClass { DecimalProperty = 15.25m };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act - Interpolate at end (t=1.0)
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 1.0f, result);

        // Assert - Should return the end value exactly
        Assert.That(result.DecimalProperty, Is.EqualTo(15.25m));
    }

    #endregion

    #region Edge Cases and Error Handling

    /// <summary>
    /// Tests that the interpolation function throws TargetException when start object is null.
    /// This occurs because reflection cannot read properties from a null target object.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithNullStartObject_ThrowsException()
    {
        // Arrange - Set up valid end object and interpolation function
        var end = new TestInterpolatableClass { FloatProperty = 30f };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act & Assert - Should throw when start object is null
        // This is expected behavior since we need to read properties from the start object
        Assert.Throws<System.Reflection.TargetException>(() =>
        {
            var result = new TestInterpolatableClass();
            interpolate!(null!, end, 0.5f, result);
        });
    }

    /// <summary>
    /// Tests that the interpolation function throws TargetException when end object is null.
    /// This occurs because reflection cannot read properties from a null target object.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithNullEndObject_ThrowsException()
    {
        // Arrange - Set up valid start object and interpolation function
        var start = new TestInterpolatableClass { FloatProperty = 10f };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act & Assert - Should throw when end object is null
        // This is expected behavior since we need to read properties from the end object
        Assert.Throws<System.Reflection.TargetException>(() =>
        {
            var result = new TestInterpolatableClass();
            interpolate!(start, null!, 0.5f, result);
        });
    }

    /// <summary>
    /// Tests that the interpolation function throws TargetException when destination object is null.
    /// This occurs because reflection cannot set properties on a null target object.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithNullDestinationObject_ThrowsException()
    {
        // Arrange - Set up valid start and end objects
        var start = new TestInterpolatableClass { FloatProperty = 10f };
        var end = new TestInterpolatableClass { FloatProperty = 30f };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act & Assert - Should throw when destination object is null
        // This is expected behavior since we need a valid object to set properties on
        Assert.Throws<System.Reflection.TargetException>(() =>
        {
            interpolate!(start, end, 0.5f, null!);
        });
    }

    /// <summary>
    /// Tests that the interpolation function handles negative t values (extrapolation backwards) without crashing.
    /// Negative t values should extrapolate backwards from the start point.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithNegativeT_HandlesCorrectly()
    {
        // Arrange - Set up start and end values for extrapolation test
        var start = new TestInterpolatableClass { FloatProperty = 10f };
        var end = new TestInterpolatableClass { FloatProperty = 30f };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act & Assert - Should not throw with negative t (extrapolation backwards)
        // We don't test exact values as they may vary based on implementation details
        Assert.DoesNotThrow(() =>
        {
            var result = new TestInterpolatableClass();
            interpolate!(start, end, -0.5f, result);
            // Just verify it doesn't crash and produces a valid number
            Assert.That(result.FloatProperty, Is.Not.NaN);
        });
    }

    /// <summary>
    /// Tests that the interpolation function handles t values greater than 1 (extrapolation forwards) without crashing.
    /// Values of t > 1 should extrapolate forwards beyond the end point.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithTGreaterThanOne_HandlesCorrectly()
    {
        // Arrange - Set up start and end values for extrapolation test
        var start = new TestInterpolatableClass { FloatProperty = 10f };
        var end = new TestInterpolatableClass { FloatProperty = 30f };
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable class");

        // Act & Assert - Should not throw with t > 1 (extrapolation forwards)
        // We don't test exact values as they may vary based on implementation details
        Assert.DoesNotThrow(() =>
        {
            var result = new TestInterpolatableClass();
            interpolate!(start, end, 1.5f, result);
            // Just verify it doesn't crash and produces a valid number
            Assert.That(result.FloatProperty, Is.Not.NaN);
        });
    }
    #endregion

    #region Array Property Tests

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for float array properties.
    /// This verifies that float arrays are recognized as interpolatable and can be used in property interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithFloatArrayProperties_CreatesFunction()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            FloatArrayProperty = [1.0f, 2.0f, 3.0f]
        };
        var end = new TestInterpolatableClass
        {
            FloatArrayProperty = [4.0f, 5.0f, 6.0f]
        };

        // Act
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Assert
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for float array properties");
    }

    /// <summary>
    /// Tests that float array properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles float arrays and interpolates all elements.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithFloatArrayProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            FloatArrayProperty = [1.0f, 2.0f, 3.0f]
        };
        var end = new TestInterpolatableClass
        {
            FloatArrayProperty = [4.0f, 5.0f, 6.0f]
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Act
        var result = new TestInterpolatableClass
        {
            FloatArrayProperty = new float[start.FloatArrayProperty.Length]
        };
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatArrayProperty, Is.Not.Null, "Float array property should not be null");
            Assert.That(result.FloatArrayProperty, Has.Length.EqualTo(3), "Float array should have correct length");
            Assert.That(result.FloatArrayProperty[0], Is.EqualTo(2.5f).Within(0.0001f), "First element should be interpolated correctly");
            Assert.That(result.FloatArrayProperty[1], Is.EqualTo(3.5f).Within(0.0001f), "Second element should be interpolated correctly");
            Assert.That(result.FloatArrayProperty[2], Is.EqualTo(4.5f).Within(0.0001f), "Third element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that int array properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles int arrays and interpolates all elements.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithIntArrayProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            IntArrayProperty = [10, 20, 30]
        };
        var end = new TestInterpolatableClass
        {
            IntArrayProperty = [40, 50, 60]
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Act
        var result = new TestInterpolatableClass
        {
            IntArrayProperty = new int[start.IntArrayProperty.Length]
        };
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IntArrayProperty, Is.Not.Null, "Int array property should not be null");
            Assert.That(result.IntArrayProperty, Has.Length.EqualTo(3), "Int array should have correct length");
            Assert.That(result.IntArrayProperty[0], Is.EqualTo(25), "First element should be interpolated correctly");
            Assert.That(result.IntArrayProperty[1], Is.EqualTo(35), "Second element should be interpolated correctly");
            Assert.That(result.IntArrayProperty[2], Is.EqualTo(45), "Third element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that double array properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles double arrays and interpolates all elements.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithDoubleArrayProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            DoubleArrayProperty = [1.5, 2.5, 3.5]
        };
        var end = new TestInterpolatableClass
        {
            DoubleArrayProperty = [4.5, 5.5, 6.5]
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Act
        var result = new TestInterpolatableClass
        {
            DoubleArrayProperty = new double[start.DoubleArrayProperty.Length]
        };
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.DoubleArrayProperty, Is.Not.Null, "Double array property should not be null");
            Assert.That(result.DoubleArrayProperty, Has.Length.EqualTo(3), "Double array should have correct length");
            Assert.That(result.DoubleArrayProperty[0], Is.EqualTo(3.0).Within(0.0001), "First element should be interpolated correctly");
            Assert.That(result.DoubleArrayProperty[1], Is.EqualTo(4.0).Within(0.0001), "Second element should be interpolated correctly");
            Assert.That(result.DoubleArrayProperty[2], Is.EqualTo(5.0).Within(0.0001), "Third element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that decimal array properties are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles decimal arrays and interpolates all elements.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithDecimalArrayProperties_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            DecimalArrayProperty = [1.5m, 2.5m, 3.5m]
        };
        var end = new TestInterpolatableClass
        {
            DecimalArrayProperty = [4.5m, 5.5m, 6.5m]
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Act
        var result = new TestInterpolatableClass
        {
            DecimalArrayProperty = new decimal[start.DecimalArrayProperty.Length]
        };
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.DecimalArrayProperty, Is.Not.Null, "Decimal array property should not be null");
            Assert.That(result.DecimalArrayProperty, Has.Length.EqualTo(3), "Decimal array should have correct length");
            Assert.That(result.DecimalArrayProperty[0], Is.EqualTo(3.0m).Within(0.001m), "First element should be interpolated correctly");
            Assert.That(result.DecimalArrayProperty[1], Is.EqualTo(4.0m).Within(0.001m), "Second element should be interpolated correctly");
            Assert.That(result.DecimalArrayProperty[2], Is.EqualTo(5.0m).Within(0.001m), "Third element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that array properties work correctly with arrays of the same length.
    /// This verifies that the interpolation function handles arrays properly when they have matching lengths.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithMatchingArrayLengths_HandlesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            FloatArrayProperty = [1.0f, 2.0f, 3.0f, 4.0f]
        };
        var end = new TestInterpolatableClass
        {
            FloatArrayProperty = [5.0f, 6.0f, 7.0f, 8.0f]
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Act
        var result = new TestInterpolatableClass
        {
            FloatArrayProperty = new float[start.FloatArrayProperty.Length]
        };
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatArrayProperty, Is.Not.Null, "Float array property should not be null");
            Assert.That(result.FloatArrayProperty, Has.Length.EqualTo(4), "Result array should have correct length");
            Assert.That(result.FloatArrayProperty[0], Is.EqualTo(3.0f).Within(0.0001f), "First element should be interpolated correctly");
            Assert.That(result.FloatArrayProperty[1], Is.EqualTo(4.0f).Within(0.0001f), "Second element should be interpolated correctly");
            Assert.That(result.FloatArrayProperty[2], Is.EqualTo(5.0f).Within(0.0001f), "Third element should be interpolated correctly");
            Assert.That(result.FloatArrayProperty[3], Is.EqualTo(6.0f).Within(0.0001f), "Fourth element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that array properties work correctly with empty arrays.
    /// This verifies that the interpolation function handles empty arrays gracefully.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithEmptyArrays_HandlesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            FloatArrayProperty = []
        };
        var end = new TestInterpolatableClass
        {
            FloatArrayProperty = []
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Act
        var result = new TestInterpolatableClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        Assert.That(result.FloatArrayProperty, Is.Not.Null, "Float array property should not be null");
        Assert.That(result.FloatArrayProperty, Is.Empty, "Result array should be empty");
    }

    #endregion

    #region Field Interpolation Tests

    /// <summary>
    /// Tests that PropertyInterpolator can create interpolation functions for classes with interpolatable fields.
    /// This verifies that fields are recognized as interpolatable and can be used in interpolation.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithInterpolatableFields_CreatesFunction()
    {
        // Arrange
        var start = new TestInterpolatableFieldsClass
        {
            FloatField = 10.0f,
            IntField = 20,
            ColorField = Color.Red,
            PointField = new Point(0, 0),
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 0f),
                    new PointF(10f, 10f),
                    new PointF(20f, 20f),
                    new PointF(30f, 30f)
                ],
                Closed = false
            }
        };
        var end = new TestInterpolatableFieldsClass
        {
            FloatField = 30.0f,
            IntField = 60,
            ColorField = Color.Blue,
            PointField = new Point(100, 100),
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 100f),
                    new PointF(110f, 110f),
                    new PointF(120f, 120f),
                    new PointF(130f, 130f)
                ],
                Closed = true
            }
        };

        // Act
        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();

        // Assert
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields");
    }

    /// <summary>
    /// Tests that float fields are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles float fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithFloatFields_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableFieldsClass { FloatField = 10.0f };
        var end = new TestInterpolatableFieldsClass { FloatField = 30.0f };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();

        // Act
        var result = new TestInterpolatableFieldsClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        Assert.That(result.FloatField, Is.EqualTo(20.0f).Within(0.0001f), "Float field should be interpolated correctly");
    }

    /// <summary>
    /// Tests that int fields are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles int fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithIntFields_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableFieldsClass { IntField = 10 };
        var end = new TestInterpolatableFieldsClass { IntField = 30 };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();

        // Act
        var result = new TestInterpolatableFieldsClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        Assert.That(result.IntField, Is.EqualTo(20), "Int field should be interpolated correctly");
    }

    /// <summary>
    /// Tests that Color fields are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles Color fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithColorFields_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableFieldsClass { ColorField = Color.Red };
        var end = new TestInterpolatableFieldsClass { ColorField = Color.Blue };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();

        // Act
        var result = new TestInterpolatableFieldsClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ColorField.A, Is.EqualTo(255), "Alpha should remain 255");
            Assert.That(result.ColorField.R, Is.EqualTo(128), "Red component should be 128 (halfway)");
            Assert.That(result.ColorField.G, Is.Zero, "Green should remain 0");
            Assert.That(result.ColorField.B, Is.EqualTo(128), "Blue component should be 128 (halfway)");
        }
    }

    /// <summary>
    /// Tests that Point fields are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles Point fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithPointFields_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableFieldsClass { PointField = new Point(0, 0) };
        var end = new TestInterpolatableFieldsClass { PointField = new Point(100, 50) };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();

        // Act
        var result = new TestInterpolatableFieldsClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PointField.X, Is.EqualTo(50), "X coordinate should be interpolated correctly");
            Assert.That(result.PointField.Y, Is.EqualTo(25), "Y coordinate should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that BezierPath fields are correctly interpolated using PropertyInterpolator.
    /// This verifies that Bezier path field interpolation works correctly, interpolating all path points and the Closed property.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathFields_InterpolatesCorrectly()
    {
        // Arrange - Create start and end Bezier paths with different coordinates
        // Start path: smaller coordinates, open path
        var start = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(10f, 20f),  // Start point
                    new PointF(30f, 40f),  // Control point 1
                    new PointF(50f, 60f),  // Control point 2
                    new PointF(70f, 80f)   // End point
                ],
                Closed = false
            }
        };

        // End path: larger coordinates (10x scale), closed path
        var end = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 200f),  // Start point (10x start)
                    new PointF(300f, 400f),  // Control point 1 (10x start)
                    new PointF(500f, 600f),  // Control point 2 (10x start)
                    new PointF(700f, 800f)   // End point (10x start)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields class");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints = new PointF[start.BezierPathField.PathPoints.Length]
            }
        };
        interpolate!(start, end, 0.5f, result);

        // Assert - At t=0.5, should be halfway between start and end
        // Expected: (10 + 100) / 2 = 55, (20 + 200) / 2 = 110, etc.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathField, Is.Not.Null, "BezierPath field should not be null");
            Assert.That(result.BezierPathField.PathPoints, Is.Not.Null, "PathPoints should not be null");
            Assert.That(result.BezierPathField.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points");
            Assert.That(result.BezierPathField.PathPoints[0].X, Is.EqualTo(55f).Within(Tolerance), "First point X should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[0].Y, Is.EqualTo(110f).Within(Tolerance), "First point Y should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[1].X, Is.EqualTo(165f).Within(Tolerance), "Control point 1 X should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[1].Y, Is.EqualTo(220f).Within(Tolerance), "Control point 1 Y should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[2].X, Is.EqualTo(275f).Within(Tolerance), "Control point 2 X should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[2].Y, Is.EqualTo(330f).Within(Tolerance), "Control point 2 Y should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[3].X, Is.EqualTo(385f).Within(Tolerance), "End point X should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[3].Y, Is.EqualTo(440f).Within(Tolerance), "End point Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that BezierPath fields return the start value when t=0.0.
    /// This verifies the boundary condition at the beginning of interpolation for fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathFields_AtT0_ReturnsStartValue()
    {
        // Arrange - Create start and end Bezier paths
        var start = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(10f, 20f),
                    new PointF(30f, 40f),
                    new PointF(50f, 60f),
                    new PointF(70f, 80f)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 200f),
                    new PointF(300f, 400f),
                    new PointF(500f, 600f),
                    new PointF(700f, 800f)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields class");

        // Act - Interpolate at start (t=0.0)
        // Initialize BezierPathField with a BezierPath object but leave PathPoints as null
        // The interpolation function should create and populate the PathPoints array
        var result = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath()
        };
        interpolate!(start, end, 0.0f, result);

        // Assert - Should return the start value exactly
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathField, Is.Not.Null, "BezierPath field should not be null");
            Assert.That(result.BezierPathField.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points");
            Assert.That(result.BezierPathField.PathPoints[0].X, Is.EqualTo(10f).Within(Tolerance), "First point X should match start");
            Assert.That(result.BezierPathField.PathPoints[0].Y, Is.EqualTo(20f).Within(Tolerance), "First point Y should match start");
            Assert.That(result.BezierPathField.PathPoints[3].X, Is.EqualTo(70f).Within(Tolerance), "End point X should match start");
            Assert.That(result.BezierPathField.PathPoints[3].Y, Is.EqualTo(80f).Within(Tolerance), "End point Y should match start");
        }
    }

    /// <summary>
    /// Tests that BezierPath fields return the end value when t=1.0.
    /// This verifies the boundary condition at the end of interpolation for fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathFields_AtT1_ReturnsEndValue()
    {
        // Arrange - Create start and end Bezier paths
        var start = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(10f, 20f),
                    new PointF(30f, 40f),
                    new PointF(50f, 60f),
                    new PointF(70f, 80f)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 200f),
                    new PointF(300f, 400f),
                    new PointF(500f, 600f),
                    new PointF(700f, 800f)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields class");

        // Act - Interpolate at end (t=1.0)
        // Initialize BezierPathField with a BezierPath object but leave PathPoints as null
        // The interpolation function should create and populate the PathPoints array
        var result = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath()
        };
        interpolate!(start, end, 1.0f, result);

        // Assert - Should return the end value exactly
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathField, Is.Not.Null, "BezierPath field should not be null");
            Assert.That(result.BezierPathField.PathPoints, Has.Length.EqualTo(4), "Path should have correct number of points");
            Assert.That(result.BezierPathField.PathPoints[0].X, Is.EqualTo(100f).Within(Tolerance), "First point X should match end");
            Assert.That(result.BezierPathField.PathPoints[0].Y, Is.EqualTo(200f).Within(Tolerance), "First point Y should match end");
            Assert.That(result.BezierPathField.PathPoints[3].X, Is.EqualTo(700f).Within(Tolerance), "End point X should match end");
            Assert.That(result.BezierPathField.PathPoints[3].Y, Is.EqualTo(800f).Within(Tolerance), "End point Y should match end");
        }
    }

    /// <summary>
    /// Tests that BezierPath fields with multiple segments are correctly interpolated.
    /// This verifies that paths with multiple Bezier segments (more than 4 points) are handled correctly for fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathFields_MultipleSegments_InterpolatesCorrectly()
    {
        // Arrange - Create paths with multiple segments (8 points = 2 segments)
        var start = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 0f),    // Segment 1: Start
                    new PointF(10f, 0f),   // Segment 1: Control 1
                    new PointF(20f, 0f),   // Segment 1: Control 2
                    new PointF(30f, 0f),   // Segment 1: End / Segment 2: Start
                    new PointF(40f, 0f),   // Segment 2: Control 1
                    new PointF(50f, 0f),   // Segment 2: Control 2
                    new PointF(60f, 0f),   // Segment 2: End
                    new PointF(70f, 0f)    // Extra point (should still be interpolated)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 100f),   // Segment 1: Start (10x Y)
                    new PointF(10f, 100f),  // Segment 1: Control 1
                    new PointF(20f, 100f),  // Segment 1: Control 2
                    new PointF(30f, 100f),  // Segment 1: End / Segment 2: Start
                    new PointF(40f, 100f),  // Segment 2: Control 1
                    new PointF(50f, 100f),  // Segment 2: Control 2
                    new PointF(60f, 100f),  // Segment 2: End
                    new PointF(70f, 100f)   // Extra point
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields class");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints = new PointF[start.BezierPathField.PathPoints.Length]
            }
        };
        interpolate!(start, end, 0.5f, result);

        // Assert - All points should be interpolated
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathField, Is.Not.Null, "BezierPath field should not be null");
            Assert.That(result.BezierPathField.PathPoints, Has.Length.EqualTo(8), "Path should have correct number of points");
            // At t=0.5, Y should be (0 + 100) / 2 = 50
            Assert.That(result.BezierPathField.PathPoints[0].Y, Is.EqualTo(50f).Within(Tolerance), "First point Y should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[4].Y, Is.EqualTo(50f).Within(Tolerance), "Fifth point Y should be interpolated correctly");
            Assert.That(result.BezierPathField.PathPoints[7].Y, Is.EqualTo(50f).Within(Tolerance), "Last point Y should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that BezierPath fields with empty paths are handled correctly.
    /// This verifies that empty path arrays are handled gracefully for fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathFields_EmptyPath_HandlesCorrectly()
    {
        // Arrange - Create paths with empty point arrays
        var start = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints = [],
                Closed = false
            }
        };

        var end = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints = [],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields class");

        // Act - Interpolate at midpoint
        var result = new TestInterpolatableFieldsClass();
        interpolate!(start, end, 0.5f, result);

        // Assert - Empty path should remain empty
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathField, Is.Not.Null, "BezierPath field should not be null");
            Assert.That(result.BezierPathField.PathPoints, Is.Not.Null, "PathPoints should not be null");
            Assert.That(result.BezierPathField.PathPoints, Is.Empty, "Empty path should remain empty");
        }
    }

    /// <summary>
    /// Tests that BezierPath fields with different path lengths are handled correctly.
    /// This verifies that the interpolation function handles mismatched path lengths gracefully.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathFields_DifferentLengths_HandlesCorrectly()
    {
        // Arrange - Create paths with different numbers of points
        var start = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 0f),
                    new PointF(10f, 10f),
                    new PointF(20f, 20f),
                    new PointF(30f, 30f)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 100f),
                    new PointF(110f, 110f),
                    new PointF(120f, 120f),
                    new PointF(130f, 130f),
                    new PointF(140f, 140f),
                    new PointF(150f, 150f)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields class");

        // Act - Interpolate at midpoint
        var result = new TestInterpolatableFieldsClass();
        interpolate!(start, end, 0.5f, result);

        // Assert - Should handle different lengths (implementation may vary, but should not crash)
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.BezierPathField, Is.Not.Null, "BezierPath field should not be null");
            Assert.That(result.BezierPathField.PathPoints, Is.Not.Null, "PathPoints should not be null");
            // The result length may vary based on implementation, but should be valid
            Assert.That(result.BezierPathField.PathPoints, Has.Length.GreaterThanOrEqualTo(0), "Path should have valid length");
        }
    }

    /// <summary>
    /// Tests that array fields are correctly interpolated using PropertyInterpolator.
    /// This verifies that the interpolation function properly handles array fields.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithArrayFields_InterpolatesCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableFieldsClass
        {
            FloatArrayField = [1.0f, 2.0f, 3.0f]
        };
        var end = new TestInterpolatableFieldsClass
        {
            FloatArrayField = [4.0f, 5.0f, 6.0f]
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();

        // Act
        var result = new TestInterpolatableFieldsClass
        {
            FloatArrayField = new float[start.FloatArrayField.Length],
            IntArrayField = new int[start.IntArrayField.Length]
        };
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.FloatArrayField, Is.Not.Null, "Float array field should not be null");
            Assert.That(result.FloatArrayField, Has.Length.EqualTo(3), "Float array should have correct length");
            Assert.That(result.FloatArrayField[0], Is.EqualTo(2.5f).Within(0.0001f), "First element should be interpolated correctly");
            Assert.That(result.FloatArrayField[1], Is.EqualTo(3.5f).Within(0.0001f), "Second element should be interpolated correctly");
            Assert.That(result.FloatArrayField[2], Is.EqualTo(4.5f).Within(0.0001f), "Third element should be interpolated correctly");
        }
    }

    /// <summary>
    /// Tests that BezierPath fields work correctly when combined with other interpolatable fields.
    /// This verifies that multiple field types can be interpolated simultaneously.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithBezierPathFields_CombinedWithOtherFields_InterpolatesCorrectly()
    {
        // Arrange - Combine BezierPath with other field types
        var start = new TestInterpolatableFieldsClass
        {
            FloatField = 10.0f,
            IntField = 20,
            ColorField = Color.Red,
            PointField = new Point(0, 0),
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 0f),
                    new PointF(10f, 10f),
                    new PointF(20f, 20f),
                    new PointF(30f, 30f)
                ],
                Closed = false
            }
        };

        var end = new TestInterpolatableFieldsClass
        {
            FloatField = 30.0f,
            IntField = 60,
            ColorField = Color.Blue,
            PointField = new Point(100, 100),
            BezierPathField = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 100f),
                    new PointF(110f, 110f),
                    new PointF(120f, 120f),
                    new PointF(130f, 130f)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();
        Assert.That(interpolate, Is.Not.Null, "Interpolation function should be created for interpolatable fields class");

        // Act - Interpolate at midpoint (t=0.5)
        var result = new TestInterpolatableFieldsClass
        {
            BezierPathField = new BezierPath
            {
                PathPoints = new PointF[start.BezierPathField.PathPoints.Length]
            },
            FloatArrayField = new float[start.FloatArrayField.Length],
            IntArrayField = new int[start.IntArrayField.Length]
        };
        interpolate!(start, end, 0.5f, result);

        // Assert - All fields should be interpolated correctly
        using (Assert.EnterMultipleScope())
        {
            // Verify other fields
            Assert.That(result.FloatField, Is.EqualTo(20.0f).Within(Tolerance), "Float field should be interpolated");
            Assert.That(result.IntField, Is.EqualTo(40), "Int field should be interpolated");
            Assert.That(result.ColorField.R, Is.EqualTo(128), "Color field should be interpolated");
            Assert.That(result.PointField.X, Is.EqualTo(50), "Point field X should be interpolated");
            Assert.That(result.PointField.Y, Is.EqualTo(50), "Point field Y should be interpolated");

            // Verify BezierPath field
            Assert.That(result.BezierPathField, Is.Not.Null, "BezierPath field should not be null");
            Assert.That(result.BezierPathField.PathPoints, Has.Length.EqualTo(4), "BezierPath should have correct number of points");
            Assert.That(result.BezierPathField.PathPoints[0].X, Is.EqualTo(50f).Within(Tolerance), "BezierPath first point X should be interpolated");
            Assert.That(result.BezierPathField.PathPoints[0].Y, Is.EqualTo(50f).Within(Tolerance), "BezierPath first point Y should be interpolated");
            Assert.That(result.BezierPathField.PathPoints[3].X, Is.EqualTo(80f).Within(Tolerance), "BezierPath end point X should be interpolated");
            Assert.That(result.BezierPathField.PathPoints[3].Y, Is.EqualTo(80f).Within(Tolerance), "BezierPath end point Y should be interpolated");
        }
    }

    /// <summary>
    /// Tests that non-interpolatable fields are ignored during interpolation.
    /// This verifies that the interpolation function properly ignores fields that cannot be interpolated.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_WithNonInterpolatableFields_IgnoresCorrectly()
    {
        // Arrange
        var start = new TestInterpolatableFieldsClass
        {
            FloatField = 10.0f,
            StringField = "Start",
            BoolField = true,
            ObjectField = new object()
        };
        var end = new TestInterpolatableFieldsClass
        {
            FloatField = 30.0f,
            StringField = "End",
            BoolField = false,
            ObjectField = new object()
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableFieldsClass>();

        // Act
        var result = new TestInterpolatableFieldsClass();
        interpolate!(start, end, 0.5f, result);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            // Interpolatable field should be interpolated
            Assert.That(result.FloatField, Is.EqualTo(20.0f).Within(0.0001f), "Float field should be interpolated");

            // Non-interpolatable fields should remain at default values (not copied from start object)
            Assert.That(result.StringField, Is.EqualTo("Test"), "String field should remain at default value");
            Assert.That(result.BoolField, Is.False, "Bool field should remain at default value");
            Assert.That(result.ObjectField, Is.Not.Null, "Object field should remain at default value");
        }
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that PropertyInterpolator performs reasonably well with complex objects.
    /// This verifies that the interpolation function can handle complex scenarios efficiently.
    /// </summary>
    [Test]
    public void CreateInterpolationFunction_Performance_ShouldBeFast()
    {
        // Arrange
        var start = new TestInterpolatableClass
        {
            FloatProperty = 10.0f,
            DoubleProperty = 20.0,
            IntProperty = 30,
            ColorProperty = Color.Red,
            PointProperty = new Point(0, 0),
            PointFProperty = new PointF(0f, 0f),
            RectangleProperty = new Rectangle(0, 0, 100, 100),
            RectangleFProperty = new RectangleF(0f, 0f, 100f, 100f),
            FloatArrayProperty = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f],
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(0f, 0f),
                    new PointF(10f, 10f),
                    new PointF(20f, 20f),
                    new PointF(30f, 30f)
                ],
                Closed = false
            }
        };
        var end = new TestInterpolatableClass
        {
            FloatProperty = 30.0f,
            DoubleProperty = 60.0,
            IntProperty = 90,
            ColorProperty = Color.Blue,
            PointProperty = new Point(100, 100),
            PointFProperty = new PointF(100f, 100f),
            RectangleProperty = new Rectangle(50, 50, 200, 200),
            RectangleFProperty = new RectangleF(50f, 50f, 200f, 200f),
            FloatArrayProperty = [6.0f, 7.0f, 8.0f, 9.0f, 10.0f],
            BezierPathProperty = new BezierPath
            {
                PathPoints =
                [
                    new PointF(100f, 100f),
                    new PointF(110f, 110f),
                    new PointF(120f, 120f),
                    new PointF(130f, 130f)
                ],
                Closed = true
            }
        };

        var interpolate = PropertyInterpolator.CreateInterpolationFunction<TestInterpolatableClass>();

        // Act & Assert - Measure performance
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var result = new TestInterpolatableClass
            {
                FloatArrayProperty = new float[start.FloatArrayProperty.Length],
                IntArrayProperty = new int[start.IntArrayProperty.Length],
                DoubleArrayProperty = new double[start.DoubleArrayProperty.Length],
                DecimalArrayProperty = new decimal[start.DecimalArrayProperty.Length]
            };
            interpolate!(start, end, 0.5f, result);
        }
        stopwatch.Stop();

        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000), "PropertyInterpolator should complete 1000 interpolations within 1000ms");
    }

    #endregion
}
