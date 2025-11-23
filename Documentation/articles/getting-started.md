# Getting Started

This guide will help you get up and running with Blackwood.Core.

## Prerequisites

- .NET 9.0 or later
- Visual Studio 2022 or later

## Installation

### From Source

1. Clone the repository or add as a submodule
2. Add a project reference to your solution:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\..\Blackwood.Core\src\Blackwood.Core.csproj" />
   </ItemGroup>
   ```
3. Build your solution; the project will restore and compile alongside your code

### From Binary

After building, reference the compiled DLL from the `bin` folder matching your
configuration and target framework.

## Building

Build the project from the command line:

```bash
dotnet build Blackwood.Core/Blackwood.Core.sln -c Release
```

Or open `Blackwood.Core.sln` in Visual Studio and build from there.

## Running Tests

Run the test suite from the command line:

```bash
dotnet test Blackwood.Core/tests/Blackwood.Core.tests.csproj -c Release
```

Or open the test project in Visual Studio and run tests from the Test Explorer.

## Next Steps

- Read the [Introduction](intro.md) for an overview
- See [Preferences](preferences.md) for preference management
- See [Assemblies](assemblies.md) for assembly enumeration
- Check the [API Reference](../api/index.md) for complete API documentation
