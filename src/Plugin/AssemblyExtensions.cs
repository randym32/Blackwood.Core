// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Blackwood;

/// <summary>
/// Provides helper extensions to the assembly class
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Finds all classes in the assembly that are subclasses of the specified
    /// type.
    /// </summary>
    /// <param name="asm">The assembly to search.</param>
    /// <param name="type">The base type or interface to match.</param>
    /// <returns>An enumerable of types in the assembly that match the specified
    /// type.</returns>
    public static IEnumerable<Type> FindClasses(this Assembly asm, Type type)
    {
        return asm.GetTypes().Where(t => t != null && !t.IsAbstract
                                   && t.IsSubclassOf(type));
    }

}