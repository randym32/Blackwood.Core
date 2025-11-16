// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Drawing;
namespace Blackwood;

/// <summary>
/// Represents an annotation that can be added to a chart for plotting.
/// </summary>
/// <typeparam name="T">The type of the index and duration values.</typeparam>
/// <remarks>
/// Like a note on the margin of a whiteboard, this class exists
/// to convince your chart that life is more than a soulless parade of data
/// points -— sometimes, even pixels deserve an existential flourish.
///
/// An annotation is a span of time with a color and optional text.
/// </remarks>
public class ChartAnnotation<T>
{
    /// <summary>
    /// Independent variable value indicating where the annotation is positioned.
    /// </summary>
    public readonly T index;

    /// <summary>
    /// Duration or length of the annotation span.
    /// </summary>
    public readonly T duration;

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
    /// Initializes a new instance of the ChartAnnotation class.
    /// </summary>
    /// <param name="index">Independent variable value indicating where the annotation is positioned.</param>
    /// <param name="duration">Duration or length of the annotation span.</param>
    /// <param name="color">Color used for the span annotation.</param>
    /// <param name="text">Optional text displayed with the annotation.</param>
    /// <param name="textColor">Optional color for the annotation text. If not specified, defaults to white.</param>
    public ChartAnnotation(T index, T duration, Color color, string? text = null, Color? textColor = null)
    {
        this.index = index;
        this.duration = duration;
        this.color = color;
        this.text = text;
        if (textColor != null)
        {
            this.textColor = (Color)textColor;
        }
    }
}

