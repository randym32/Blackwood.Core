using System.Reflection;
using Blackwood;

namespace AssembliesExample;

/// <summary>
/// Example demonstrating how to use Application.Assemblies() to enumerate
/// and work with assemblies in your application.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Assemblies Example");
        Console.WriteLine("==================\n");

        // Display the application name
        var applicationName = Application.Name;
        Console.WriteLine($"Application Name: {applicationName ?? "(null)"}");

        // Demonstrate enumerating assemblies
        EnumerateAssemblies();

        // Demonstrate finding types in assemblies
        FindTypesInAssemblies();

        // Demonstrate assembly information
        DisplayAssemblyInformation();

        // Demonstrate searching for resources
        SearchForResources();
    }

    /// <summary>
    /// Demonstrates how to enumerate assemblies using Application.Assemblies().
    /// </summary>
    static void EnumerateAssemblies()
    {
        Console.WriteLine("1. Enumerating Assemblies");
        Console.WriteLine("-------------------------");

        // Application.Assemblies() returns assemblies in a specific order:
        // 1. Entry assembly (if available)
        // 2. Executing assembly
        // 3. All loaded assemblies (in reverse order, most recent first)
        var assemblies = Application.Assemblies().ToList();

        Console.WriteLine($"Total assemblies found: {assemblies.Count}\n");

        Console.WriteLine("Assembly order (as returned by Application.Assemblies()):");
        for (int i = 0; i < assemblies.Count; i++)
        {
            var asm = assemblies[i];
            var name = asm.GetName().Name ?? "Unknown";
            var location = asm.Location;
            var locationInfo = string.IsNullOrEmpty(location)
                ? "(dynamic or in-memory)"
                : Path.GetFileName(location);

            Console.WriteLine($"  {i + 1}. {name}");
            Console.WriteLine($"     Location: {locationInfo}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates finding types in assemblies.
    /// </summary>
    static void FindTypesInAssemblies()
    {
        Console.WriteLine("2. Finding Types in Assemblies");
        Console.WriteLine("------------------------------");

        // Search for a specific type across all assemblies
        var targetTypeName = "Application";
        Console.WriteLine($"Searching for type '{targetTypeName}'...\n");

        var foundTypes = new List<(Assembly Assembly, Type Type)>();

        foreach (var assembly in Application.Assemblies())
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.Name.Contains(targetTypeName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var type in types)
                {
                    foundTypes.Add((assembly, type));
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Some assemblies may have types that can't be loaded
                Console.WriteLine($"  Warning: Could not load all types from {assembly.GetName().Name}");
            }
            catch (Exception ex)
            {
                // Skip assemblies that can't be inspected
                Console.WriteLine($"  Warning: Could not inspect {assembly.GetName().Name}: {ex.Message}");
            }
        }

        if (foundTypes.Any())
        {
            Console.WriteLine($"Found {foundTypes.Count} matching type(s):\n");
            foreach (var (assembly, type) in foundTypes)
            {
                Console.WriteLine($"  Type: {type.FullName}");
                Console.WriteLine($"  Assembly: {assembly.GetName().Name}");
                Console.WriteLine($"  Namespace: {type.Namespace ?? "(no namespace)"}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("No matching types found.\n");
        }
    }

    /// <summary>
    /// Demonstrates displaying detailed assembly information.
    /// </summary>
    static void DisplayAssemblyInformation()
    {
        Console.WriteLine("3. Assembly Information");
        Console.WriteLine("------------------------");

        // Get the entry assembly (if available)
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            Console.WriteLine("Entry Assembly:");
            DisplayAssemblyDetails(entryAssembly);
            Console.WriteLine();
        }

        // Get the executing assembly
        var executingAssembly = Assembly.GetExecutingAssembly();
        Console.WriteLine("Executing Assembly:");
        DisplayAssemblyDetails(executingAssembly);
        Console.WriteLine();

        // Display information about Blackwood.Core assembly
        var blackwoodCoreAssembly = typeof(Application).Assembly;
        Console.WriteLine("Blackwood.Core Assembly:");
        DisplayAssemblyDetails(blackwoodCoreAssembly);
        Console.WriteLine();
    }

    /// <summary>
    /// Displays detailed information about an assembly.
    /// </summary>
    static void DisplayAssemblyDetails(Assembly assembly)
    {
        var name = assembly.GetName();
        Console.WriteLine($"  Name: {name.Name}");
        Console.WriteLine($"  Full Name: {name.FullName}");
        Console.WriteLine($"  Version: {name.Version}");

        var location = assembly.Location;
        if (!string.IsNullOrEmpty(location))
        {
            Console.WriteLine($"  Location: {location}");
            if (File.Exists(location))
            {
                var fileInfo = new FileInfo(location);
                Console.WriteLine($"  File Size: {fileInfo.Length:N0} bytes");
                Console.WriteLine($"  Last Modified: {fileInfo.LastWriteTime}");
            }
        }
        else
        {
            Console.WriteLine($"  Location: (dynamic or in-memory assembly)");
        }

        // Try to get informational version
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (infoVersion != null)
        {
            Console.WriteLine($"  Informational Version: {infoVersion.InformationalVersion}");
        }

        // Count types in the assembly
        try
        {
            var typeCount = assembly.GetTypes().Length;
            Console.WriteLine($"  Types: {typeCount}");
        }
        catch
        {
            Console.WriteLine($"  Types: (unable to enumerate)");
        }
    }

    /// <summary>
    /// Demonstrates searching for embedded resources in assemblies.
    /// </summary>
    static void SearchForResources()
    {
        Console.WriteLine("4. Searching for Embedded Resources");
        Console.WriteLine("-----------------------------------");

        // Search for embedded resources across all assemblies
        var resourceNamePattern = "README";
        Console.WriteLine($"Searching for resources containing '{resourceNamePattern}'...\n");

        var foundResources = new List<(Assembly Assembly, string ResourceName)>();

        foreach (var assembly in Application.Assemblies())
        {
            try
            {
                var resourceNames = assembly.GetManifestResourceNames()
                    .Where(name => name.Contains(resourceNamePattern, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var resourceName in resourceNames)
                {
                    foundResources.Add((assembly, resourceName));
                }
            }
            catch (Exception ex)
            {
                // Skip assemblies that can't be inspected
                Console.WriteLine($"  Warning: Could not inspect resources in {assembly.GetName().Name}: {ex.Message}");
            }
        }

        if (foundResources.Any())
        {
            Console.WriteLine($"Found {foundResources.Count} matching resource(s):\n");
            foreach (var (assembly, resourceName) in foundResources)
            {
                Console.WriteLine($"  Resource: {resourceName}");
                Console.WriteLine($"  Assembly: {assembly.GetName().Name}");
                Console.WriteLine();
            }
        }
        else
        {
            Console.WriteLine("No matching resources found.\n");
        }

        // Display all resources in the executing assembly
        Console.WriteLine("All resources in executing assembly:");
        var executingAssembly = Assembly.GetExecutingAssembly();
        var allResources = executingAssembly.GetManifestResourceNames();
        if (allResources.Any())
        {
            foreach (var resource in allResources)
            {
                Console.WriteLine($"  - {resource}");
            }
        }
        else
        {
            Console.WriteLine("  (no embedded resources)");
        }
        Console.WriteLine();
    }
}

