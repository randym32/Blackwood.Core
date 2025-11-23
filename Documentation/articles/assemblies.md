# Working with Assemblies

Use `Application.Assemblies()` to enumerate the assemblies in your application.
It returns assemblies in priority order, which helps when searching for
resources, types, or other assembly-level information.

## Overview

**`Application.Assemblies()`**: Returns assemblies, giving priority to your
  executing application's assembly.

## Scanning for Assemblies

The `Application.Assemblies()` method returns assemblies in a specific order:

1. **Entry Assembly** (if available) - Your main application assembly
2. **Executing Assembly** - The assembly where the code is running
3. **All Loaded Assemblies** - Other assemblies loaded in the application
   domain, in reverse order (most recent first)

Resources in your main application are checked before resources in referenced libraries.

## Enumerating Assemblies

To enumerate all assemblies:

```csharp
using Blackwood;
using System.Reflection;

// Get all assemblies in priority order
foreach (Assembly assembly in Application.Assemblies())
{
    // Get some information about each assembly
    var name = assembly.GetName().Name;
    var location = assembly.Location;
    Console.WriteLine($"Assembly: {name}");
    Console.WriteLine($"Location: {location}");
}
```

## Finding Types in Assemblies

Searching for types across assemblies:

```csharp
using Blackwood;
using System.Reflection;

// Search for a specific type
string targetTypeName = "Application";
var foundTypes = new List<Type>();

// Iterate through each assembly returned by Application.Assemblies()
foreach (Assembly assembly in Application.Assemblies())
{
    try
    {
        // Get all types from the current assembly whose name contains the target string (case-insensitive)
        var types = assembly.GetTypes()
            .Where(t => t.Name.Contains(targetTypeName, StringComparison.OrdinalIgnoreCase));

        // Add the matching types to the foundTypes list
        foundTypes.AddRange(types);
    }
    catch (ReflectionTypeLoadException)
    {
        // Skip assemblies with types that can't be loaded
    }
}

foreach (var type in foundTypes)
{
    Console.WriteLine($"Found: {type.FullName} in {type.Assembly.GetName().Name}");
}
```

## Searching for Embedded Resources

The priority ordering means your application's resources are checked before
library resources.

### Finding a Single Resource

To find a resource in assemblies:

```csharp
using Blackwood;
using System.IO;
using System.Reflection;

// The resource name or path to find
string resourcePath = "Resources/config.json";
Stream? resourceStream = null;

// Iterate over all assemblies used in the application.
foreach (Assembly assembly in Application.Assemblies())
{
    // Create a wrapper around the assembly
    var embeddedResources = new EmbeddedResources(assembly);

    // Check if the resource exists in this assembly
    if (embeddedResources.Exists(resourcePath))
    {
        // If found, get a stream for the resource
        resourceStream = embeddedResources.Stream(resourcePath);
        if (resourceStream != null)
        {
            // Log which assembly contained the resource
            Console.WriteLine($"Found resource in assembly: {assembly.GetName().Name}");
            break; // Stop searching after first match
        }
    }
}

// If the resource stream was found, use it
if (resourceStream != null)
{
    // Use a StreamReader to read the contents of the resource
    using var reader = new StreamReader(resourceStream);

    // Print the content of the resource if found, and indicate which assembly provided it.
    string content = reader.ReadToEnd();
    Console.WriteLine(content);
}
```


### Finding All Matching Resources

To collect all matching resources instead of stopping at the first match:

```csharp

// The name (or path) of  the embedded resource you want to find
string resourcePath = "Resources/config.json";
var foundResources = new List<(Assembly assembly, Stream stream)>();

// Iterate over every assembly in use
foreach (Assembly assembly in Application.Assemblies())
{
    // Create a wrapper around this assembly
    var embeddedResources = new EmbeddedResources(assembly);

    // Check if the resource exists in this assembly
    if (embeddedResources.Exists(resourcePath))
    {
        // Get a stream to the resource (if found)
        var stream = embeddedResources.Stream(resourcePath);

        // Only add to the list if the stream isn't null
        if (stream != null)
        {
            foundResources.Add((assembly, stream));
        }
    }
}

// Process all found resources
foreach (var (assembly, stream) in foundResources)
{
    // Log the assembly name that contains the resource
    Console.WriteLine($"Found in: {assembly.GetName().Name}");

    // TODO: Add your logic here to process the resource stream (e.g., read its contents)

    // Always dispose streams when done to free resources
    stream.Dispose();
}
```
