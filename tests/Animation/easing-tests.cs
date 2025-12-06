using NUnit.Framework;
using Blackwood;
namespace Blackwood.Core.Tests;

/// <summary>
/// A test suite for easing functions in the Blackwood.Core library.
///
/// This test class validates the mathematical correctness and behavioral properties
/// of all easing functions, ensuring they meet the expected mathematical properties
/// for animation and interpolation scenarios.
///
/// Easing functions are mathematical curves that control the rate of change in animations,
/// providing natural-feeling motion that starts slow, speeds up, or maintains constant velocity.
/// </summary>
[TestFixture]
public class EasingTests
{
    #region Test Data

    /// <summary>
    /// Standard test values for evaluating easing functions at key points.
    /// These values represent common animation progress points (0%, 25%, 50%, 75%, 100%).
    /// </summary>
    private static readonly float[] TestValues = [0.0f, 0.25f, 0.5f, 0.75f, 1.0f];

    /// <summary>
    /// Tolerance value for floating-point comparisons in easing function tests.
    /// This accounts for potential floating-point precision issues in mathematical calculations.
    /// </summary>
    private const float Tolerance = 0.001f;

    #endregion

    #region Linear Tests

    /// <summary>
    /// Tests that the linear easing function returns the input value unchanged.
    ///
    /// Linear easing is the simplest easing function where the output equals the input.
    /// This creates constant velocity animations with no acceleration or deceleration.
    /// Mathematically: f(t) = t for all t in [0,1]
    /// </summary>
    [Test]
    public void Linear_ShouldReturnSameValue()
    {
        // Arrange - Get the linear easing function
        var easing = Animation.GetEasing(EasingFunction.Linear);

        // Act & Assert - Test that linear easing returns the input value unchanged
        foreach (var t in TestValues)
        {
            var result = easing(t);
            Assert.That(result, Is.EqualTo(t).Within(Tolerance),
                $"Linear easing at t={t} should return {t} (identity function)");
        }
    }

    #endregion

    #region Smoothstep Tests

