# Blackwood.Core Examples

This folder contains example projects demonstrating how to use Blackwood.Core features.

## Examples

### PreferencesExample

Demonstrates the preferences system:
- Defining preferences with attributes
- Loading and saving preferences
- Using checkpoints to revert changes
- Resetting to default values

Run: `dotnet run --project PreferencesExample`


### UndoRedoExample

Demonstrates undo/redo functionality:
- Creating undo/redo operations
- Building an undo stack
- Implementing undo and redo

Run: `dotnet run --project UndoRedoExample`

### AssembliesExample

Demonstrates assembly enumeration and inspection:
- Enumerating assemblies using Application.Assemblies()
- Finding types across assemblies
- Displaying assembly information (name, version, location)
- Searching for embedded resources

Run: `dotnet run --project AssembliesExample`

## Building All Examples

To build all examples:

```bash
dotnet build
```

## Running All Examples

Each example can be run individually:

```bash
dotnet run --project PreferencesExample
dotnet run --project UndoRedoExample
dotnet run --project AssembliesExample
```

