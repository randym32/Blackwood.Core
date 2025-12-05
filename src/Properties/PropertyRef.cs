// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;

/// <summary>
/// Represents a reference to a property or port within a node graph,
/// specified by a path string that can navigate through nested members.
/// </summary>
/// <remarks>
/// A class, to allow distinguishing it from strings as literals.
/// </remarks>
public class PropertyRef
{
    /// <summary>
    /// The path to the property.  This starts with the node identifier,
    /// followed by a '.' and then the path to the property.
    /// The path to the property is a dot-separated list of property names.
    /// </summary>
    public readonly string path;

    /// <summary>
    /// Construct a property reference.
    /// </summary>
    /// <param name="path">The path to the property.</param>
    public PropertyRef(string path)
    {
        this.path = path;
    }
}
