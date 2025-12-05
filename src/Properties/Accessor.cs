// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Blackwood;

public static partial class AssemblyUtils
{
    /// <summary>
    /// Parse a path string that starts with a class name.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <param name="memberReference">The member reference.</param>
    /// <returns>The type of the class referred to.</returns>
    static public Type? ParseClassMemberString(string str, out string? memberReference)
    {
        // This is used to find the longest match for the class name
        int longestMatch = 0;
        Type? bestMatch = null;
        memberReference = null;

        // Try to find the type by its name in all loaded assemblies.
        // This will look for the longest match, and then the shortest match.
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Get all the types in the assembly
            foreach (var t in asm.GetTypes())
            {
                // See if the path starts with the full name of the type
                if (  t.FullName?.Length > longestMatch
                   && str.Length > t.FullName.Length
                   && str[t.FullName.Length] == '.'
                   && str.StartsWith(t.FullName)
                   )
                {
                    longestMatch = t.FullName.Length;
                    bestMatch = t;
                }
                // See if the path starts with the name of the type
                else if (  t.Name.Length > longestMatch
                        && str.Length > t.Name.Length
                        && str[t.Name.Length] == '.'
                        && str.StartsWith(t.Name)
                        )
                {
                    longestMatch = t.Name.Length;
                    bestMatch = t;
                }
            }
        }

        // If a best match was found, return the path after the match
        if (bestMatch != null)
            memberReference = str.Substring(longestMatch + 1);

        // Return the best match
        return bestMatch;
    }


    /// <summary>
    /// Walks the given object and path, returning a getter for the last member and a setter.
    /// Returns null if traversal fails and sets 'set' to null.
    /// </summary>
    /// <param name="firstTarget">The first object to start with.</param>
    /// <param name="path">The path to the property or field.</param>
    /// <param name="set">The delegate for setting the value of the member; null if the member is not found.</param>
    /// <returns>A delegate that gets the value of the specified member of the target object.</returns>
    public static Func<object?>? GetAccesorForPath(object firstTarget, string[] path, out Action<object>? set)
    {
        object? target = firstTarget;
        set = null;
        Func<object?>? get = null;

        // Handle null or empty path
        if (path == null || path.Length == 0)
            return null;

        // This will hold the set delegate for the intermediate members.
        // That last one will be returned.
        Action<object>? intermediateSet = null;

        // Walk through all path elements except the last, resolving each successive target.
        for (int index = 0; index < path.Length; index++)
        {
            // If the target is null, fail. 
            // This only fail if this is not the last element in the path
            // For the last element, we want to allow null values (e.g., nullable properties)
            if (target == null)
                return null;

            // Get the accessor for the member
            get = GetMemberAccessor(target, path[index].Trim(), out intermediateSet);
            if (get == null)
            {
                return null;
            }

            // Set the target of the next iteration to the value of the member
            target = get();
        }

        // Return the accessor for the last member
        set = intermediateSet;
        return get;
    }


    /// <summary>
    /// Retrieves the value of the given property or field from the specified 
    /// object instance.
    /// </summary>
    /// <param name="target">The object to inspect for the member</param>
    /// <param name="name">The name of the property or field to get.</param>
    /// <param name="set">The delegate for setting the value of the member;
    /// null if the member is not found.</param>
    /// <returns>
    /// Returns a delegate that gets the value of the specified member of the 
    /// target object.
    /// Returns null if the member is not found.
    /// </returns>
    internal static Func<object?>? GetMemberAccessor(object target, string name, out Action<object>? set)
    {
        // Handle null target
        if (target == null || string.IsNullOrEmpty(name))
        {
            set = null;
            return null;
        }

        // Look up the accessor
        return GetMemberAccessor(target.GetType(), target, name, out set);
    }

    /// <summary>
    /// Retrieves the value of the given property or field from the specified object instance.
    /// </summary>
    /// <param name="type">The type of the object to inspect for the member</param>
    /// <param name="target">The object to inspect for the member; null if working with a class</param>
    /// <param name="name">The name of the property or field to get.</param>
    /// <param name="set">The delegate for setting the value of the member; null if the member is not found, or is not modifiable.</param>
    /// <returns>
    /// Returns a delegate that gets the value of the specified member of the target object.
    /// Returns null if the member is not found, or cant be read.
    /// </returns>
    internal static Func<object?>? GetMemberAccessor(Type type, object target, string name, out Action<object>? set)
    {
        set = null;
        // Handle null target
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        // This is used to distinguish between class methods and instance methods
        var classOrInstance = target == null ? BindingFlags.Static :BindingFlags.Instance;

        // First scan for a property with the given name
        // There is a standard .NET call to look up a property by name:
        // This returns a PropertyInfo instance if found, or null otherwise.
        var propInfo = type.GetProperty(name, classOrInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        if (propInfo != null)
        {
            // Check if the property has a getter and setter
            var setter = propInfo.GetSetMethod();

            // Check if property is not fully accessible (ie, not missing public getter or setter)
            if (setter != null)
            {
                // Property is fully accessible
                set = (value) => propInfo.SetValue(target, value);
            }

            var getter = propInfo.GetGetMethod();
            if (getter == null)
                return null;
            return () => propInfo.GetValue(target);
        }

        // Fall back to a field with that name
        var fieldInfo = type.GetField(name, classOrInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        if (fieldInfo != null)
        {
            // Found the field, return the value
            // Check that it is modifiable
            if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
            {
                set = (value) => fieldInfo.SetValue(target, value);
            }
            return () => fieldInfo.GetValue(target);
        }

        // As a last attempt, do a case-insensitive and prefix search for a
        // method named "Get"+name (e.g., "GetFoo" for "Foo") and expose as a getter
        // if found, return the method as a getter
        var getMethod = type.GetMethods(classOrInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                            .FirstOrDefault(  m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                                           && m.GetParameters().Length == 0);
        if (getMethod == null)
        {
            getMethod = type.GetMethods(classOrInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .FirstOrDefault(m => m.Name.Equals("Get" + name, StringComparison.OrdinalIgnoreCase)
                                    && m.GetParameters().Length == 0);
        }

        if (getMethod != null)
        {
            return () => getMethod.Invoke(target, null);
        }

        // Couldnt find anything
        return null;
    }
}