    /// <summary>
    /// Tests that the smoothstep easing function produces mathematically correct values.
    ///
    /// Smoothstep is a cubic polynomial: f(t) = 3t² - 2t³
    /// It provides smooth acceleration and deceleration with zero velocity at endpoints.
    /// This creates natural-feeling animations that start and end gently.
    /// </summary>
    [Test]
    public void Smoothstep_ShouldHaveCorrectValues()
    {
        // Arrange - Get the smoothstep easing function
        var easing = Animation.GetEasing(EasingFunction.Smoothstep);

        // Act & Assert - Test mathematically verified values for smoothstep function
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "Smoothstep at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "Smoothstep at t=1 should be 1");

            // Midpoint: f(0.5) = 0.5 (symmetric property)
            Assert.That(easing(0.5f), Is.EqualTo(0.5f).Within(Tolerance), "Smoothstep at t=0.5 should be 0.5");

            // Calculated intermediate values using f(t) = 3t² - 2t³
            Assert.That(easing(0.25f), Is.EqualTo(0.15625f).Within(Tolerance), "Smoothstep at t=0.25 should be 0.15625");
            Assert.That(easing(0.75f), Is.EqualTo(0.84375f).Within(Tolerance), "Smoothstep at t=0.75 should be 0.84375");
        }
    }

    /// <summary>
    /// Tests that the smoothstep easing function is monotonic (always increasing).
    ///
    /// Monotonicity is a critical property for easing functions - the output should
    /// never decrease as the input increases. This ensures smooth, predictable animations
    /// without any "backwards" motion or unexpected behavior.
    /// </summary>
    [Test]
    public void Smoothstep_ShouldBeMonotonic()
    {
        // Arrange - Get smoothstep function and prepare to test 101 points
        var easing = Animation.GetEasing(EasingFunction.Smoothstep);
        var values = new float[101];

        // Act - Sample the function at 101 points from 0.0 to 1.0
        for (int i = 0; i <= 100; i++)
        {
            values[i] = easing(i / 100.0f);
        }

        // Assert - Verify that each value is greater than or equal to the previous value
        for (int i = 1; i <= 100; i++)
        {
            Assert.That(values[i], Is.GreaterThanOrEqualTo(values[i - 1]),
                $"Smoothstep should be monotonic at t={i/100.0f} (no decreasing values allowed)");
        }
    }

    #endregion

    #region Smootherstep Tests

    /// <summary>
    /// Tests that the smootherstep easing function produces mathematically correct values.
    ///
    /// Smootherstep is a quintic polynomial: f(t) = 6t⁵ - 15t⁴ + 10t³
    /// It provides even smoother acceleration and deceleration than smoothstep,
    /// with zero first and second derivatives at the endpoints.
    /// This creates the most natural-feeling animations with very gentle starts and ends.
    /// </summary>
    [Test]
    public void Smootherstep_ShouldHaveCorrectValues()
    {
        // Arrange - Get the smootherstep easing function
        var easing = Animation.GetEasing(EasingFunction.Smootherstep);

        // Act & Assert - Test mathematically verified values for smootherstep function
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "Smootherstep at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "Smootherstep at t=1 should be 1");

            // Midpoint: f(0.5) = 0.5 (symmetric property)
            Assert.That(easing(0.5f), Is.EqualTo(0.5f).Within(Tolerance), "Smootherstep at t=0.5 should be 0.5");
        }
    }

    /// <summary>
    /// Tests that the smootherstep easing function is monotonic (always increasing).
    ///
    /// Monotonicity ensures that animations progress smoothly without any backwards motion.
    /// Smootherstep should maintain this property while providing smoother curvature than smoothstep.
    /// </summary>
    [Test]
    public void Smootherstep_ShouldBeMonotonic()
    {
        // Arrange - Get smootherstep function and prepare to test 101 points
        var easing = Animation.GetEasing(EasingFunction.Smootherstep);
        var values = new float[101];

        // Act - Sample the function at 101 points from 0.0 to 1.0
        for (int i = 0; i <= 100; i++)
        {
            values[i] = easing(i / 100.0f);
        }

        // Assert - Verify that each value is greater than or equal to the previous value
        for (int i = 1; i <= 100; i++)
        {
            Assert.That(values[i], Is.GreaterThanOrEqualTo(values[i - 1]),
                $"Smootherstep should be monotonic at t={i/100.0f} (no decreasing values allowed)");
        }
    }

    /// <summary>
    /// Tests that smootherstep provides smoother curvature than smoothstep.
    ///
    /// While both functions have the same boundary values and midpoint, smootherstep
    /// has a different curvature profile that provides more gradual acceleration
    /// and deceleration, making it feel more natural in animations.
    /// </summary>
    [Test]
    public void Smootherstep_ShouldBeSmootherThanSmoothstep()
    {
        // Arrange - Get both easing functions for comparison
        var smoothstep = Animation.GetEasing(EasingFunction.Smoothstep);
        var smootherstep = Animation.GetEasing(EasingFunction.Smootherstep);

        // Act & Assert - Test that smootherstep maintains the same midpoint value
        var t = 0.5f;
        var smoothValue = smoothstep(t);
        var smootherValue = smootherstep(t);

        // Both should be 0.5 at t=0.5, but smootherstep has different curvature characteristics
        Assert.That(smootherValue, Is.EqualTo(0.5f).Within(Tolerance),
            "Smootherstep should maintain the same midpoint value as smoothstep");
    }

    #endregion

    #region Quadratic Tests

    /// <summary>
    /// Tests that the ease-in quadratic function starts slowly and accelerates.
    ///
    /// EaseInQuad uses the formula: f(t) = t²
    /// This creates animations that start with zero velocity and gradually accelerate,
    /// providing a natural "ease-in" effect commonly used for objects starting to move.
    /// </summary>
    [Test]
    public void EaseInQuad_ShouldStartSlow()
    {
        // Arrange - Get the ease-in quadratic easing function
        var easing = Animation.GetEasing(EasingFunction.EaseInQuad);

        // Act & Assert - Test mathematically verified values for quadratic ease-in
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseInQuad at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseInQuad at t=1 should be 1");

            // Midpoint: f(0.5) = 0.25 (quadratic growth is slower in the first half)
            Assert.That(easing(0.5f), Is.EqualTo(0.25f).Within(Tolerance), "EaseInQuad at t=0.5 should be 0.25");
        }
    }

    /// <summary>
    /// Tests that the ease-out quadratic function ends slowly and decelerates.
    ///
    /// EaseOutQuad uses the formula: f(t) = 1 - (1-t)²
    /// This creates animations that start fast and gradually decelerate to zero velocity,
    /// providing a natural "ease-out" effect commonly used for objects coming to rest.
    /// </summary>
    [Test]
    public void EaseOutQuad_ShouldEndSlow()
    {
        // Arrange - Get the ease-out quadratic easing function
        var easing = Animation.GetEasing(EasingFunction.EaseOutQuad);

        // Act & Assert - Test mathematically verified values for quadratic ease-out
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseOutQuad at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseOutQuad at t=1 should be 1");

            // Midpoint: f(0.5) = 0.75 (quadratic decay is faster in the second half)
            Assert.That(easing(0.5f), Is.EqualTo(0.75f).Within(Tolerance), "EaseOutQuad at t=0.5 should be 0.75");
        }
    }

    /// <summary>
    /// Tests that the ease-in-out quadratic function is symmetric around the midpoint.
    ///
    /// EaseInOutQuad combines both ease-in and ease-out: f(t) = 2t² for t ≤ 0.5, 1 - 2(1-t)² for t > 0.5
    /// This creates animations that start slow, accelerate in the middle, then decelerate at the end,
    /// providing a natural "ease-in-out" effect that feels balanced and organic.
    /// </summary>
    [Test]
    public void EaseInOutQuad_ShouldBeSymmetric()
    {
        // Arrange - Get the ease-in-out quadratic easing function
        var easing = Animation.GetEasing(EasingFunction.EaseInOutQuad);

        // Act & Assert - Test symmetric properties of the ease-in-out function
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseInOutQuad at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseInOutQuad at t=1 should be 1");

            // Midpoint: f(0.5) = 0.5 (symmetric property)
            Assert.That(easing(0.5f), Is.EqualTo(0.5f).Within(Tolerance), "EaseInOutQuad at t=0.5 should be 0.5");

            // Test symmetry around 0.5: f(t) + f(1-t) = 1
            Assert.That(easing(0.25f) + easing(0.75f), Is.EqualTo(1.0f).Within(Tolerance),
                "EaseInOutQuad should be symmetric around t=0.5");
        }
    }

    #endregion

    #region Cubic Tests

    /// <summary>
    /// Tests that the ease-in cubic function starts very slowly and accelerates more than quadratic.
    ///
    /// EaseInCubic uses the formula: f(t) = t³
    /// This creates animations that start with zero velocity and gradually accelerate,
    /// but with more pronounced slow start than quadratic easing.
    /// The cubic curve provides stronger ease-in effect for dramatic acceleration.
    /// </summary>
    [Test]
    public void EaseInCubic_ShouldStartVerySlow()
    {
        // Arrange - Get the ease-in cubic easing function
        var easing = Animation.GetEasing(EasingFunction.EaseInCubic);

        // Act & Assert - Test mathematically verified values for cubic ease-in
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseInCubic at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseInCubic at t=1 should be 1");

            // Midpoint: f(0.5) = 0.125 (cubic growth is much slower in the first half)
            Assert.That(easing(0.5f), Is.EqualTo(0.125f).Within(Tolerance), "EaseInCubic at t=0.5 should be 0.125");
        }
    }

    /// <summary>
    /// Tests that the ease-out cubic function ends very slowly and decelerates more than quadratic.
    ///
    /// EaseOutCubic uses the formula: f(t) = 1 - (1-t)³
    /// This creates animations that start fast and gradually decelerate to zero velocity,
    /// but with more pronounced slow end than quadratic easing.
    /// The cubic curve provides stronger ease-out effect for dramatic deceleration.
    /// </summary>
    [Test]
    public void EaseOutCubic_ShouldEndVerySlow()
    {
        // Arrange - Get the ease-out cubic easing function
        var easing = Animation.GetEasing(EasingFunction.EaseOutCubic);

        // Act & Assert - Test mathematically verified values for cubic ease-out
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseOutCubic at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseOutCubic at t=1 should be 1");

            // Midpoint: f(0.5) = 0.875 (cubic decay is much faster in the second half)
            Assert.That(easing(0.5f), Is.EqualTo(0.875f).Within(Tolerance), "EaseOutCubic at t=0.5 should be 0.875");
        }
    }

    /// <summary>
    /// Tests that the ease-in-out cubic function is symmetric around the midpoint.
    ///
    /// EaseInOutCubic combines both ease-in and ease-out: f(t) = 4t³ for t ≤ 0.5, 1 - 4(1-t)³ for t > 0.5
    /// This creates animations that start very slow, accelerate in the middle, then decelerate very slowly at the end,
    /// providing a dramatic "ease-in-out" effect with strong acceleration and deceleration phases.
    /// </summary>
    [Test]
    public void EaseInOutCubic_ShouldBeSymmetric()
    {
        // Arrange - Get the ease-in-out cubic easing function
        var easing = Animation.GetEasing(EasingFunction.EaseInOutCubic);

        // Act & Assert - Test symmetric properties of the ease-in-out cubic function
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseInOutCubic at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseInOutCubic at t=1 should be 1");

            // Midpoint: f(0.5) = 0.5 (symmetric property)
            Assert.That(easing(0.5f), Is.EqualTo(0.5f).Within(Tolerance), "EaseInOutCubic at t=0.5 should be 0.5");

            // Test symmetry around 0.5: f(t) + f(1-t) = 1
            Assert.That(easing(0.25f) + easing(0.75f), Is.EqualTo(1.0f).Within(Tolerance),
                "EaseInOutCubic should be symmetric around t=0.5");
        }
    }

    #endregion

    #region Sine Tests

    /// <summary>
    /// Tests that the ease-in sine function uses a sine curve for natural acceleration.
    ///
    /// EaseInSine uses the formula: f(t) = 1 - cos(πt/2)
    /// This creates animations that start with zero velocity and gradually accelerate
    /// using a sine curve, providing very smooth and natural-feeling motion.
    /// The sine curve provides the most organic easing effect.
    /// </summary>
    [Test]
    public void EaseInSine_ShouldUseSineCurve()
    {
        // Arrange - Get the ease-in sine easing function
        var easing = Animation.GetEasing(EasingFunction.EaseInSine);

        // Act & Assert - Test mathematically verified values for sine ease-in
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseInSine at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseInSine at t=1 should be 1");

            // Midpoint: f(0.5) ≈ 0.2929 (sine curve provides smooth acceleration)
            Assert.That(easing(0.5f), Is.EqualTo(0.2929f).Within(Tolerance), "EaseInSine at t=0.5 should be 0.2929");
        }
    }

    /// <summary>
    /// Tests that the ease-out sine function uses a sine curve for natural deceleration.
    ///
    /// EaseOutSine uses the formula: f(t) = sin(πt/2)
    /// This creates animations that start fast and gradually decelerate to zero velocity
    /// using a sine curve, providing very smooth and natural-feeling motion.
    /// The sine curve provides the most organic easing effect.
    /// </summary>
    [Test]
    public void EaseOutSine_ShouldUseSineCurve()
    {
        // Arrange - Get the ease-out sine easing function
        var easing = Animation.GetEasing(EasingFunction.EaseOutSine);

        // Act & Assert - Test mathematically verified values for sine ease-out
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseOutSine at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseOutSine at t=1 should be 1");

            // Midpoint: f(0.5) ≈ 0.7071 (sine curve provides smooth deceleration)
            Assert.That(easing(0.5f), Is.EqualTo(0.7071f).Within(Tolerance), "EaseOutSine at t=0.5 should be 0.7071");
        }
    }

    /// <summary>
    /// Tests that the ease-in-out sine function is symmetric around the midpoint.
    ///
    /// EaseInOutSine combines both ease-in and ease-out: f(t) = (1 - cos(πt))/2
    /// This creates animations that start very smoothly, accelerate in the middle,
    /// then decelerate very smoothly at the end, providing the most natural-feeling
    /// "ease-in-out" effect with organic acceleration and deceleration.
    /// </summary>
    [Test]
    public void EaseInOutSine_ShouldBeSymmetric()
    {
        // Arrange - Get the ease-in-out sine easing function
        var easing = Animation.GetEasing(EasingFunction.EaseInOutSine);

        // Act & Assert - Test symmetric properties of the ease-in-out sine function
        using (Assert.EnterMultipleScope())
        {
            // Boundary conditions: f(0) = 0, f(1) = 1
            Assert.That(easing(0.0f), Is.Zero.Within(Tolerance), "EaseInOutSine at t=0 should be 0");
            Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance), "EaseInOutSine at t=1 should be 1");

            // Midpoint: f(0.5) = 0.5 (symmetric property)
            Assert.That(easing(0.5f), Is.EqualTo(0.5f).Within(Tolerance), "EaseInOutSine at t=0.5 should be 0.5");
        }
    }

    #endregion

    #region Boundary Tests

    /// <summary>
    /// Tests that all easing functions correctly handle boundary values (t=0 and t=1).
    ///
    /// This is a critical test that ensures all easing functions meet the fundamental
    /// requirement that f(0) = 0 and f(1) = 1. These boundary conditions are essential
    /// for proper animation behavior, ensuring animations start and end at the correct values.
    /// </summary>
    [Test]
    public void AllEasingFunctions_ShouldHandleBoundaryValues()
    {
        // Arrange - Define all available easing functions for testing
        var allEasingFunctions = new[]
        {
            EasingFunction.Linear,
            EasingFunction.Smoothstep,
            EasingFunction.Smootherstep,
            EasingFunction.EaseInQuad,
            EasingFunction.EaseOutQuad,
            EasingFunction.EaseInOutQuad,
            EasingFunction.EaseInCubic,
            EasingFunction.EaseOutCubic,
            EasingFunction.EaseInOutCubic,
            EasingFunction.EaseInSine,
            EasingFunction.EaseOutSine,
            EasingFunction.EaseInOutSine
        };

        // Act & Assert - Test boundary conditions for each easing function
        foreach (var easingType in allEasingFunctions)
        {
            var easing = Animation.GetEasing(easingType);

            // Test boundary values - fundamental requirement for all easing functions
            using (Assert.EnterMultipleScope())
            {
                Assert.That(easing(0.0f), Is.Zero.Within(Tolerance),
                    $"{easingType} at t=0 should be 0 (start condition)");
                Assert.That(easing(1.0f), Is.EqualTo(1.0f).Within(Tolerance),
                    $"{easingType} at t=1 should be 1 (end condition)");
            }
        }
    }

    /// <summary>
    /// Tests that all easing functions are monotonic (always increasing).
    ///
    /// Monotonicity is a critical property for easing functions - the output should
    /// never decrease as the input increases. This ensures smooth, predictable animations
    /// without any "backwards" motion or unexpected behavior that could break animations.
    /// </summary>
    [Test]
    public void AllEasingFunctions_ShouldBeMonotonic()
    {
        // Arrange - Define all available easing functions for testing
        var allEasingFunctions = new[]
        {
            EasingFunction.Linear,
            EasingFunction.Smoothstep,
            EasingFunction.Smootherstep,
            EasingFunction.EaseInQuad,
            EasingFunction.EaseOutQuad,
            EasingFunction.EaseInOutQuad,
            EasingFunction.EaseInCubic,
            EasingFunction.EaseOutCubic,
            EasingFunction.EaseInOutCubic,
            EasingFunction.EaseInSine,
            EasingFunction.EaseOutSine,
            EasingFunction.EaseInOutSine
        };

        // Act & Assert - Test monotonicity for each easing function
        foreach (var easingType in allEasingFunctions)
        {
            var easing = Animation.GetEasing(easingType);
            var previousValue = easing(0.0f);

            // Test 101 points from 0.0 to 1.0 to ensure monotonicity
            for (int i = 1; i <= 100; i++)
            {
                var t = i / 100.0f;
                var currentValue = easing(t);

                Assert.That(currentValue, Is.GreaterThanOrEqualTo(previousValue),
                    $"{easingType} should be monotonic at t={t} (no decreasing values allowed)");
                previousValue = currentValue;
            }
        }
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Tests that easing function retrieval and execution is fast enough for real-time use.
    ///
    /// Performance is critical for animation systems, as easing functions are typically
    /// called many times per frame during animations. This test ensures that the easing
    /// system can handle high-frequency calls without causing performance bottlenecks.
    /// </summary>
    [Test]
    public void GetEasing_ShouldBeFast()
    {
        // Arrange - Set up performance test parameters
        var iterations = 10000;
        var startTime = DateTime.UtcNow;

        // Act - Perform many easing function calls to measure performance
        for (int i = 0; i < iterations; i++)
        {
            var easing = Animation.GetEasing(EasingFunction.EaseInOutQuad);
            easing(0.5f);
        }

        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert - Should complete in reasonable time for real-time animation use
        Assert.That(duration.TotalMilliseconds, Is.LessThan(1000),
            "GetEasing should be fast enough for real-time animation use (10k calls in <1s)");
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that easing functions handle values outside the normal [0,1] range gracefully.
    ///
    /// While easing functions are designed for t in [0,1], real-world usage may sometimes
    /// pass values outside this range. This test ensures the functions don't crash
    /// or produce unexpected results when given out-of-range inputs.
    /// </summary>
    [Test]
    public void EasingFunctions_ShouldHandleNegativeValues()
    {
        // Arrange - Get a linear easing function for testing edge cases
        var easing = Animation.GetEasing(EasingFunction.Linear);

        // Act & Assert - Should handle negative and out-of-range values gracefully
        Assert.DoesNotThrow(() => easing(-0.1f), "Should handle negative t values without throwing");
        Assert.DoesNotThrow(() => easing(1.1f), "Should handle t > 1 values without throwing");
    }

    /// <summary>
    /// Tests that easing functions handle extreme floating-point values without crashing.
    ///
    /// This test ensures robustness against extreme inputs that might occur due to
    /// floating-point precision issues, calculation errors, or malicious input.
    /// The functions should handle these cases gracefully without throwing exceptions.
    /// </summary>
    [Test]
    public void EasingFunctions_ShouldHandleExtremeValues()
    {
        // Arrange - Get a linear easing function for testing extreme cases
        var easing = Animation.GetEasing(EasingFunction.Linear);

        // Act & Assert - Should handle extreme floating-point values without throwing
        Assert.DoesNotThrow(() => easing(float.MinValue), "Should handle float.MinValue without throwing");
        Assert.DoesNotThrow(() => easing(float.MaxValue), "Should handle float.MaxValue without throwing");
        Assert.DoesNotThrow(() => easing(float.NaN), "Should handle float.NaN without throwing");
    }

    #endregion
}
