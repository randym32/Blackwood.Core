// Copyright (c) 2022-2026 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Globalization;

namespace Blackwood;


/// <summary>
/// Adds an equality that ignores name
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Compares two Color objects based on their RGBA values.
    /// </summary>
    /// <param name="color1">The first color to compare.</param>
    /// <param name="color2">The second color to compare.</param>
    /// <returns>True if the colors are equal based on their RGBA values, otherwise false.</returns>
    public static bool EqualsRGBA(this Color color1, Color color2)
    {
        // Compare the RGBA values
        return color1.R == color2.R &&
               color1.G == color2.G &&
               color1.B == color2.B &&
               color1.A == color2.A;
    }
}


static public partial class Match
{
    /// <summary>
    /// Regular expression used to parse RGBA colors, as well as named colors
    /// </summary>
    /// <remarks>
    /// The rgb/rgba alternative uses <c>[^)]+</c> so decimals, percents, and
    /// spaces inside the parentheses are part of one token. A trailing
    /// <c>(?![0-9a-zA-Z_])</c> rejects partial prefixes like <c>rgb(255</c>
    /// when immediately followed by more identifier characters.
    /// </remarks>
    private const string ColorPattern = @"(rgba?\s*\([^)]+\))(?![0-9a-zA-Z_])|(#[\da-fA-F]+)(?![0-9a-zA-Z_])|([-\w]+)";

    /// <summary>
    /// Parse a color name/specification from the string.
    /// </summary>
    /// <param name="text">The text string.</param>
    /// <returns>null if the color did not match.</returns>
    internal static Color? ColorHelp1(string text)
    {
        try
        {
            // Try to convert the name to a color
            var result = Color.FromName(text);
            if (!result.IsEmpty && result.IsKnownColor)
                return result;
        }
        catch (Exception)
        { }

        return null;
    }


    /// <summary>
    /// Parse a color RGBA specification from the string.
    /// </summary>
    /// <param name="text">The text string.</param>
    /// <returns>null if the color did not match.</returns>
    internal static Color? ColorHelp2(string text)
    {
        try
        {
            // Try to convert the name/spec to a color
            var result = ColorTranslator.FromHtml(text);
            if (!result.IsEmpty)
                return result;
        }
        catch (Exception)
        { }

        return null;
    }


    /// <summary>
    /// Parse a color from the passed string.
    /// </summary>
    /// <param name="text">The text to parse</param>
    /// <param name="ofs">The index in the string to start parsing at.</param>
    /// <returns>null on error, otherwise the parsed color</returns>
    /// <remarks>
    /// This recognizes the following color formats:
    /// 1. CSS-style rgb() and rgba() functions, e.g. "rgb(255,0,0)", "rgba(255, 0, 0, 0.5)"
    /// 2. Hex color strings starting with "#", such as "#fff", "#ff0033", "#123abc"
    /// 3. Named colors consisting of alphanumeric, dash, or underscore characters, like "red", "dodger-blue", "color_foo"
    /// </remarks>
    public static ParseSuccess? ParseColor(string text, int ofs)
    {
        // CSS functions are handled first. ColorTranslator.FromHtml throws on
        // rgb()/rgba(), and a regex match on the bare prefix "rgb" would bypass
        // ParseBase token-boundary checks when digits follow the closing paren.
        if (TryParseCSSRGBString(text, ofs, out var cssColor, out var cssEnd)
            && IsColorTokenBoundary(text, cssEnd))
            return new ParseSuccess(Color_id, cssColor, cssEnd);

        // Get the string for the possible color
        var c = ParseBase(ColorPattern, text, ofs);
        if (null == c)
        {
            // Didn't find anything that matches
            return null;
        }

        // Get the matching substring for the internal parsing
        var param = c.Value;

        // Try to convert the RGB to a color
        var result = ColorHelp1(param);
        if (null != result)
            // Return the success, with the lexing position moved past this pattern
            return new ParseSuccess(Color_id, result, ofs + c.Length);

        // FromHtml handles #hex; it throws on rgb()/rgba() so skip those here.
        if (!param.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            result = ColorHelp2(param);
            if (null != result)
                // Return the success, with the lexing position moved past this pattern
                return new ParseSuccess(Color_id, result, ofs + c.Length);
        }

        // Try looking in internal table
        if (RGBColors.RGBColor(param, out var color))
            // Return the success, with the lexing position moved past this pattern
            return new ParseSuccess(Color_id, color, ofs + c.Length);

        return null;
    }

