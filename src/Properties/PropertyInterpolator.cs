// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

/*
   In a fancy, ideal world, there would be automatic code generation to access,
   rather than a bunch of intermediate delegates, which might make some feel this
   is slow.
*/

using System.Drawing;
using System.Reflection;

namespace Blackwood;

/// <summary>
/// Provides functionality to identify interpolatable properties in a class and
/// create interpolation functions.
/// </summary>
/// <remarks>
/// This class analyzes a given type to find properties that can be interpolated
/// using LinearInterpolate.Lerp, and generates interpolation functions that can
/// animate between two instances of the class.
/// </remarks>
public static class PropertyInterpolator
{
    #region Delegates
    /// <summary>
    /// A delegate that represents an interpolation function.
    /// </summary>
    /// <param name="start">The start value</param>
    /// <param name="end">The end value</param>
    /// <param name="t">The time factor between 0.0 and 1.0</param>
    /// <returns>The interpolated value</returns>
    public delegate object dInterpolationFunction(object start, object end, float t);

    /// <summary>
    /// A delegate that represents an interpolation function that interpolates
    /// between the start and finish, into the given destination object.
    /// </summary>
    /// <param name="start">The start value</param>
    /// <param name="end">The end value</param>
    /// <param name="t">The time factor between 0.0 and 1.0</param>
    /// <param name="dest">The destination object, which will be modified.</param>
    public delegate void dInterpolate(object start, object end, float t, object dest);
    #endregion

    #region Cache of Interpolation Functions

    /// <summary>
    /// A dictionary of interpolation functions for various types.
    /// </summary>
    internal static Dictionary<System.Type, dInterpolationFunction> InterpolationFunctions = new()
    {
        {typeof(Color    ), static (obj,other,t) => LinearInterpolate.Lerp((Color)obj, (Color)other, t)},
        {typeof(float    ), static (obj,other,t) => LinearInterpolate.Lerp((float)obj, (float)other, t)},
        {typeof(double   ), static (obj,other,t) => LinearInterpolate.Lerp((double)obj, (double)other, t)},
        {typeof(int      ), static (obj,other,t) => LinearInterpolate.Lerp((int)obj, (int)other, t) },
        {typeof(decimal  ), static (obj,other,t) => LinearInterpolate.Lerp((decimal)obj, (decimal)other, t)},
        {typeof(byte     ), static (obj,other,t) => LinearInterpolate.Lerp((byte)obj, (byte)other, t)},
        {typeof(short    ), static (obj,other,t) => LinearInterpolate.Lerp((short)obj, (short)other, t)},
        {typeof(long     ), static (obj,other,t) => LinearInterpolate.Lerp((long)obj, (long)other, t) },
        {typeof(sbyte    ), static (obj,other,t) => LinearInterpolate.Lerp((sbyte)obj, (sbyte)other, t)},
        {typeof(ushort   ), static (obj,other,t) => LinearInterpolate.Lerp((ushort)obj, (ushort)other, t)},
        {typeof(uint     ), static (obj,other,t) => LinearInterpolate.Lerp((uint)obj, (uint)other, t)},
        {typeof(ulong    ), static (obj,other,t) => LinearInterpolate.Lerp((ulong)obj, (ulong)other, t)},
    };

    /// <summary>
    /// A dictionary of interpolation functions for various types that interpolate into a destination object.
    /// </summary>
    internal static Dictionary<System.Type, dInterpolate> InterpolateFunctions = new()
    {
        { typeof(float[]   ), static (obj, other, t, dest) => LinearInterpolate.Lerp((float[])obj, (float[])other, t, (float[])dest) },
        { typeof(double[]  ), static (obj, other, t, dest) => LinearInterpolate.Lerp((double[])obj, (double[])other, t, (double[])dest) },
        { typeof(int[]     ), static (obj, other, t, dest) => LinearInterpolate.Lerp((int[])obj, (int[])other, t, (int[])dest) },
        { typeof(decimal[] ), static (obj, other, t, dest) => LinearInterpolate.Lerp((decimal[])obj, (decimal[])other, t, (decimal[])dest) },
        { typeof(byte[]    ), static (obj, other, t, dest) => LinearInterpolate.Lerp((byte[])obj, (byte[])other, t, (byte[])dest) },
        { typeof(short[]   ), static (obj, other, t, dest) => LinearInterpolate.Lerp((short[])obj, (short[])other, t, (short[])dest) },
        { typeof(long[]    ), static (obj, other, t, dest) => LinearInterpolate.Lerp((long[])obj, (long[])other, t, (long[])dest) },
        { typeof(sbyte[]   ), static (obj, other, t, dest) => LinearInterpolate.Lerp((sbyte[])obj, (sbyte[])other, t, (sbyte[])dest) },
        { typeof(ushort[]  ), static (obj, other, t, dest) => LinearInterpolate.Lerp((ushort[])obj, (ushort[])other, t, (ushort[])dest) },
        { typeof(uint[]    ), static (obj, other, t, dest) => LinearInterpolate.Lerp((uint[])obj, (uint[])other, t, (uint[])dest) },
        { typeof(ulong[]   ), static (obj, other, t, dest) => LinearInterpolate.Lerp((ulong[])obj, (ulong[])other, t, (ulong[])dest) },
    };
#endregion

