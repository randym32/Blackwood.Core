// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Blackwood;

/// <summary>
/// Extension methods for the Math class.
/// </summary>
public static class CoreExtensions
{
    /// <summary>
    /// Union two rectangles.
    /// </summary>
    /// <param name="rect">The first rectangle.</param>
    /// <param name="other">The second rectangle.</param>
    /// <returns>The union of the two rectangles.</returns>
    static public RectangleF UnionWith(this RectangleF rect, RectangleF other)
    {
        var left = Math.Min(rect.Left, other.Left);
        var right = Math.Min(rect.Top, other.Top);
        return new RectangleF(
          left,
          right,
          Math.Max(rect.Right, other.Right) - left,
          Math.Max(rect.Bottom, other.Bottom)-right
       );
    }
}