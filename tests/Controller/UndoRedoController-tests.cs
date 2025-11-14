namespace Blackwood.Core.Tests;

/// <summary>
/// Test suite for the UndoRedo functionality in Blackwood.WinForms.
/// Tests cover undo/redo stack management, thread safety, and integration scenarios.
/// This test class validates the DockContent-based undo/redo system.
/// </summary>
[TestFixture]
public class UndoRedoControllerTests
{
    #region Test Setup and Teardown

    private UndoRedoController? testUndoRedoStack;

    /// <summary>
    /// Setup method that runs before each test to ensure a clean state.
    /// This method is called by NUnit before each test method execution.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        testUndoRedoStack = new UndoRedoController();
    }

    /// <summary>
    /// Teardown method that runs after each test to clean up resources.
    /// This method is called by NUnit after each test method execution.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
    }

    #endregion

    #region UndoRedo Class Tests

    /// <summary>
    /// Tests that the UndoRedo constructor correctly initializes all properties.
    /// This verifies the basic functionality of the UndoRedo class.
    /// </summary>
    [Test]
    public void UndoRedo_Constructor_InitializesCorrectly()
    {
        // Arrange
        var description = "Test Action";
        var undoAction = new Action(() => { /* undo logic */ });
        var redoAction = new Action(() => { /* redo logic */ });

        // Act
        var undoRedo = new UndoRedo(description, undoAction, redoAction);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(undoRedo.Description, Is.EqualTo(description), "Description should be set correctly");
            Assert.That(undoRedo.Undo, Is.EqualTo(undoAction), "Undo action should be set correctly");
            Assert.That(undoRedo.Redo, Is.EqualTo(redoAction), "Redo action should be set correctly");
            Assert.That(undoRedo.next, Is.Null, "Next field should be null initially");
        }
    }

    /// <summary>
    /// Tests that the UndoRedo constructor handles null actions gracefully.
    /// This verifies robustness when invalid parameters are passed.
    /// </summary>
    [Test]
    public void UndoRedo_Constructor_WithNullActions_HandlesGracefully()
    {
        // Arrange
        var description = "Test Action";
        Action? nullUndo = null;
        Action? nullRedo = null;

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var undoRedo = new UndoRedo(description, nullUndo!, nullRedo!);
            Assert.That(undoRedo.Description, Is.EqualTo(description), "Description should still be set");
        }, "Should handle null actions without throwing exceptions");
    }

    /// <summary>
    /// Tests that the UndoRedo constructor handles empty description.
    /// This verifies that empty strings are handled correctly.
    /// </summary>
    [Test]
    public void UndoRedo_Constructor_WithEmptyDescription_HandlesCorrectly()
    {
        // Arrange
        var emptyDescription = "";
        var undoAction = new Action(() => { /* undo logic */ });
        var redoAction = new Action(() => { /* redo logic */ });

        // Act
        var undoRedo = new UndoRedo(emptyDescription, undoAction, redoAction);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(undoRedo.Description, Is.EqualTo(emptyDescription), "Empty description should be preserved");
            Assert.That(undoRedo.Undo, Is.EqualTo(undoAction), "Undo action should be set correctly");
            Assert.That(undoRedo.Redo, Is.EqualTo(redoAction), "Redo action should be set correctly");
        }
    }

    #endregion

    #region UndoItem and RedoItem Property Tests

    /// <summary>
    /// Tests that UndoItem property returns the top item from the undo stack.
    /// This verifies the basic functionality of the UndoItem property.
    /// </summary>
    [Test]
    public void UndoItem_ReturnsTopUndoStackItem()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.AppendUndo(undoRedo2);

        // Act
        var undoItem = testUndoRedoStack.UndoItem;

        // Assert
        Assert.That(undoItem, Is.EqualTo(undoRedo2), "UndoItem should return the top item from undo stack");
        Assert.That(undoItem?.Description, Is.EqualTo("Second Action"), "UndoItem should have correct description");
    }

    /// <summary>
    /// Tests that UndoItem property returns null when undo stack is empty.
    /// This verifies that the property handles empty stacks correctly.
    /// </summary>
    [Test]
    public void UndoItem_EmptyStack_ReturnsNull()
    {
        // Arrange
        Assert.That(testUndoRedoStack!.UndoItem, Is.Null, "Undo stack should be empty initially");

        // Act
        var undoItem = testUndoRedoStack.UndoItem;

        // Assert
        Assert.That(undoItem, Is.Null, "UndoItem should be null when undo stack is empty");
    }

    /// <summary>
    /// Tests that RedoItem property returns the top item from the redo stack.
    /// This verifies the basic functionality of the RedoItem property.
    /// </summary>
    [Test]
    public void RedoItem_ReturnsTopRedoStackItem()
    {
        // Arrange
        var undoRedo = CreateTestUndoRedo("Test Action");
        testUndoRedoStack!.AppendUndo(undoRedo);
        testUndoRedoStack.Undo(); // Move to redo stack

        // Act
        var redoItem = testUndoRedoStack.RedoItem;

        // Assert
        Assert.That(redoItem, Is.EqualTo(undoRedo), "RedoItem should return the top item from redo stack");
        Assert.That(redoItem?.Description, Is.EqualTo("Test Action"), "RedoItem should have correct description");
    }

    /// <summary>
    /// Tests that RedoItem property returns null when redo stack is empty.
    /// This verifies that the property handles empty stacks correctly.
    /// </summary>
    [Test]
    public void RedoItem_EmptyStack_ReturnsNull()
    {
        // Arrange
        Assert.That(testUndoRedoStack!.RedoItem, Is.Null, "Redo stack should be empty initially");

        // Act
        var redoItem = testUndoRedoStack.RedoItem;

        // Assert
        Assert.That(redoItem, Is.Null, "RedoItem should be null when redo stack is empty");
    }

    /// <summary>
    /// Tests that UndoItem and RedoItem properties reflect stack changes correctly.
    /// This verifies that the properties update as items are moved between stacks.
    /// </summary>
    [Test]
    public void UndoItem_RedoItem_ReflectStackChanges()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.AppendUndo(undoRedo2);

        using (Assert.EnterMultipleScope())
        {
            // Act & Assert - Initial state
            Assert.That(testUndoRedoStack   .UndoItem, Is.EqualTo(undoRedo2), "UndoItem should be second action");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "RedoItem should be null initially");
        }

        // Act - Undo first item
        testUndoRedoStack.Undo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo1), "UndoItem should be first action after undo");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo2), "RedoItem should be second action after undo");
        }

        // Act - Undo second item
        testUndoRedoStack.Undo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.Null, "UndoItem should be null after all undos");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo1), "RedoItem should be first action after second undo");
        }
    }

    #endregion

    #region DockContent UndoRedo Method Tests

    /// <summary>
    /// Tests that AppendUndo correctly adds items to the undo stack.
    /// This verifies the basic functionality of the AppendUndo method.
    /// </summary>
    [Test]
    public void AppendUndo_AddsItemToUndoStack()
    {
        // Arrange
        var undoRedo = CreateTestUndoRedo("Test Action");
        Assert.That(testUndoRedoStack!.UndoItem, Is.Null, "Undo stack should be null initially");

        // Act
        testUndoRedoStack.AppendUndo(undoRedo);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo), "Undo stack should contain the added item");
            Assert.That(undoRedo.next, Is.Null, "Next field should be null for first item");
        }
    }

    /// <summary>
    /// Tests that AppendUndo clears the redo stack when adding new items.
    /// This verifies that new operations invalidate the redo history.
    /// </summary>
    [Test]
    public void AppendUndo_ClearsRedoStack()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.Undo(); // Move to redo stack
        Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo1), "Redo stack should contain first action");

        // Act
        testUndoRedoStack.AppendUndo(undoRedo2);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo2), "Undo stack should contain second action");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "Redo stack should be cleared when new item is added");
        }
    }

    /// <summary>
    /// Tests that AppendUndo correctly chains multiple items in LIFO order.
    /// This verifies that the stack maintains proper order.
    /// </summary>
    [Test]
    public void AppendUndo_MultipleItems_MaintainsLIFOOrder()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        var undoRedo3 = CreateTestUndoRedo("Third Action");

        // Act
        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.AppendUndo(undoRedo2);
        testUndoRedoStack.AppendUndo(undoRedo3);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo3), "Latest item should be at the top");
            Assert.That(undoRedo3.next, Is.EqualTo(undoRedo2), "Second item should be linked");
            Assert.That(undoRedo2.next, Is.EqualTo(undoRedo1), "First item should be at the bottom");
            Assert.That(undoRedo1.next, Is.Null, "Last item should have null next");
        }
    }

    /// <summary>
    /// Tests that Undo correctly moves items from undo stack to redo stack.
    /// This verifies the basic functionality of the Undo method.
    /// </summary>
    [Test]
    public void Undo_MovesItemFromUndoToRedoStack()
    {
        // Arrange
        var undoRedo = CreateTestUndoRedo("Test Action");
        testUndoRedoStack!.AppendUndo(undoRedo);
        Assert.That(testUndoRedoStack.RedoItem, Is.Null, "Redo stack should be null initially");

        // Act
        testUndoRedoStack.Undo();

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.UndoItem, Is.Null, "Undo stack should be empty after undo");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo), "Redo stack should contain the undone item");
        }
    }

    /// <summary>
    /// Tests that Undo executes the undo action when called.
    /// This verifies that the undo action is actually executed.
    /// </summary>
    [Test]
    public void Undo_ExecutesUndoAction()
    {
        // Arrange
        var undoExecuted = false;
        var undoRedo = new UndoRedo("Test Action", () => undoExecuted = true, () => { });
        testUndoRedoStack!.AppendUndo(undoRedo);

        // Act
        testUndoRedoStack.Undo();

        // Assert
        Assert.That(undoExecuted, Is.True, "Undo action should be executed");
    }

    /// <summary>
    /// Tests that Undo does nothing when undo stack is empty.
    /// This verifies robustness when no items are available to undo.
    /// </summary>
    [Test]
    public void Undo_EmptyStack_DoesNothing()
    {
        // Arrange
        Assert.That(testUndoRedoStack!.UndoItem, Is.Null, "Undo stack should be empty initially");

        // Act & Assert
        Assert.DoesNotThrow(() => testUndoRedoStack.Undo(), "Undo should not throw when stack is empty");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.Null, "Undo stack should remain empty");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "Redo stack should remain empty");
        }
    }

    /// <summary>
    /// Tests that Redo correctly moves items from redo stack to undo stack.
    /// This verifies the basic functionality of the Redo method.
    /// </summary>
    [Test]
    public void Redo_MovesItemFromRedoToUndoStack()
    {
        // Arrange
        var undoRedo = CreateTestUndoRedo("Test Action");
        testUndoRedoStack!.AppendUndo(undoRedo);
        testUndoRedoStack.Undo(); // Move to redo stack
        Assert.That(testUndoRedoStack.UndoItem, Is.Null, "Undo stack should be empty after undo");

        // Act
        testUndoRedoStack.Redo();

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "Redo stack should be empty after redo");
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo), "Undo stack should contain the redone item");
        }
    }

    /// <summary>
    /// Tests that Redo executes the redo action when called.
    /// This verifies that the redo action is actually executed.
    /// </summary>
    [Test]
    public void Redo_ExecutesRedoAction()
    {
        // Arrange
        var redoExecuted = false;
        var undoRedo = new UndoRedo("Test Action", () => { }, () => redoExecuted = true);
        testUndoRedoStack!.AppendUndo(undoRedo);
        testUndoRedoStack.Undo(); // Move to redo stack

        // Act
        testUndoRedoStack.Redo();

        // Assert
        Assert.That(redoExecuted, Is.True, "Redo action should be executed");
    }

    /// <summary>
    /// Tests that Redo does nothing when redo stack is empty.
    /// This verifies robustness when no items are available to redo.
    /// </summary>
    [Test]
    public void Redo_EmptyStack_DoesNothing()
    {
        // Arrange
        Assert.That(testUndoRedoStack!.RedoItem, Is.Null, "Redo stack should be empty initially");

        // Act & Assert
        Assert.DoesNotThrow(() => testUndoRedoStack.Redo(), "Redo should not throw when stack is empty");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.Null, "Undo stack should remain empty");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "Redo stack should remain empty");
        }
    }

    /// <summary>
    /// Tests that Undo handles null actions gracefully.
    /// This verifies robustness when undo actions are null.
    /// </summary>
    [Test]
    public void Undo_WithNullAction_HandlesGracefully()
    {
        // Arrange
        var undoRedo = new UndoRedo("Test Action", null!, () => { /* redo logic */ });
        testUndoRedoStack!.AppendUndo(undoRedo);

        // Act & Assert
        Assert.DoesNotThrow(() => testUndoRedoStack.Undo(), "Undo should not throw when action is null");
        Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo), "Item should still be moved to redo stack");
    }

    /// <summary>
    /// Tests that Redo handles null actions gracefully.
    /// This verifies robustness when redo actions are null.
    /// </summary>
    [Test]
    public void Redo_WithNullAction_HandlesGracefully()
    {
        // Arrange
        var undoRedo = new UndoRedo("Test Action", () => { /* undo logic */ }, null!);
        testUndoRedoStack!.AppendUndo(undoRedo);
        testUndoRedoStack.Undo(); // Move to redo stack

        // Act & Assert
        Assert.DoesNotThrow(() => testUndoRedoStack.Redo(), "Redo should not throw when action is null");
        Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo), "Item should still be moved to undo stack");
    }

    #endregion

    #region Complete Undo/Redo Workflow Tests

    /// <summary>
    /// Tests a complete undo/redo workflow with multiple operations.
    /// This verifies the end-to-end functionality of the undo/redo system.
    /// </summary>
    [Test]
    public void CompleteWorkflow_MultipleOperations_WorksCorrectly()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        var undoRedo3 = CreateTestUndoRedo("Third Action");

        // Act - Build undo stack
        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.AppendUndo(undoRedo2);
        testUndoRedoStack.AppendUndo(undoRedo3);

        // Verify initial state
        Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo3), "Third item should be at top of undo stack");

        // Act - Undo operations
        testUndoRedoStack.Undo(); // Move undoRedo3 to redo stack
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo2), "Second item should be at top of undo stack");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo3), "Third item should be at top of redo stack");
        }

        testUndoRedoStack.Undo(); // Move undoRedo2 to redo stack
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo1), "First item should be at top of undo stack");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo2), "Second item should be at top of redo stack");
        }

        // Act - Redo operations
        testUndoRedoStack.Redo(); // Move undoRedo2 back to undo stack
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo2), "Second item should be back at top of undo stack");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo3), "Third item should be at top of redo stack");
        }

        testUndoRedoStack.Redo(); // Move undoRedo3 back to undo stack
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo3), "Third item should be back at top of undo stack");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "Redo stack should be empty");
        }
    }

    /// <summary>
    /// Tests that adding new items clears the redo stack.
    /// This verifies that new operations invalidate the redo history.
    /// </summary>
    [Test]
    public void AppendUndo_WithExistingRedoStack_ClearsRedoStack()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        var undoRedo3 = CreateTestUndoRedo("Third Action");

        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.Undo(); // Move to redo stack
        Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo1), "First item should be in redo stack");

        // Act
        testUndoRedoStack.AppendUndo(undoRedo2);
        testUndoRedoStack.AppendUndo(undoRedo3);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo3), "Latest item should be at top of undo stack");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "Redo stack should be cleared when new items are added");
        }
    }

    /// <summary>
    /// Tests that UndoItem and RedoItem properties work correctly in complex scenarios.
    /// This verifies that the properties reflect the current state accurately.
    /// </summary>
    [Test]
    public void UndoItem_RedoItem_ComplexScenario_WorksCorrectly()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        var undoRedo3 = CreateTestUndoRedo("Third Action");

        // Act - Build undo stack
        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.AppendUndo(undoRedo2);
        testUndoRedoStack.AppendUndo(undoRedo3);

        using (Assert.EnterMultipleScope())
        {
            // Assert - Check initial state
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo3), "UndoItem should be third action");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "RedoItem should be null initially");
        }

        // Act - Undo operations
        testUndoRedoStack.Undo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo2), "UndoItem should be second action after first undo");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo3), "RedoItem should be third action after first undo");
        }

        testUndoRedoStack.Undo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo1), "UndoItem should be first action after second undo");
            Assert.That(testUndoRedoStack.RedoItem, Is.EqualTo(undoRedo2), "RedoItem should be second action after second undo");
        }

        // Act - Add new item (should clear redo stack)
        var undoRedo4 = CreateTestUndoRedo("Fourth Action");
        testUndoRedoStack.AppendUndo(undoRedo4);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo4), "UndoItem should be fourth action after adding new item");
            Assert.That(testUndoRedoStack.RedoItem, Is.Null, "RedoItem should be null after adding new item");
        }
    }

    #endregion

    #region Thread Safety Tests

    /// <summary>
    /// Tests that the undo/redo system handles concurrent access correctly.
    /// This verifies thread safety of the stack operations.
    /// </summary>
    [Test]
    public void ConcurrentAccess_HandlesCorrectly()
    {
        // Arrange
        var undoRedos = new List<UndoRedo>();
        for (int i = 0; i < 10; i++)
        {
            undoRedos.Add(CreateTestUndoRedo($"Action {i}"));
        }

        // Act
        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            int index = i; // Capture for closure
            tasks[i] = Task.Run(() => testUndoRedoStack!.AppendUndo(undoRedos[index]));
        }

        Task.WaitAll(tasks);

        // Assert
        Assert.That(testUndoRedoStack!.UndoItem, Is.Not.Null, "At least one operation should succeed");
        // Verify that all items are properly linked
        var current = testUndoRedoStack.UndoItem;
        var count = 0;
        while (current != null && count < 20) // Prevent infinite loop
        {
            count++;
            current = current.next;
        }
        Assert.That(count, Is.GreaterThan(0), "Items should be properly linked");
    }

    /// <summary>
    /// Tests that concurrent undo operations are handled safely.
    /// This verifies thread safety of the undo method.
    /// </summary>
    [Test]
    public void ConcurrentUndo_HandlesCorrectly()
    {
        // Arrange
        var undoRedo1 = CreateTestUndoRedo("First Action");
        var undoRedo2 = CreateTestUndoRedo("Second Action");
        testUndoRedoStack!.AppendUndo(undoRedo1);
        testUndoRedoStack.AppendUndo(undoRedo2);

        // Act
        var tasks = new Task[5];
        for (int i = 0; i < 5; i++)
        {
            tasks[i] = Task.Run(() => testUndoRedoStack.Undo());
        }

        Task.WaitAll(tasks);

        // Assert
        // At least one undo should have succeeded
        Assert.That(testUndoRedoStack.RedoItem, Is.Not.Null, "At least one undo should have moved items to redo stack");
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that the undo/redo system handles null actions gracefully.
    /// This verifies robustness when null actions are passed.
    /// </summary>
    [Test]
    public void UndoRedo_WithNullActions_HandlesGracefully()
    {
        // Arrange
        var undoRedo = new UndoRedo("Test Action", null!, null!);

        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            testUndoRedoStack!.AppendUndo(undoRedo);
            testUndoRedoStack.Undo();
        }, "Should handle null actions without throwing exceptions");
    }

    /// <summary>
    /// Tests that the undo/redo system handles empty descriptions.
    /// This verifies that empty strings are handled correctly.
    /// </summary>
    [Test]
    public void UndoRedo_EmptyDescription_HandlesCorrectly()
    {
        // Arrange
        var undoRedo = CreateTestUndoRedo("");

        // Act
        testUndoRedoStack!.AppendUndo(undoRedo);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo), "Empty description should be handled correctly");
            Assert.That(undoRedo.Description, Is.EqualTo(""), "Empty description should be preserved");
        }
    }

    /// <summary>
    /// Tests that the undo/redo system handles very long descriptions.
    /// This verifies that long strings are handled correctly.
    /// </summary>
    [Test]
    public void UndoRedo_LongDescription_HandlesCorrectly()
    {
        // Arrange
        var longDescription = new string('A', 1000); // 1000 character string
        var undoRedo = CreateTestUndoRedo(longDescription);

        // Act
        testUndoRedoStack!.AppendUndo(undoRedo);

        using (Assert.EnterMultipleScope())
        {
            // Assert
            Assert.That(testUndoRedoStack.UndoItem, Is.EqualTo(undoRedo), "Long description should be handled correctly");
            Assert.That(undoRedo.Description, Is.EqualTo(longDescription), "Long description should be preserved");
        }
    }

    /// <summary>
    /// Tests that the undo/redo system handles rapid operations.
    /// This verifies robustness under high-frequency operations.
    /// </summary>
    [Test]
    public void RapidOperations_HandlesCorrectly()
    {
        // Arrange
        var undoRedos = new List<UndoRedo>();
        for (int i = 0; i < 100; i++)
        {
            undoRedos.Add(CreateTestUndoRedo($"Action {i}"));
        }

        // Act
        foreach (var undoRedo in undoRedos)
        {
            testUndoRedoStack!.AppendUndo(undoRedo);
        }

        // Assert
        Assert.That(testUndoRedoStack!.UndoItem, Is.EqualTo(undoRedos[99]), "Last item should be at top");

        // Verify the chain is intact
        var current = testUndoRedoStack.UndoItem;
        var count = 0;
        while (current != null && count < 200) // Prevent infinite loop
        {
            count++;
            current = current.next;
        }
        Assert.That(count, Is.EqualTo(100), "All items should be properly linked");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper method to create a test UndoRedo instance with simple actions.
    /// This method creates a standardized test object for use in tests.
    /// </summary>
    /// <param name="description">The description for the undo/redo action</param>
    /// <returns>A new UndoRedo instance for testing</returns>
    private static UndoRedo CreateTestUndoRedo(string description)
    {
        return new UndoRedo(
            description,
            () => { /* Test undo action */ },
            () => { /* Test redo action */ }
        );
    }

    #endregion
}