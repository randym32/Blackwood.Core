// Copyright (c) 2021 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

using System.Text;
namespace Blackwood;

static public partial class Match
{
    /// <summary>
    /// Compute a 64-bit FNV (Fowler–Noll–Vo) hash from the passed string.
    /// </summary>
    /// <param name="str">The string (it is recommended to make this all uppercase first)</param>
    /// <returns>The 64-bit ID for the string</returns>
    /// <remarks>See https://en.m.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function for more details on this hash function.</remarks>
    public static ulong FNV1a_64(string str)
    {
        // Convert to the bytes
        byte[] bytes = Encoding.ASCII.GetBytes(str);

        ulong hash = 14695981039346656037UL; // FNV offset basis

        // Apply each of the characters
        foreach (byte ch in bytes)
        {
            // Xor in the charcter
            hash ^= ch;

            // Multiply by the FNV prime
            hash *= 1099511628211UL;
        }

        // Return the result
        return hash;
    }
}
