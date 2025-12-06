// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Drawing;
using Sys = System.Math;
namespace Blackwood;

/// <summary>
/// Provides linear interpolation (lerp) functionality for various data types.
/// Supports numeric types, graphics types (Color, Point, Rectangle), and
/// easing functions.
/// </summary>
/// <remarks>
/// This class offers both simple linear interpolation and advanced easing
/// functions for creating smooth animations and transitions. All interpolation
/// methods expect a time factor 't' between 0.0 and 1.0, where 0.0 represents
/// the start value and 1.0 represents the end value.
///
/// I'm not a fan of the nane `Lerp` but its so standard, I'm sticking with it.
/// </remarks>
/// <example>
/// <code>
/// // Basic interpolation
/// float result = LinearInterpolate.Lerp(10f, 20f, 0.5f); // Returns 15f
///
/// // With easing
/// Color smoothColor = LinearInterpolate.Lerp(Color.Red, Color.Blue, 0.5f);
///
/// // Point interpolation
/// Point current = LinearInterpolate.Lerp(new Point(0, 0), new Point(100, 100), 0.3f); // Returns (30, 30)
/// </code>
/// </example>
public static partial class LinearInterpolate
{
    /// <summary>
    /// Interpolates between two numeric values using linear interpolation.
    /// </summary>
    /// <typeparam name="T">A numeric type that implements INumber&lt;type&gt;
    /// </typeparam>
    /// <param name="start">The starting value</param>
    /// <param name="end">The ending value</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0</param>
    /// <returns>The interpolated value, clamped between start and end</returns>
    /// <remarks>
    /// This method works with any type that implements System.Numerics.INumber&lt;type&gt;,
    /// including float, double, int, decimal, and custom numeric types.
    /// </remarks>
    /// <example>
    /// <code>
    /// float result = LinearInterpolate.Lerp(10f, 20f, 0.5f); // Returns 15f
    /// int result2 = LinearInterpolate.Lerp(100, 200, 0.3f); // Returns 130
    /// </code>
    /// </example>
    public static T Lerp<T>(T start, T end, float t)
        where T : System.Numerics.INumber<T>
        => T.Clamp(T.CreateChecked(double.CreateChecked(start) + double.CreateChecked(end - start) * t), start, end);

    /// <summary>
    /// Interpolates between two Color values using linear interpolation.
    /// </summary>
    /// <param name="start">The starting color</param>
    /// <param name="end">The ending color</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0</param>
    /// <returns>A new Color with interpolated ARGB components</returns>
    /// <remarks>
    /// Each ARGB component (Alpha, Red, Green, Blue) is interpolated
    /// independently.  The result is rounded to the nearest integer for each
    /// component.
    /// </remarks>
    /// <example>
    /// <code>
    /// Color result = LinearInterpolate.Lerp(Color.Red, Color.Blue, 0.5f); // Returns a purple color
    /// </code>
    /// </example>
    public static Color Lerp(Color start, Color end, float t)
        => Color.FromArgb(
            (int)Sys.Round(start.A + (end.A - start.A) * t),
            (int)Sys.Round(start.R + (end.R - start.R) * t),
            (int)Sys.Round(start.G + (end.G - start.G) * t),
            (int)Sys.Round(start.B + (end.B - start.B) * t)
        );

    /// <summary>
    /// Interpolates between two Point values using linear interpolation.
    /// </summary>
    /// <param name="start">The starting point</param>
    /// <param name="end">The ending point</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0</param>
    /// <returns>A new Point with interpolated X and Y coordinates</returns>
    /// <remarks>
    /// The X and Y coordinates are interpolated independently and rounded to 
    /// the nearest integer.
    /// </remarks>
    /// <example>
    /// <code>
    /// Point result = LinearInterpolate.Lerp(new Point(0, 0), new Point(100, 100), 0.5f); // Returns (50, 50)
    /// </code>
    /// </example>
    public static Point Lerp(Point start, Point end, float t)
        => new(
            (int)Sys.Round(start.X + (end.X - start.X) * t),
            (int)Sys.Round(start.Y + (end.Y - start.Y) * t)
        );

