// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Drawing;
namespace Blackwood;

/// <summary>
/// Represents an annotation that can be added to a chart for plotting.
/// </summary>
/// <typeparam name="IndexType">The type of the index value.</typeparam>
/// <typeparam name="DurationType">The type of the duration/length value.</typeparam>
/// <remarks>
/// Like a note on the margin of a whiteboard, this class exists
/// to convince your chart that life is more than a soulless parade of data
/// points -— sometimes, even pixels deserve an existential flourish.
///
/// An annotation is a span of time with a color and optional text.
/// </remarks>
/// <remarks>
/// Creates a new annotation.
/// </remarks>
/// <param name="index">Independent variable value indicating where the annotation is positioned.</param>
/// <param name="duration">Duration or length of the annotation span.</param>
/// <param name="color">Color used for the span annotation.</param>
/// <param name="text">Optional text displayed with the annotation.</param>
/// <param name="textColor">Optional color for the annotation text. If not specified, defaults to white.</param>
public class ChartAnnotation<IndexType, DurationType>(IndexType index, DurationType duration, Color color, string? text = null, Color? textColor = null)
{
    /// <summary>
    /// Independent variable value indicating where the annotation is positioned.
    /// </summary>
    public readonly IndexType index = index;

    /// <summary>
    /// Duration or length of the annotation span.
    /// </summary>
    public readonly DurationType duration = duration;

    /// <summary>
    /// Color used for the span annotation.
    /// </summary>
    public readonly Color color = color;

    /// <summary>
    /// Color used for the annotation text. Defaults to white if not specified.
    /// </summary>
    public readonly Color textColor = textColor ?? Color.White;


    /// <summary>
    /// Optional text displayed with the annotation.
    /// </summary>
    public readonly string? text = text;
}

/// <summary>
/// Simpler annotation where the index and duration share the same type parameter.
/// </summary>
/// <typeparam name="IndexType">The type of both the index and the duration.</typeparam>
/// <remarks>
/// Creates a new annotation where the index and duration share the same type.
/// </remarks>
public class ChartAnnotation<IndexType>(IndexType index, IndexType duration, Color color, string? text = null, Color? textColor = null) : ChartAnnotation<IndexType, IndexType>(index, duration, color, text, textColor)
{
}

