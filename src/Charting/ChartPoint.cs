// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;

/// <summary>
/// A point that can be added to a chart for plotting, like an existential
/// pigeon strutting along the axis of your chart.
/// </summary>
/// <param name="index">The independent variable.</param>
/// <param name="y">The dependent variable.</param>
/// <param name="annotation">The annotation for the point.  Optional.</param>
public class ChartPoint<T>(T index, double y, string? annotation = null)
{
    /// <summary>
    /// The independent variable.
    /// </summary>
    public readonly T index = index;

    /// <summary>
    /// The dependent variable.
    /// </summary>
    public readonly double y = y;

    /// <summary>
    /// The annotation for the point.  Optional.
    /// </summary>
    public readonly string? annotation = annotation;
}
