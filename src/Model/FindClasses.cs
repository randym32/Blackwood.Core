// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Blackwood;

/// <summary>
/// This class provides helper utilities to find classes in assemblies,
/// and access their properties.
/// </summary>
public static partial class AssemblyUtils
{
    /// <summary>
    /// Loads an assembly from the specified file and returns the set of matching classes.
    /// </summary>
    /// <param name="strFile">The full path to the assembly file (.dll or .exe).</param>
    /// <param name="type">The base type or interface to match.</param>
    /// <returns>An enumerable of types in the assembly that match the specified type.</returns>
    public static ReturnStatus<IEnumerable<Type>> FindClassesInAssembly(string strFile, Type type)
    {
        // Get the full path of the file
        strFile = Path.GetFullPath(strFile);

        // Check if file exists
        if (!File.Exists(strFile))
            return ReturnStatus<IEnumerable<Type>>.Failed($"Assembly file not found: {strFile}");

        // Check file extension to determine loading method
        var extension = Path.GetExtension(strFile).ToLowerInvariant();
        if (extension != ".dll" && extension != ".exe")
            return ReturnStatus<IEnumerable<Type>>.Failed($"Unsupported file type: {strFile}. Only .dll and .exe files are supported.");

        var actualFile = strFile;

        // For executables, try multiple approaches
        // check if the file is a valid .NET assembly
        Assembly? asm = null;
        try
        {
            // Try different loading methods based on file type
            if (extension == ".exe")
            {
                try
                {
                    asm = Assembly.LoadFile(actualFile);
                }
                catch (BadImageFormatException)
                {
                    // For .exe files, try to find a corresponding .dll file
                    var dllPath = Path.ChangeExtension(strFile, ".dll");
                    if (File.Exists(dllPath))
                    {
                        actualFile = dllPath;
                        extension = ".dll";
                    }
                }
            }
            // For libraries, or if LoadFile fails, try LoadFrom
            if (null == asm)
                asm = Assembly.LoadFrom(actualFile);
        }
        catch (BadImageFormatException ex)
        {
            // If we tried a .dll file and it failed, try the original .exe
            if (actualFile == strFile)
                return ReturnStatus<IEnumerable<Type>>.Failed($"Invalid assembly file: {strFile}. The file is not a valid .NET assembly or may be corrupted.");

            try
            {
                asm = Assembly.LoadFile(strFile);
            }
            catch
            {
                return ReturnStatus<IEnumerable<Type>>.Failed($"Cannot load assembly from either {strFile} or {actualFile}. The files are not valid .NET assemblies.");
            }
        }
        catch (FileLoadException ex)
        {
            return ReturnStatus<IEnumerable<Type>>.Failed($"Failed to load assembly: {actualFile}. {ex.Message}");
        }
        catch (Exception ex)
        {
            return ReturnStatus<IEnumerable<Type>>.Failed($"Error loading assembly: {actualFile}. {ex.Message}");
        }

        // Build a collection of the nodes that are subclasses of STNode in the assembly
        return new ReturnStatus<IEnumerable<Type>>(FindClassesInAssembly(asm, type));
    }

    /// <summary>
    /// Finds all classes in the assembly that are subclasses of the specified type.
    /// </summary>
    /// <param name="asm">The assembly to search.</param>
    /// <param name="type">The base type or interface to match.</param>
    /// <returns>An enumerable of types in the assembly that match the specified type.</returns>
    static IEnumerable<Type> FindClassesInAssembly(Assembly asm, Type type)
    {
        return asm.GetTypes().Where(t => t != null && !t.IsAbstract && t.IsSubclassOf(type));
    }

    /// <summary>
    /// Finds all classes in the current AppDomain that are subclasses of the specified type.
    /// </summary>
    /// <param name="type">The base type or interface to match.</param>
    /// <returns>An enumerable of types in the current AppDomain that match the specified type.</returns>
    public static IEnumerable<Type> FindClasses(Type type)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var t in FindClassesInAssembly(asm, type))
                    yield return t;
    }
}