// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Blackwood;

/// <summary>
/// This class is used to provide access to the plugin functions.
/// </summary>
public class AssemblyWrapper : IDisposable
{
    #region Properties
    /// <summary>
    /// The default class to look for procedures in assemblies.
    /// </summary>
    readonly string Class="";

    /// <summary>
    /// The main assembly for the plugin.
    /// </summary>
    readonly Assembly? assembly;

    /// <summary>
    /// Handle to the loaded DLL
    /// </summary>
    IntPtr handle_;

    /// <summary>
    /// Cache of function pointers for the loaded plugin.
    /// </summary>
    readonly Dictionary<string, Delegate> functionCache_ = [];

    /// <summary>
    /// Flag to indicate if the object has been disposed
    /// </summary>
    bool disposed_;
    #endregion

    #region Constructor and Disposal
    /// <summary>
    /// Initialize the plugin wrapper
    /// </summary>
    /// <param name="assembly">The assembly to wrap</param>
    /// <param name="Class">The default class to look for procedures in
    /// assemblies.</param>
    internal AssemblyWrapper(Assembly assembly, string Class)
    {
        this.Class = Class;
        this.assembly = assembly;
    }

    /// <summary>
    /// Initialize the plugin wrapper
    /// </summary>
    /// <param name="handle">The handle to the DLL</param>
    internal AssemblyWrapper(IntPtr handle)
    {
        this.handle_ = handle;
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        if (disposed_)
            return;
        disposed_ = true;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    /// <param name="disposing">True if disposing,
    /// false if finalizing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        // Dispose of the cache contents
        if (functionCache_ != null)
        {
            // Properly dispose each item in functionCache_ if it implements
            // IDisposable
            foreach (var item in functionCache_.Values)
            {
                try
                {
                    if (item is IDisposable disposable)
                        disposable.Dispose();
                }
                catch
                {
                    // Ignore exceptions during disposal of individual items
                }
            }
            functionCache_.Clear();
        }

        // Try to unload the DLL
        if (handle_ != IntPtr.Zero)
        {
            NativeLibrary.Free(handle_);
            handle_ = IntPtr.Zero;
        }
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Checks to see if they are the same or not
    /// </summary>
    /// <param name="b">The object to compare to</param>
    /// <returns>True if the plugins are the same, false otherwise</returns>
    public override bool Equals(object? b)
    {
        // Compare plugins
        if (b is not AssemblyWrapper p2)
            return false;
        // Check based on the names
        return (assembly != null && assembly.Equals(p2.assembly));
    }


    /// <summary>
    /// Get the hash code for the plugin.
    /// </summary>
    /// <returns>The hash code for the plugin.</returns>
    public override int GetHashCode()
    {
        // Use DLL and assembly for hash code calculation, as in Equals
        return assembly == null ? base.GetHashCode() : assembly.GetHashCode();
    }
    #endregion

    #region C# Class and Method Lookup
    /// <summary>
    /// Finds all classes in the assembly that are subclasses of the specified
    /// type.
    /// </summary>
    /// <param name="asm">The assembly to search.</param>
    /// <param name="type">The base type or interface to match.</param>
    /// <returns>An enumerable of types in the assembly that match the specified
    /// type.</returns>
    IEnumerable<Type> FindClasses(Type type)
    {
        if (null == assembly)
            return [];
        return assembly.FindClasses(type);
    }


    /// <summary>
    /// Look up a method in the loaded plugin.
    /// </summary>
    /// <typeparam name="T">The delegate type to return</typeparam>
    /// <param name="className">Name of the class to look up</param>
    /// <param name="methodName">Name of the method to look up</param>
    /// <param name="target">The target object to call the method on.  This is
    /// only used for instance methods.</param>
    /// <returns>The method if found, otherwise null</returns>
    T? LookUpMethod<T>(string className, string methodName, object? target = null)
         where T : Delegate
    {
        // Look up the class, return null if not found
        var type = assembly?.GetType(className);
        if (null == type)
            return null;

        // Look up the method, return null if not found
        var methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (null == methodInfo)
            return null;

        // For static methods, return the delegate
        if (methodInfo.IsStatic)
            return methodInfo.CreateDelegate<T>();

        // For instance methods, return the delegate
        return methodInfo.CreateDelegate<T>(target);
    }
    #endregion

    #region C/C++ Function Loading
    /// <summary>
    /// Get a delegate for a function in the loaded plugin.
    /// </summary>
    /// <typeparam name="T">Delegate type, specifying the calling signature of
    /// the function.  This should have the attribute
    /// [UnmanagedFunctionPointer(CallingConvention.Cdecl)]</typeparam>
    /// <param name="functionName">Name of the function</param>
    /// <returns>Delegate or null if the function is not found</returns>
    T? LookUpFunction<T>(string functionName)
        where T : Delegate
    {
        // Check cache first, return the delegate if it is found
        if (functionCache_.TryGetValue(functionName, out Delegate? cachedDelegate)
            && null != cachedDelegate)
        {
            // The function was found in the cache; return the delegate
            return (T)cachedDelegate;
        }

        // Check if the DLL is loaded, return null if not
        if (handle_ == IntPtr.Zero)
        {
            // There isn't a handle to load from
            return null;
        }

        // Get the function pointer, return null if not found
        var functionPtr = NativeLibrary.GetExport(handle_, functionName);
        if (functionPtr == IntPtr.Zero)
        {
            // The function was not found in the DLL; return the error message
            return null;
        }

        // Create the delegate for the specified function with the specified
        // delegate type, return null if not found
        T? function = Marshal.GetDelegateForFunctionPointer<T>(functionPtr);
        if (function == null)
            return null;

        // The delegate was created successfully, add it to the cache and
        // return it
        functionCache_[functionName] = function;
        return function;
    }
    #endregion

    /// <summary>
    /// Look up a procedure or class method in the loaded plugin.
    /// </summary>
    /// <typeparam name="T">The delegate type to return</typeparam>
    /// <param name="className">Name of the class to look up.  Only used for
    /// .Net assemblies.</param>
    /// <param name="methodName">Name of the procedure or method to look up.
    /// Only used for C# methods.</param>
    /// <param name="target">The target object to call the method on.  This is
    /// only used for C# instance methods.</param>
    /// <returns>The procedure or method if found, otherwise null</returns>
    public T? GetMethod<T>( string className
                          , string methodName
                          , object? target = null
                          )
         where T : Delegate
    {
        // If this is a wrapper around an assembly, use the C# method lookup
        if (null != assembly)
            return LookUpMethod<T>(className, methodName, target);

        // Otherwise, use the C/C++ function lookup
        return LookUpFunction<T>(methodName);
    }


    /// <summary>
    /// Look up a procedure in the loaded plugin.
    /// </summary>
    /// <typeparam name="T">The delegate type to return</typeparam>
    /// <param name="className">Name of the class to look up.  Only used for
    /// .Net assemblies.</param>
    /// <param name="methodName">Name of the procedure or method to look up.
    /// Only used for C# methods.</param>
    /// <returns>The procedure or method if found, otherwise null</returns>
    public T? GetProcedure<T>(string procedureName)
         where T : Delegate
    {
        // If this is a wrapper around an assembly, use the C# method lookup
        if (null != assembly)
            return LookUpMethod<T>(Class, procedureName, null);

        // Otherwise, use the C/C++ function lookup
        return LookUpFunction<T>(procedureName);
    }
}
