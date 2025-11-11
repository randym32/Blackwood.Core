# Introduction to Blackwood.Core

**Blackwood.Core** is a .NET library providing utilities for system
integration, including application preferences, menu management, and property
access.

## Key Features

Blackwood.Core provides the following capabilities:

- **Application Information**: Access application name and assembly information
- **Preferences System**: Automatic discovery and management of application preferences
- **Menu Management**: Build application menus from JSON templates
- **Property Access**: Dynamic property access through proxy objects

## Getting Started

### Application Information

Access application information using the `Application` class:

```csharp
using Blackwood;

// Get application name from assembly
string? applicationName = Application.Name;

Console.WriteLine($"Application: {applicationName}");
```

### Preferences

The preferences system automatically discovers static properties marked with
`DescriptionAttribute` and makes them available through a proxy object:

```csharp
using Blackwood;

// Get the preferences proxy object
var preferences = Application.PreferencesProxy();

// Use with PropertyGrid or access properties dynamically
```

For detailed information, see the [Preferences](preferences.md) documentation.

### Menu Management

Load menu templates from JSON files or embedded resources:

```csharp
using Blackwood;

// Load menu templates for the application
Menus.LoadMenuTemplates();

// Or load for a specific application name
Menus.LoadMenuTemplates("MyApplication");
```

Menu templates can be defined in JSON format and loaded from files or embedded
resources.

## Next Steps

- Read the [Getting Started](getting-started.md) guide for installation instructions
- Explore the [Preferences](preferences.md) documentation for detailed preference management
- Check the [API Reference](../api/) for complete API documentation