    #region Scanning for Interpolation Functions
    /// <summary>
    /// Scans for interpolation functions to cache.
    /// </summary>
    /// <param name="types">Optional: Types to scan. If null, scans all loaded
    /// assemblies.</param>
    /// <remarks>
    /// This method is used to allow natural, intuitive use of interpolation
    /// functions that have been created without requiring explicitly
    /// registering them.
    ///
    /// These interpolation functions must be:
    /// - static methods
    /// - named "Lerp"
    /// - be public
    /// - take exactly three parameters (float t, T start, T end),
    /// - return T
    /// </remarks>
    public static void ScanForInterpolationFunctions(IEnumerable<System.Type>? types = null)
    {
        // If the set of types wasnt specified, scann all loaded types in current assemblies
        types ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return []; }
                });

        foreach (var type in types)
        {
            //if (!type.IsSealed || !type.IsAbstract) // not a static class
            //    continue;

            // Only public static methods named "Lerp" with expected sigs
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                // Check the name: The name must be "Lerp"
                if (method.Name != "Lerp")
                    continue;

                // Check the parameters
                var parameters = method.GetParameters();
                if (parameters.Length != 3)
                    continue;
                // Check the first two parameters: they must be equal type
                var pType = parameters[0].ParameterType;
                if (pType != parameters[1].ParameterType)
                    continue;
                // Check the third parameter: it must be float (or compatible)
                if (parameters[2].ParameterType != typeof(float))
                    continue;
                // Check the return type: it must match the type of the first two parameters
                if (method.ReturnType != pType)
                    continue;

                // Only consider if this type is not already present in InterpolationFunctions
                if (InterpolationFunctions.ContainsKey(pType))
                    continue;

                // Make a delegate:
                // This wrapper effectively boxes the arguments to the method call.
                object del(object start, object end, float t)
                {
                    return method.Invoke(null, [start, end, t])!;
                }
                InterpolationFunctions.Add(pType, del);
            }
        }
    }

    /// <summary>
    /// Scans for interpolation functions to cache.
    /// </summary>
    /// <param name="types">Optional: Types to scan. If null, scans all loaded
    /// assemblies.</param>
    /// <remarks>
    /// This method is used to allow natural, intuitive use of interpolation
    /// functions that have been created without requiring explicitly
    /// registering them.
    ///
    /// These interpolate functions must be:
    /// - static methods
    /// - named "Interpolate"
    /// - be public
    /// - take exactly four parameters (T start, T end, float t, T dest),
    /// - return void
    /// </remarks>
    public static void ScanForInterpolateFunctions(IEnumerable<System.Type>? types = null)
    {
        // If the set of types wasnt specified, scann all loaded types in current assemblies
        types ??= AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); } catch { return []; }
                });

        foreach (var type in types)
        {
            // Only public static methods named "Lerp" with expected sigs
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                // Check the name: The name must be "Lerp"
                if (method.Name != "Interpolate")
                    continue;

                // Check the parameters
                var parameters = method.GetParameters();
                if (parameters.Length != 4)
                    continue;
                // Check the first two parameters: they must be equal type
                var pType = parameters[0].ParameterType;
                if (pType != parameters[1].ParameterType)
                    continue;
                // Check the third parameter: it must be float (or compatible)
                if (parameters[2].ParameterType != typeof(float))
                    continue;
                // Check the fourth parameter: it must be the same type as the first two parameters
                if (parameters[3].ParameterType != pType)
                    continue;

                // Only consider if this type is not already present in InterpolateFunctions
                if (InterpolationFunctions.ContainsKey(pType))
                    continue;

                // Make a delegate:
                // This wrapper effectively boxes the arguments to the method call.
                void del(object start, object end, float t, object dest)
                {
                    method.Invoke(null, [start, end, t, dest]);
                }
                InterpolateFunctions.Add(pType, del);
            }
        }
    }


    /// <summary>
    /// Static constructor to ensure interpolation methods are scanned at startup.
    /// </summary>
    /// <remarks>
    /// This ensures that interpolation methods are scanned at startup by invoking
    /// the scan in a static constructor.
    /// </remarks>
    static PropertyInterpolator()
    {
        ScanForInterpolationFunctions();
        ScanForInterpolateFunctions();
    }
    #endregion

    #region Derive Interpolation Functions
    /// <summary>
    /// Creates an interpolation function for a given type that interpolates all
    /// interpolatable properties.
    /// </summary>
    /// <typeparam name="type">The type to create an interpolation function for</typeparam>
    /// <returns>A function that interpolates between two instances of type</returns>
    public static dInterpolate? CreateInterpolationFunction(System.Type type)
    {
        // Try to get cached interpolation function for the type
        if (InterpolateFunctions.TryGetValue(type, out var fn))
            return fn;

        dInterpolate? interpolate;
        // Check if this is an array type - if so, try to create an array
        // interpolation function
        if (type.IsArray)
        {
            interpolate = CreateInterpolationOfArray(type);
        }
        else
            interpolate = CreateInterpolationOfObject(type);

        // Cache the value
        if (null != interpolate)
            InterpolateFunctions[type] = interpolate;
        return interpolate;
    }


    /// <summary>
    /// Creates an interpolation function for a given class or structure type
    /// that interpolates all interpolatable properties.
    /// </summary>
    /// <typeparam name="type">The type to create an interpolation function for</typeparam>
    /// <returns>A function that interpolates between two instances of type</returns>
    static dInterpolate? CreateInterpolationOfObject(System.Type type)
    {
        // See if it is a type that can derive an interpolation function
        // Analyzes a type to find all interpolatable properties.
        var simpleProperties = new List<(dInterpolationFunction, ProxyPropertyDescriptor)>();
        var properties = new List<(dInterpolate, ProxyPropertyDescriptor, bool, bool)>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip properties that cant be set or can't get current values
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            // Look up or derive an interpolation function
            if (InterpolationFunctions.TryGetValue(prop.PropertyType, out var fn1))
            {
                var pd = ProxyPropertyDescriptor.Create(prop);
                simpleProperties.Add((fn1, pd));
                continue;
            }
            // Try one of the more complex ones
            var propertyType = prop.PropertyType;
            var fn = CreateInterpolationFunction(propertyType);
            if (null != fn)
            {
                var pd = ProxyPropertyDescriptor.Create(prop);

                // If the property is a struct or other boxed value type, we
                // need to assign the updated value back to the property in dest,
                // because SetValue on boxed value types will not update the
                // property automatically.
                var needToSetAgain = (propertyType.IsValueType && !propertyType.IsPrimitive && !propertyType.IsEnum);
                var isArray = propertyType.IsArray;
                properties.Add((fn, pd, needToSetAgain, isArray));
            }
        }

        // Scan over the fields
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            // Skip fixed values, like constants and and readonly values
            if (field.IsInitOnly || field.IsLiteral)
                continue;

            // Look up or derive an interpolation function
            if (InterpolationFunctions.TryGetValue(field.FieldType, out var fn1))
            {
                var pd = ProxyPropertyDescriptor.Create(field);
                simpleProperties.Add((fn1, pd));
                continue;
            }
            // Try one of the more complex ones
            var propertyType = field.FieldType;
            var fn = CreateInterpolationFunction(propertyType);
            if (null != fn)
            {
                // Get the property descriptor
                var pd = ProxyPropertyDescriptor.Create(field);

                // If the property is a struct or other boxed value type, we
                // need to assign the updated value back to the property in dest,
                // because SetValue on boxed value types will not update the
                // property automatically.
                var needToSetAgain = (propertyType.IsValueType && !propertyType.IsPrimitive && !propertyType.IsEnum);
                var isArray = propertyType.IsArray;
                properties.Add((fn, pd, needToSetAgain, isArray));
            }
        }
        // If no properties found, return null
        if (properties.Count <= 0 && simpleProperties.Count <= 0)
            return null;

        // Create an interpolation function from the properties
        void interpolate(object start, object end, float t, object dest)
        {
            // Interpolate each property
            foreach (var (interpolate, prop) in simpleProperties)
            {
                // Get the values for the start and end points
                var startValue = prop.GetValue(start);
                var endValue = prop.GetValue(end);

                // Skip if the points are undefined
                if (startValue == null || endValue == null)
                    continue;

                // Interpolate the value using the interpolation function
                var interpolatedValue = interpolate(startValue, endValue, t);
                prop.SetValue(dest, interpolatedValue);
            }

            // Interpolate each property
            foreach (var (interpolate, prop, setAgain, isArray) in properties)
            {
                // Get the values for the start and end points
                var startValue = prop.GetValue(start);
                var endValue = prop.GetValue(end);
                var dest2 = prop.GetValue(dest);

                // Skip if the points are undefined
                if (startValue == null || endValue == null)
                    continue;

                // For array properties, ensure the destination array is
                // initialized to the correct length
                if (isArray)
                {
                    if (  startValue is Array startArray
                       && endValue is Array endArray
                       )
                    {
                        // Determine the target length (use the length of the
                        // start array, or end if different)
                        var targetLength = Math.Max(startArray.Length, endArray.Length);

                        // If destination is null or has wrong length, create a new array
                        if (dest2 is not Array destArray || destArray.Length != targetLength)
                        {
                            var elementType = prop.PropertyType.GetElementType();
                            if (elementType != null)
                            {
                                destArray = Array.CreateInstance(elementType, targetLength);
                                prop.SetValue(dest, destArray);
                                dest2 = destArray;
                            }
                        }
                    }
                }

                // Skip if destination is still null
                if (dest2 == null)
                    continue;

                // Interpolate the value using the interpolation function
                interpolate(startValue, endValue, t, dest2);

                // If the property is a struct or other boxed value type, we
                // need to assign the updated value back to the property in dest,
                // because SetValue on boxed value types will not update the
                // property automatically.
                if (setAgain)
                {
                    prop.SetValue(dest, dest2);
                }
            }
        }
        return interpolate;
    }

    /// <summary>
    /// Creates an interpolation function for a given array type that interpolates all
    /// interpolatable properties.
    /// </summary>
    /// <typeparam name="type">The type to create an interpolation function for</typeparam>
    /// <returns>A function that interpolates between two instances of type</returns>
    static dInterpolate? CreateInterpolationOfArray(System.Type type)
    {
        var elementType = type.GetElementType();
        if (elementType == null)
            return null;

        // Check if we have an interpolation function for the element type
        dInterpolate? elementInterpolateFn = null;

        // First, check if there's a simple interpolation function for the element type
        if (InterpolationFunctions.TryGetValue(elementType, out dInterpolationFunction? elementInterpolationFn))
        {
            // Create an array interpolation function that uses the element interpolation function
            void arrayInterpolate(object start, object end, float t, object dest)
            {
                var startArray = (Array)start;
                var endArray = (Array)end;
                var destArray = (Array)dest;

                if (startArray == null || endArray == null || destArray == null)
                    return;

                // Ensure arrays have the same length
                var length = Math.Min(Math.Min(startArray.Length, endArray.Length), destArray.Length);

                for (int i = 0; i < length; i++)
                {
                    var startValue = startArray.GetValue(i);
                    var endValue = endArray.GetValue(i);

                    if (startValue == null || endValue == null)
                        continue;

                    var interpolatedValue = elementInterpolationFn(startValue, endValue, t);
                    destArray.SetValue(interpolatedValue, i);
                }
            }

            return arrayInterpolate;
        }

        // Try to get a more complex interpolation function for the element type
        elementInterpolateFn = CreateInterpolationFunction(elementType);
        if (elementInterpolateFn != null)
        {
            // Create an array interpolation function that uses the element interpolation function
            void arrayInterpolate(object start, object end, float t, object dest)
            {
                var startArray = (Array)start;
                var endArray = (Array)end;
                var destArray = (Array)dest;

                if (startArray == null || endArray == null || destArray == null)
                    return;

                // Ensure arrays have the same length
                var length = Math.Min(Math.Min(startArray.Length, endArray.Length), destArray.Length);

                for (int i = 0; i < length; i++)
                {
                    var startValue = startArray.GetValue(i);
                    var endValue = endArray.GetValue(i);
                    var destValue = destArray.GetValue(i);

                    if (startValue == null || endValue == null || destValue == null)
                        continue;

                    elementInterpolateFn(startValue, endValue, t, destValue);
                    destArray.SetValue(destValue, i);
                }
            }

            return arrayInterpolate;
        }

        // If we can't create an interpolation function for the array, return null
        return null;
    }

    /// <summary>
    /// Creates a complete interpolation function for a type by automatically finding interpolatable properties.
    /// </summary>
    /// <typeparam name="T">The type to create an interpolation function for</typeparam>
    /// <returns>A function that interpolates between two instances of T</returns>
    public static dInterpolate? CreateInterpolationFunction<T>()
    {
        return CreateInterpolationFunction(typeof(T));
    }
    #endregion
}