    /// <summary>
    /// True when parsing may continue past <paramref name="index"/> or the next
    /// character is whitespace or an operator (same rule as <see cref="ParseBase"/>).
    /// </summary>
    private static bool IsColorTokenBoundary(string text, int index)
        => index >= text.Length || WhitespaceAndOperators.Contains(text[index]);

    /// <summary>
    /// Parses <c>rgb(r,g,b)</c> or <c>rgba(r,g,b,a)</c> without delegating to
    /// <c>ColorTranslator.FromHtml</c>.
    /// </summary>
    /// <param name="text">Full input string.</param>
    /// <param name="ofs">Index at which to start, allowing leading whitespace.</param>
    /// <param name="color">Parsed ARGB on success.</param>
    /// <param name="nextIndex">Index of the first character after the closing parenthesis.</param>
    /// <returns><see langword="true"/> when the string matches the expected comma-separated form.</returns>
    private static bool TryParseCSSRGBString(string text, int ofs, out Color color, out int nextIndex)
    {
        color = default;
        nextIndex = ofs;

        while (ofs < text.Length && char.IsWhiteSpace(text[ofs]))
            ofs++;

        nextIndex = ofs;
        var hasAlpha = text.AsSpan(ofs).StartsWith("rgba(", StringComparison.OrdinalIgnoreCase);
        if (!hasAlpha && !text.AsSpan(ofs).StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
            return false;

        // Split the parenthesized argument list — no spaces required between commas.
        var open = text.IndexOf('(', ofs);
        var close = text.IndexOf(')', open + 1);
        if (open < 0 || close <= open)
            return false;

        var parts = text[(open + 1)..close].Split(',');
        var needed = hasAlpha ? 4 : 3;
        if (parts.Length != needed)
            return false;

        if (   !TryParseColorByte(parts[0], 0, out var r)
            || !TryParseColorByte(parts[1], 0, out var g)
            || !TryParseColorByte(parts[2], 0, out var b))
            return false;

        var a = 255;
        if (hasAlpha && !TryParseColorByte(parts[3], 255, out a))
            return false;

        color = Color.FromArgb(a, r, g, b);
        nextIndex = close + 1;
        return true;
    }

    /// <summary>Parses one RGB channel as a byte (0–255), percent (0–100%), or unit float (0–1).</summary>
    /// <param name="part">Single comma-separated token from a CSS color function.</param>
    /// <param name="def">Default returned on failure.</param>
    /// <param name="channel">Byte value 0–255 on success.</param>
    /// <returns><see langword="true"/> when <paramref name="part"/> is a valid channel literal.</returns>
    private static bool TryParseColorByte(string part, int def,out int channel)
    {
        part = part.Trim();
        // Integer bytes are the primary serialize format (e.g. rgb(255,0,0)).
        if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var byteValue)
            && byteValue is >= 0 and <= 255)
        {
            channel = byteValue;
            return true;
        }

        // CSS percentages (e.g. rgb(100%, 0%, 0%)).
        if (part.EndsWith('%'))
        {
            var pctText = part[..^1].Trim();
            if (float.TryParse(pctText, NumberStyles.Float, CultureInfo.InvariantCulture, out var pct)
                && pct is >= 0f and <= 100f)
            {
                channel = (int)MathF.Round(pct / 100f * 255f);
                return true;
            }
        }

        // Unit floats support legacy linear object reads echoed as css (e.g. rgb(1,0,0.5)).
        if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var unit)
            && unit is >= 0f and <= 1f)
        {
            channel = (int)MathF.Round(unit * 255f);
            return true;
        }

        channel = def;
        return false;
    }
}
