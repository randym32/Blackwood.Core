// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;


/// <summary>
/// An interface for accessing application-level information and functionality.
/// </summary>
/// <remarks>
/// The control center for your app's universe (or at least its runtime).
/// </remarks>
public static partial class Application
{
    /// <summary>
    /// The application name.
    /// </summary>
    /// <value>
    /// The application name.
    /// </value>
    /// <remarks>
    /// This method will return the name of the application from the assembly.
    /// If the name is not found, it will return "unknownApp".
    /// </remarks>
    public static string? Name => Blackwood.Base.Application.Name;
}

