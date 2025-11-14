// Copyright (c) 2009-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;

/// <summary>
/// A stack of undo/redo items.
/// </summary>
/// <remarks>
/// This stack is used to store the undo/redo items for a document or workspace.
/// </remarks>
public partial class UndoRedoController
{
    /// <summary>
    /// The undo stack for this document or workspace.  This is the stack of
    /// items that can be undone.
    /// </summary>
    protected UndoRedo? undoStack;

    /// <summary>
    /// The redo stack for this document or workspace.  This is the stack of
    /// items that can be re-applied.
    /// </summary>
    protected UndoRedo? redoStack;

    /// <summary>
    /// The top item on the undo stack.  This is the item that can be undone.
    /// </summary>
    public UndoRedo? UndoItem => undoStack;

    /// <summary>
    /// The top item on the redo stack.  This is the item that can be re-applied.
    /// </summary>
    public UndoRedo? RedoItem => redoStack;

    /// <summary>
    /// Clears the undo and redo stacks.
    /// </summary>
    public void Clear()
    {
        // Reset the undo redo stack
        undoStack = null;
        redoStack = null;
    }

    /// <summary>
    /// Append an undo/redo item to the undo stack.  This is the item that can be undone.
    /// </summary>
    /// <param name="UR">The undo/redo item to append.</param>
    public void AppendUndo(UndoRedo UR)
    {
        lock (this)
        {
            // Link it to being the head of the stack
            UR.next = undoStack;
            undoStack = UR;

            // Throw out the redo stack (those its can't be redone now that
            // we've changed everything)
            redoStack = null;
        }
    }

    /// <summary>
    /// Undo the top item on the undo stack.
    /// </summary>
    public void Undo()
    {
        lock (this)
        {
            // Get the top item from the undo stack
            var x = undoStack;
            if (x == null) return;

            // Remove the top item from the undo stack
            undoStack = x.next;

            // Add the item to the redo stack
            x.next = redoStack;
            redoStack = x;

            // Perform the undo action
            x.Undo?.Invoke();
        }
    }


    /// <summary>
    /// Undo the top item on the redo stack.
    /// </summary>
    public void Redo()
    {
        lock (this)
        {
            // Get the top item from the redo stack
            var x = redoStack;
            if (x == null) return;

            // Remove the top item from the redo stack
            redoStack = x.next;

            // Add the item to the redo stack
            x.next = undoStack;
            undoStack = x;

            // Perform the Redo action
            x.Redo?.Invoke();
        }
    }
}
