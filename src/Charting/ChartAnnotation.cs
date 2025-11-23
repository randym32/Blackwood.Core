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
public class ChartAnnotation<IndexType, DurationType>
{
    /// <summary>
    /// Independent variable value indicating where the annotation is positioned.
    /// </summary>
    public readonly IndexType index;

    /// <summary>
    /// Duration or length of the annotation span.
    /// </summary>
    public readonly DurationType duration;

    /// <summary>
    /// Color used for the span annotation.
    /// </summary>
    public readonly Color color;

    /// <summary>
    /// Color used for the annotation text. Defaults to white if not specified.
    /// </summary>
    public readonly Color textColor = Color.White;


    /// <summary>
    /// Optional text displayed with the annotation.
    /// </summary>
    public readonly string? text;

    /// <summary>
    /// Creates a new annotation.
    /// </summary>
    /// <param name="index">Independent variable value indicating where the annotation is positioned.</param>
    /// <param name="duration">Duration or length of the annotation span.</param>
    /// <param name="color">Color used for the span annotation.</param>
    /// <param name="text">Optional text displayed with the annotation.</param>
    /// <param name="textColor">Optional color for the annotation text. If not specified, defaults to white.</param>
    public ChartAnnotation(IndexType index, DurationType duration, Color color, string? text = null, Color? textColor = null)
    {
        this.index     = index;
        this.duration  = duration;
        this.color     = color;
        this.text      = text;
        this.textColor = textColor ?? Color.White;
    }
}

/// <summary>
/// Simpler annotation where the index and duration share the same type parameter.
/// </summary>
/// <typeparam name="IndexType">The type of both the index and the duration.</typeparam>
public class ChartAnnotation<IndexType> : ChartAnnotation<IndexType, IndexType>
{
    /// <summary>
    /// Creates a new annotation where the index and duration share the same type.
    /// </summary>
    public ChartAnnotation(IndexType index, IndexType duration, Color color, string? text = null, Color? textColor = null)
        : base(index, duration, color, text, textColor)
    {
    }
}

