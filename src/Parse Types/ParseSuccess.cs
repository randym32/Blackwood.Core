// Copyright (c) 2024-2026 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;

/// <summary>
/// The ParseSuccess class encapsulates the result of a successful parsing
/// operation.  It contains the identifier of the matched rule, the value
/// obtained from parsing, and the index of the next character to parse in the
/// input string.
/// 
/// This class implements the IComparable interface to allow comparison between
/// different instances of ParseSuccess based on their nextIndex, matchId, and
/// value.
/// </summary>
public class ParseSuccess: IComparable<ParseSuccess>
{
    /// <summary>
    /// The identifier of the rule that matched.
    /// </summary>
    internal readonly uint matchId;

    /// <summary>
    /// The value as a result of parsing the pattern.
    /// </summary>
    public readonly object value;

    //todo: include score -- metaphone matches score lower

    /// <summary>
    /// The next index of the string to parse. 
    /// This is used when this parse is a sub-parse.
    /// </summary>
    public readonly int nextIndex;

    /// <summary>
    /// Creates a structure to hold the state of parsing success.
    /// </summary>
    /// <param name="matchId">The id of the pattern that successfully matched.</param>
    /// <param name="value">The value for that match.</param>
    /// <param name="nextIndex">The index of the next character in the string to parse.</param>
    internal ParseSuccess(uint matchId, object value, int nextIndex)
    {
        this.matchId   = matchId;
        this.value     = value;
        this.nextIndex = nextIndex;
    }

    /// <summary>
    /// Compare this instance with another instance of parse success, to see if
    /// they are the same.
    /// </summary>
    /// <param name="other">The other item to compare against.</param>
    /// <returns>Less than zero if this item is less than the other;
    /// zero if both items are equal;
    /// greater than zero if this item is greater then the other item.</returns>
    /// <remarks>The score is not compared.</remarks>
    public int CompareTo(ParseSuccess? other)
    {
        if (other == null) return 1;
        if (nextIndex != other.nextIndex)
            return nextIndex - other.nextIndex;
        if (matchId != other.matchId)
            return matchId < other.matchId?-1:1;
        return value==other.value?0:value==null?1:other.value==null?-1:0;
    }

    /// <summary>
    /// Compares this object with the passed one for equality
    /// </summary>
    /// <param name="obj">The object to compare against</param>
    /// <returns>true if they are the same, false otherwise</returns>
    public override bool Equals(object? obj)
    {
        if (null != obj && obj is ParseSuccess p)
        {
            if (matchId == p.matchId && nextIndex == p.nextIndex)
                // Check the lists for equality
                if (value is List<object> a && p.value is List<object> b)
                    return Enumerable.SequenceEqual(a, b);
                else
                    // Not lists, so use regular equality test
                    return value.Equals(p.value);
            return false;
        }
        return base.Equals(obj);
    }

    /// <summary>
    /// The hash of the the match
    /// </summary>
    /// <returns>The hash value</returns>
    public override int GetHashCode()
    {
        // 397 is used "because it typically overflows the range of the int and
        // thus mixes bits a bit better."
        var hash = matchId.GetHashCode()* 397 + nextIndex.GetHashCode();

        // Conditionally hash the value object too
        if (null != value)
            hash = hash * 397 + value.GetHashCode();

        // Return the resulting hash
        return hash;
    }
}

