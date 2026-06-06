// Copyright (c) 2020-2022 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.  

using System.Text;
namespace Blackwood;

static public partial class Match
{
    /// <summary>
    /// Compute a 32-bit FNV (Fowler–Noll–Vo) hash from the passed string.
    /// </summary>
    /// <param name="str">The string (it is recommended to make this all uppercase first)</param>
    /// <returns>The 32-bit ID for the string</returns>
    /// <remarks>
    /// The FNV-1a hash algorithm is a non-cryptographic hash function created
    /// by Glenn Fowler, Landon Curt Noll, and Phong Vo.It is designed to be
    /// fast and simple while providing a good distribution of hash values for
    /// different inputs.
    /// 
    /// The function takes a string and an optional initial hash value as input
    /// and returns a 32-bit hash value.
    /// 
    /// See https://en.m.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function for more details on this hash function.
    /// </remarks>
    public static uint FNV1a(string str)
    {
        // Convert to the bytes
        byte[] bytes = Encoding.ASCII.GetBytes(str);

        // The extra seed is from the Yoshimitsu TRIAD
        // Which supposedly works better for smaller strings
        //0xD8AFFD71 ^ 2166136261;
        uint hash = 2166136261;

        // Apply each of the characters
        foreach (byte ch in bytes)
        {
            // Xor in the character
            hash ^= ch;

            // Multiply it by the prime
            hash *= 16777619;
        }

        // Return the result
        return hash;
    }
}

