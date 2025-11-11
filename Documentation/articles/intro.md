# Introduction to Blackwood.Core

**Blackwood.Core** is a .NET library of utilities for system integration,
including application preferences, menu management, and property access.

## Key Features

Blackwood.Core provides the following:

- **Application Information**: Access application name and assembly information
- **Preferences System**: Automatic discovery and management of application preferences
- **Menu Management**: Build application menus from JSON templates
- **Property Access**: Dynamic property access through proxy objects
- **Assembly Enumeration**: Enumerate and search assemblies in priority order

## Getting Started

### Application Information

Get application information from the `Application` class:

```csharp
using Blackwood;

// Get application name from assembly
string? applicationName = Application.Name;

Console.WriteLine($"Application: {applicationName}");

// Enumerate loaded assemblies
foreach (Assembly assembly in Application.Assemblies())
{
    Console.WriteLine($"Assembly: {assembly.GetName().Name}");
    Console.WriteLine($"  Version: {assembly.GetName().Version}");
    Console.WriteLine($"  Location: {assembly.Location}");
}
```

For more details, see the [Assemblies](assemblies.md) documentation.

### Preferences

The preferences system discovers static properties marked with
`DescriptionAttribute` and exposes them through a proxy object:

```csharp
using Blackwood;

var preferences = Application.PreferencesProxy();

// Use with PropertyGrid or access properties dynamically
```

See the [Preferences](preferences.md) documentation for details.

### Menu Management

Load menu templates from JSON files or embedded resources:

```csharp
using Blackwood;

// Load menu templates for the application
Menus.LoadMenuTemplates();

// Or load for a specific application name
Menus.LoadMenuTemplates("MyApplication");
```

Menu templates are defined in JSON and loaded from files or embedded resources.

## Next Steps

- Read the [Getting Started](getting-started.md) guide for installation
- See [Preferences](preferences.md) for preference management
- See [Assemblies](assemblies.md) for assembly enumeration
- Check the [API Reference](../api/) for complete API documentation
