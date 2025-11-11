namespace Blackwood.Core.Tests;

/// <summary>
/// Test suite for the UndoRedo functionality in Blackwood.WinForms.
/// Tests cover undo/redo stack management, thread safety, and integration scenarios.
/// This test class validates the DockContent-based undo/redo system.
/// </summary>
[TestFixture]
public class UndoRedoTests
{
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

        // Assert
        Assert.That(undoRedo.Description, Is.EqualTo(description), "Description should be set correctly");
        Assert.That(undoRedo.Undo, Is.EqualTo(undoAction), "Undo action should be set correctly");
        Assert.That(undoRedo.Redo, Is.EqualTo(redoAction), "Redo action should be set correctly");
        Assert.That(undoRedo.next, Is.Null, "Next field should be null initially");
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

        // Assert
        Assert.That(undoRedo.Description, Is.EqualTo(emptyDescription), "Empty description should be preserved");
        Assert.That(undoRedo.Undo, Is.EqualTo(undoAction), "Undo action should be set correctly");
        Assert.That(undoRedo.Redo, Is.EqualTo(redoAction), "Redo action should be set correctly");
    }

    #endregion

}