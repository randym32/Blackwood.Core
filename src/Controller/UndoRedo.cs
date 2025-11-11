// Copyright (c) 2009-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;

/// <summary>
/// This is used to hold the undo/redo items.
/// </summary>
/// <param name="description">A description of what will happen</param>
/// <param name="undo">This will undo an action</param>
/// <param name="redo">This will redo an action</param>
/// <remarks>
/// Creates an undo-redo helper.  I've never really understood why C# does not
/// have a standard Undo/Redo framework... or I've never found it.
/// </remarks>
public class UndoRedo(string description, Action undo, Action redo)
{
    /// <summary>
    /// The next item in the undo/redo stack.
    /// </summary>
    public UndoRedo? next;

    /// <summary>
    /// A description of what will happen.
    /// </summary>
    public readonly string Description = description;

    /// <summary>
    /// This will undo an action.
    /// </summary>
    public readonly Action Undo = undo;


    /// <summary>
    /// This will redo an action.
    /// </summary>
    public readonly Action Redo = redo;
}
