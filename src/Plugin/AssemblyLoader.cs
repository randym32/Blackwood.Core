// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Blackwood;

/// <summary>
/// This is a helper to load plugin assemblies from a given path.
/// </summary>
/// <param name="pluginName">The plugin path</param>
/// <remarks>
/// When a plugin a context is used help ensure that it loads the assemblies
/// from that folder structure.  Multiple plugins could use different versions
/// of of assemblies with the same identifier.  Interestingly, the context
/// allows a plugin to be unloaded.
/// </remarks>
class AssemblyLoader(string pluginName) : AssemblyLoadContext
{
    /// <summary>
    /// This is called by the parent AssemblyLoadContext when the program needs
    /// to load an assembly (esp. relevant to the main DLL of the plugin.)
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to load for the
    /// plugin</param>
    /// <returns>The assembly if found, otherwise null to keep searching</returns>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Load from the applications DLL's first.  This prevents some sort of
        // problem if we let the other search go.  Something about .net Core
        // and Standard not working together (ugh)
        try
        {
            var name = assemblyName.Name;
            if (null != name)
            {
                var assembly = Assembly.Load(name);
                if (null != assembly)
                    return assembly;
            }
        }
        catch (Exception) { }

        // Try the path that the plugin originally came from
        var pluginResolver = new AssemblyDependencyResolver(pluginName);
        var assemblyPath = pluginResolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }
        return null;
    }


    /// <summary>
    /// This is called by the parent AssemblyLoadContextwhen the program needs
    /// to load an unmanaged DLL
    /// </summary>
    /// <param name="unmanagedDllName">The name of the unmanaged DLL that it
    /// wishes to load</param>
    /// <returns>A pointer to the DLL, if found, otherwise null to keep
    /// searching</returns>
    /// <remarks>
    /// The file name is unmanagedDllName plus the extension, if one was not
    /// provided.  The search paths are the current directory, the app domain
    /// base directory, the current directory, and the directory of the
    /// executing assembly.
    /// </remarks>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        // Try to find the DLL.
        // If the path is not absolute, try to find the DLL in the current
        // directory, the app domain base directory, the current directory,
        // and the directory of the executing assembly.
        var name = unmanagedDllName;
        var path = Resolve(name);
        if (!string.IsNullOrEmpty(path))
        {
            // Note: unmanaged DLL's do not get shadow copied.  It could be
            // useful to do that.
            return LoadUnmanagedDllFromPath(path);
        }

        // The file was not found.
        return IntPtr.Zero;
    }


    #region Utility Methods
    /// <summary>
    /// Get the extension for a library file.
    /// </summary>
    /// <returns>The extension for a library file.</returns>
    static string GetLibraryExtension()
    {
        // Use RuntimeInformation and a switch to determine the preferred extension:
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll"
             : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ".so"
             : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib"
             : ".dll";
    }

    /// <summary>
    /// The path to the plugin.
    /// </summary>
    internal string libraryPath = String.Empty;


    /// <summary>
    /// Resolve the path to a library file for the given plugin name.
    /// </summary>
    /// <param name="name">The name of the library to resolve.</param>
    /// <returns>The path to the library, or an empty string if not found.
    /// </returns>
    /// <remarks>
    /// The file name is the name plus the extension, if one was not provided.
    /// The search paths are:
    /// - the current directory,
    /// - the app domain base directory,
    /// - the current directory,
    /// - the directory of the executing assembly.
    /// </remarks>
    internal string Resolve(string name)
    {
        // If the library path is already set, return it.
        if (!string.IsNullOrEmpty(libraryPath))
        {
            return libraryPath;
        }

        // To search for the plugin file, we need to add the extension to the
        // name, if it is not already there.
        var ext = GetLibraryExtension();
        if (!name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            name += ext;

        // Try to find the DLL in the current directory, the app domain base
        // directory, the current directory, and the directory of the executing
        // assembly.
        libraryPath = FindFile(name);
        if (string.IsNullOrEmpty(libraryPath))
        {
            // Try the .NET framework resolver, which uses a deps.json file
            try
            {
                var pluginResolver = new AssemblyDependencyResolver(pluginName);
                var path = pluginResolver.ResolveUnmanagedDllToPath(name);
                libraryPath = path ?? string.Empty;
            }
            catch (InvalidOperationException)
            {
                // If the provided component path is not a managed application,
                // fall back silently
                libraryPath = string.Empty;
            }
        }

        return libraryPath;
    }


    /// <summary>
    /// Find a file in common locations
    /// </summary>
    /// <param name="name">Name of the file</param>
    /// <returns>Full path to the file or empty string if not found</returns>
    static string FindFile(string name)
    {
        // Search for the DLL in the current directory, the app domain base
        // directory, the current directory, and the directory of the executing
        // assembly.
        string[] searchPaths =
        [
            name, // Current directory, or if the name has a rooted path
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name),
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", name),
            Path.Combine(FS.ExeFilePath??""          , name),
            Path.Combine(Environment.CurrentDirectory, name),
            // Path.Combine(FS.CommonApplicationDataPath, name),
            // Path.Combine(FS.AppDataPath              , name),
        ];

        // Search for the DLL in the search paths
        foreach (string path in searchPaths)
            if (File.Exists(path))
                return Path.GetFullPath(path);

        // The file was not found.
        return string.Empty;
    }
#endregion
}

