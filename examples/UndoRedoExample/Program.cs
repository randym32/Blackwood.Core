using Blackwood;

namespace UndoRedoExample;

/// <summary>
/// Demonstrates a simple undo/redo stack built with <see cref="UndoRedo"/>.
/// </summary>
class Program
{
    /// <summary>
    /// Simple numeric state we will mutate via operations.
    /// </summary>
    private static int _value = 0;
    /// <summary>
    /// Head of the undo stack (singly linked via <see cref="UndoRedo.next"/>).
    /// </summary>
    private static UndoRedo? _undoStack;
    /// <summary>
    /// Head of the redo stack.
    /// </summary>
    private static UndoRedo? _redoStack;

    /// <summary>
    /// Entry point that performs a sequence of actions and demonstrates undo/redo.
    /// </summary>
    static void Main(string[] args)
    {
        // Console banner for readability.
        Console.WriteLine("Undo/Redo Example");
        Console.WriteLine("=================\n");

        // Show the initial value before any operations.
        Console.WriteLine($"Initial value: {_value}");

        // Perform a few operations, each recorded for undo/redo.
        PerformOperation("Add 5", () => _value += 5, () => _value -= 5);
        PerformOperation("Multiply by 2", () => _value *= 2, () => _value /= 2);
        PerformOperation("Add 10", () => _value += 10, () => _value -= 10);

        Console.WriteLine($"\nCurrent value: {_value}");

        // Undo operations in LIFO order.
        Console.WriteLine("\nUndoing operations:");
        Undo();
        Undo();
        Undo();

        Console.WriteLine($"\nCurrent value after undo: {_value}");

        // Redo operations in LIFO order.
        Console.WriteLine("\nRedoing operations:");
        Redo();
        Redo();
        Redo();

        Console.WriteLine($"\nCurrent value after redo: {_value}");
    }

    /// <summary>
    /// Performs an operation and records it on the undo stack.
    /// </summary>
    /// <param name="description">Human-friendly description of the operation.</param>
    /// <param name="doAction">Action that applies the operation.</param>
    /// <param name="undoAction">Action that reverts the operation.</param>
    static void PerformOperation(string description, Action doAction, Action undoAction)
    {
        // Apply the operation.
        doAction();
        var undoRedo = new UndoRedo(description, undoAction, doAction)
        {
            next = _undoStack
        };
        // Push onto the undo stack.
        _undoStack = undoRedo;
        // New operation invalidates the redo stack.
        _redoStack = null;
        Console.WriteLine($"  {description}: value = {_value}");
    }

    /// <summary>
    /// Undoes the most recent operation if available.
    /// </summary>
    static void Undo()
    {
        if (_undoStack == null)
        {
            Console.WriteLine("  Nothing to undo");
            return;
        }
        // Run the stored undo action.
        _undoStack.Undo();
        Console.WriteLine($"  Undo: {_undoStack.Description} - value = {_value}");

        // Move the node from undo stack to redo stack.
        var next = _undoStack.next;
        _undoStack.next = _redoStack;
        _redoStack = _undoStack;
        _undoStack = next;
    }

    /// <summary>
    /// Redoes the most recently undone operation if available.
    /// </summary>
    static void Redo()
    {
        if (_redoStack == null)
        {
            Console.WriteLine("  Nothing to redo");
            return;
        }
        // Run the stored redo action.
        _redoStack.Redo();
        Console.WriteLine($"  Redo: {_redoStack.Description} - value = {_value}");

        // Move the node from redo stack back to undo stack.
        var next = _redoStack.next;
        _redoStack.next = _undoStack;
        _undoStack = _redoStack;
        _redoStack = next;
    }
}

