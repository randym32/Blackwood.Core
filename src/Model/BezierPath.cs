// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Drawing;

namespace Blackwood;

/// <summary>
/// Represents a Bezier path consisting of one or more Bezier segments.
/// </summary>
public class BezierPath
{
    /// <summary>
    /// The points that make up each of the Bezier segments.
    /// This array defines the path geometry;
    /// every group of 4 points describes a cubic Bezier segment
    /// (start point, control point 1, control point 2, end point).
    /// </summary>
    public PointF[] PathPoints = [];

    /// <summary>
    /// Gets a value indicating whether the path is closed.
    /// True if the path is closed (forms a loop), otherwise false.
    /// </summary>
    public bool Closed = false;
}

