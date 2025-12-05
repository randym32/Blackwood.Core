// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;
namespace Blackwood;

/// <summary>
/// A helper class of miscellaneous things
/// </summary>
public static partial class Utils
{
    /// <summary>
    /// A regex to split a C/C++/C# name at word boundaries.
    /// </summary>
    [GeneratedRegex(@"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|_")]
    private static partial Regex CNameSplitRegex();

    /// <summary>
    /// Convert a C# name to a label.
    /// </summary>
    /// <param name="cname">The C# name to convert.</param>
    /// <returns>The converted label.</returns>
    /// <remarks>
    /// This splits the name at word boundaries, capitalizing the first
    /// letter of each word.
    /// It also removes the first word if it is "g" or "m" or other Hungarian
    /// notation prefix.
    /// It also removes "node" or "class" from the end.
    /// </remarks>
    public static string ConvertCNameToLabel(string cname)
    {
        // Use Regex to split where a lowercase letter is followed by an uppercase letter or on underscores
        // For instance HTMLParser should be split into HTML and Parser.
        string[] result = CNameSplitRegex().Split(cname);
        result = result.Select(x => x.ToLowerInvariant()).ToArray();

        // Remove the first word if it is "g" or "m" or other Hungarian notation
        // prefix.
        if (result.Length > 0 && (result[0] == "g" || result[0] == "m"))
            result = result.Skip(1).ToArray();

        // Remove "node" or "class" from the end
        if (result.Length > 0)
        {
            var last = result[result.Length - 1];
            if (last == "node" || last == "class")
                result = result.Take(result.Length - 1).ToArray();
        }

        // Join the parts with a space
        var str = string.Join(" ", result);

        // capitalize the first letter
        if (!string.IsNullOrEmpty(str))
            str = char.ToUpperInvariant(str[0]) + str.Substring(1);
        return str;
    }
}
