using Blackwood;

namespace UndoRedoExample;

/// <summary>
/// Demonstrates undo/redo functionality using the <see cref="UndoRedoController"/> class.
/// </summary>
class Program
{
    /// <summary>
    /// Entry point that demonstrates UndoRedoController.
    /// </summary>
    static void Main(string[] args)
    {
        Console.WriteLine("Undo/Redo Example");
        Console.WriteLine("==================\n");

        DemoUndoRedoController();
    }

    /// <summary>
    /// Demonstrates using UndoRedoController for undo/redo management.
    /// </summary>
    static void DemoUndoRedoController()
    {
        // Create a controller to manage undo/redo operations
        var controller = new UndoRedoController();
        int value = 0;

        Console.WriteLine($"Initial value: {value}");

        // Perform operations using the controller
        void PerformOperationWithController(string description, Action doAction, Action undoAction)
        {
            // Apply the operation
            doAction();
            // Record the undo/redo entry
            controller.AppendUndo(new UndoRedo(description, undoAction, doAction));
            Console.WriteLine($"  {description}: value = {value}");
        }

        // Perform a few operations
        PerformOperationWithController("Add 5", () => value += 5, () => value -= 5);
        PerformOperationWithController("Multiply by 2", () => value *= 2, () => value /= 2);
        PerformOperationWithController("Add 10", () => value += 10, () => value -= 10);

        Console.WriteLine($"\nCurrent value: {value}");

        // Check what can be undone/redone
        Console.WriteLine($"\nCan undo: {controller.UndoItem != null} ({controller.UndoItem?.Description ?? "N/A"})");
        Console.WriteLine($"Can redo: {controller.RedoItem != null} ({controller.RedoItem?.Description ?? "N/A"})");

        // Undo operations
        Console.WriteLine("\nUndoing operations:");
        while (controller.UndoItem != null)
        {
            var description = controller.UndoItem.Description;
            controller.Undo();
            Console.WriteLine($"  Undo: {description} - value = {value}");
        }

        Console.WriteLine($"\nCurrent value after undo: {value}");

        // Check what can be undone/redone
        Console.WriteLine($"\nCan undo: {controller.UndoItem != null} ({controller.UndoItem?.Description ?? "N/A"})");
        Console.WriteLine($"Can redo: {controller.RedoItem != null} ({controller.RedoItem?.Description ?? "N/A"})");

        // Redo operations
        Console.WriteLine("\nRedoing operations:");
        while (controller.RedoItem != null)
        {
            var description = controller.RedoItem.Description;
            controller.Redo();
            Console.WriteLine($"  Redo: {description} - value = {value}");
        }

        Console.WriteLine($"\nCurrent value after redo: {value}");

        // Demonstrate that new operations clear the redo stack
        Console.WriteLine("\nDemonstrating redo stack clearing:");
        Console.WriteLine("Undoing one operation to create a redo stack...");
        if (controller.UndoItem != null)
        {
            controller.Undo();
            Console.WriteLine($"Can redo: {controller.RedoItem != null} ({controller.RedoItem?.Description ?? "N/A"})");
            Console.WriteLine("Adding new operation (this clears the redo stack)...");
            PerformOperationWithController("Subtract 3", () => value -= 3, () => value += 3);
            Console.WriteLine($"Can redo: {controller.RedoItem != null} (should be false - redo stack was cleared)");
        }
    }
}

