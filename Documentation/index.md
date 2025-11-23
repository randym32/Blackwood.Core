# Blackwood.Core

Welcome to the Blackwood.Core documentation. This library provides system
abstractions and utilities for .NET applications.

## Overview

Blackwood.Core provides a foundation for application preferences, menu
management, property access, and charting utilities to help build modular,
testable system components.

## Features

- **Application**: Application-level information, preferences system and
  assembly scanning utilities
- **Menu Tools**: Menu system for building application menus from JSON templates
- **Properties**: Property proxy for dynamic property access
- **Charting**: Chart data structures and point reduction algorithms for data
  visualization
- **Application Instance Messaging**: Forwarder utilities for safely handing off
  command-line arguments (like file-open requests) to an already running
  application instance over named pipes

## Getting Started

### Installation

You can consume the project either by referencing the project from source or by using the compiled binaries produced by the solution.

#### From Source (recommended during development)
1. Add a project reference to `Blackwood.Core/src/Blackwood.Core.csproj` in your solution:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\..\Blackwood.Core\src\Blackwood.Core.csproj" />
   </ItemGroup>
   ```
2. Build your solution as usual; the project will restore and compile alongside your code.

#### From Binary (local build output)
After building the `Blackwood.Core` project, reference the produced DLL from
the `bin` folder appropriate to your configuration and target framework.


## Building

- Open `Blackwood.Core.sln` in Visual Studio or build from the command line:
  ```bash
  dotnet build Blackwood.Core/Blackwood.Core.sln -c Release
  ```

## Running Tests

- Open the `tests` project solution or run:
  ```bash
  dotnet test Blackwood.Core/tests/Blackwood.Core.tests.csproj -c Release
  ```

## Documentation

For detailed documentation on specific features, see:

- [Getting Started](articles/getting-started.md) - Quick start guide
- [Introduction](articles/intro.md) - Overview of Blackwood.Core
- [Charting](articles/charting.md) - Chart data structures and point reduction
  algorithms
- [Menu Configuration](articles/menu-configuration.md) - Menu configuration
  system documentation
- [Preferences](articles/preferences.md) - Preferences system documentation
- [API Reference](api/index.md) - Complete API documentation

## Contributing

Please read `CONTRIBUTING.md` and adhere to the `CODE_OF_CONDUCT.md` in the root
of this repository. PRs with focused, well-documented changes are welcome.

## License

This project is licensed under the terms of the repository's `LICENSE` file.