    /// <summary>
    /// Interpolates between two PointF values using linear interpolation.
    /// </summary>
    /// <param name="start">The starting point</param>
    /// <param name="end">The ending point</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0</param>
    /// <returns>A new PointF with interpolated X and Y coordinates</returns>
    /// <remarks>
    /// The X and Y coordinates are interpolated independently.
    /// </remarks>
    /// <example>
    /// <code>
    /// PointF result = LinearInterpolate.Lerp(new PointF(0f, 0f), new PointF(100f, 100f), 0.5f); // Returns (50f, 50f)
    /// </code>
    /// </example>
    public static PointF Lerp(PointF start, PointF end, float t)
        => new(
            start.X + (end.X - start.X) * t,
            start.Y + (end.Y - start.Y) * t
        );

    /// <summary>
    /// Interpolates between two Rectangle values using linear interpolation.
    /// </summary>
    /// <param name="start">The starting rectangle</param>
    /// <param name="end">The ending rectangle</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0</param>
    /// <returns>A new Rectangle with interpolated X, Y, Width, and Height</returns>
    /// <remarks>
    /// The X, Y, Width, and Height are interpolated independently and rounded
    /// to the nearest integer.
    /// </remarks>
    /// <example>
    /// <code>
    /// Rectangle result = LinearInterpolate.Lerp(new Rectangle(0, 0, 100, 50), new Rectangle(50, 25, 200, 100), 0.5f);
    /// </code>
    /// </example>
    public static Rectangle Lerp(Rectangle start, Rectangle end, float t)
        => new (
            (int)Sys.Round(start.X + (end.X - start.X) * t),
            (int)Sys.Round(start.Y + (end.Y - start.Y) * t),
            (int)Sys.Round(start.Width + (end.Width - start.Width) * t),
            (int)Sys.Round(start.Height + (end.Height - start.Height) * t)
        );

    /// <summary>
    /// Interpolates between two RectangleF values using linear interpolation.
    /// </summary>
    /// <param name="start">The starting rectangle</param>
    /// <param name="end">The ending rectangle</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0</param>
    /// <returns>A new RectangleF with interpolated X, Y, Width, and Height</returns>
    /// <remarks>
    /// The X, Y, Width, and Height are interpolated independently.
    /// </remarks>
    /// <example>
    /// <code>
    /// RectangleF result = LinearInterpolate.Lerp(new RectangleF(0f, 0f, 100f, 50f), new RectangleF(50f, 25f, 200f, 100f), 0.5f);
    /// </code>
    /// </example>
    public static RectangleF Lerp(RectangleF start, RectangleF end, float t)
        => new(
            start.X + (end.X - start.X) * t,
            start.Y + (end.Y - start.Y) * t,
            start.Width + (end.Width - start.Width) * t,
            start.Height + (end.Height - start.Height) * t
        );

    /// <summary>
    /// Linearly interpolates between two arrays of values (element-wise).
    /// </summary>
    /// <typeparam name="T">A numeric type that implements INumber&lt;T&gt;.
    /// </typeparam>
    /// <param name="start">The starting array.</param>
    /// <param name="end">The ending array.</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0.</param>
    /// <returns>
    /// A new array with each element interpolated between the corresponding
    /// start and end values.
    /// </returns>
    /// <remarks>
    /// The result array is the same length as the shortest of the input arrays.
    /// </remarks>
    public static T[] Lerp<T>(T[] start, T[] end, float t)
        where T : System.Numerics.INumber<T>
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);
        int len = System.Math.Min(start.Length, end.Length);
        var result = new T[len];
        for (int i = 0; i < len; i++)
            result[i] = Lerp(start[i], end[i], t);
        return result;
    }

    /// <summary>
    /// Linearly interpolates between two arrays of values (element-wise).
    /// </summary>
    /// <typeparam name="T">A numeric type that implements INumber&lt;T&gt; and
    /// IEquatable&lt;T&gt;.</typeparam>
    /// <param name="start">The starting array.</param>
    /// <param name="end">The ending array.</param>
    /// <param name="t">Interpolation factor between 0.0 and 1.0.</param>
    /// <param name="result">The result array.</param>
    /// <remarks>
    /// The result array is the same length as the input arrays.
    /// </remarks>
    public static void Lerp<T>(T[] start, T[] end, float t, T[] result)
        where T : System.Numerics.INumber<T>, IEquatable<T>
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(end);
        ArgumentNullException.ThrowIfNull(result);
        if (start.Length != end.Length)
            throw new ArgumentException("Start and end arrays must be the same length");
        if (start.Length != result.Length)
            throw new ArgumentException("Result array must be the same length as start and end arrays");
        int len = start.Length;
        for (int i = 0; i < len; i++)
            result[i] = start[i].Equals(end[i]) ? start[i] : Lerp(start[i], end[i], t);
    }

}
