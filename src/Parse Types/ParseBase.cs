// Copyright (c) 2024-2026 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Blackwood;


static public partial class Match
{
    #region Id's for the builtin patterns
    /// <summary>
    /// The match id of the color type.
    /// </summary>
    static readonly uint Color_id = Match.FNV1a("color".ToUpper());
    #endregion

    /// <summary>
    /// A character set containing whitespace and common operators
    /// </summary>
    public static readonly HashSet<char> WhitespaceAndOperators =
    [
        ' ',  // Space
        '\t', // Tab
        '\n', // Newline
        '\r', // Carriage return
        '+',  // Addition
        '-',  // Subtraction
        '*',  // Multiplication
        '/',  // Division
        '%',  // Modulo
        '(',  // Left parenthesis
        ')',  // Right parenthesis
        '=',  // Assignment/Equality
        '<',  // Less than
        '>',  // Greater than
        '&',  // Bitwise AND
        '|',  // Bitwise OR
        '^',  // Bitwise XOR
        '!',  // Logical NOT
        '~',  // Bitwise NOT
        ',',  // Comma
        ':',  // Colon
        ';'   // Semicolon
    ];


    #region Parse using builtin .Net helpers
    /// <summary>
    /// Matches the section of string against the template.
    /// </summary>
    /// <param name="Pattern">The regex pattern to match against.  Note: the regular expression rules are sensitive to the first alternative that matches, even though a later one may match better.</param>
    /// <param name="text">The text to parse.</param>
    /// <param name="ofs">The index in the string to start parsing at.</param>
    /// <returns>null on error, otherwise the matching region</returns>
    internal static System.Text.RegularExpressions.Match? ParseBase(string Pattern, string text, int ofs)
    {
        // Get the string starting at the given offset
        var matchStr = 0==ofs?text:text.Substring(ofs);

        // Match the string against the pattern
        var c = Regex.Matches(matchStr, Pattern, RegexOptions.IgnoreCase);
        if (c.Count < 1)
        {
            // Didn't find anything that matches
            return null;
        }

        // Must not end in the middle of a token
        if (matchStr.Length > c[0].Length &&
            !WhitespaceAndOperators.Contains(matchStr[c[0].Length]))
        {
            return null;
        }

        // return the match
        return c[0];
    }

    #endregion

}
