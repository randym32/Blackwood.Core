# Undo / Redo

Blackwood.Core provides lightweight undo / redo helpers that are shared across UI layers. The primary abstractions are:

- `UndoRedo` — encapsulates a single undoable action
- `UndoRedoController` — manages stacks of `UndoRedo` items and exposes commands
  that controllers or panels can call
- Higher-level integrations within UI controllers (for example,
  `PropertiesPanel` and `PreferencesPanel` in Blackwood.WinForms) that create
   and execute undo/redo entries when state changes

## UndoRedo basics

The `UndoRedo` record stores:

- `Description` — user-facing text for menu entries or history logs
- `Undo` — delegate that reverts the change
- `Redo` — delegate that reapplies the change
- `next` — link used by the stack implementation

A minimal usage example:

```csharp
using Blackwood.WinForms;

// Initial value
int value = 0;

// Create an UndoRedo entry with description, undo, and redo actions.
UndoRedo item = new UndoRedo(
    description: "Increment value", // A user-visible string for UI/history purposes
    undo: () => value--,  // Undo: decrement to revert "increment"
    redo: () => value++   // Redo: increment to reapply the change
);

item.Redo(); // value becomes 1
item.Undo(); // value returns to 0
```

For multiple operations, store them in an `UndoRedoController` and wire UI
commands (menu items, keyboard shortcuts, toolbar buttons) to call `Undo()` or
`Redo()`.

## Integrating with PropertyGrid

`PropertiesPanel` hooks `PropertyGrid.PropertyValueChanged` in the WinForms
layer and records undo / redo entries automatically:

```csharp
// Listen for changes in the PropertyGrid's property values.
propertyGrid.PropertyValueChanged += (sender, e) =>
{
    // Try to retrieve the property descriptor for the changed property.
    var descriptor = e.ChangedItem?.PropertyDescriptor;
    // If there's no descriptor, abort (cannot record undo for unknown property).
    if (descriptor == null)
        return;

    // Store the previous value (before the change).
    var oldValue = e.OldValue;
    // Store the new/current value (after the change).
    var newValue = e.ChangedItem.Value;

    // Log an undo/redo entry with a description for the specific property edited.
    AppendUndo(new UndoRedo(
        $"Change to {e.ChangedItem.Label}",                // Description for UI/history display
        undo: () => descriptor.SetValue(null, oldValue),   // Undo action reverts to old value
        redo: () => descriptor.SetValue(null, newValue))); // Redo action reapplies the change
};
```

This pattern logs intent ("Change to …") and ensures redo is applied after the
undo stack captures the current state. `AppendUndo` pushes the item and clears
the redo stack, matching typical UX expectations.


## Menu integration

You can expose undo / redo commands in the menu via JSON templates:

```json
{
  "MenuStrip": "Edit",
  "MenuItems": [
    { "MenuPath": "Edit>&Undo", "QuickKey": "Ctrl+Z", "ToolTip": "Undo last action" },
    { "MenuPath": "Edit>&Redo", "QuickKey": "Ctrl+Y", "ToolTip": "Redo last action" }
  ]
}
```

Hook the `Click` event of the generated `ToolStripMenuItem` to the undo / redo
handlers on your panel or main controller.


## Tips and best practices

- Capture meaningful `Description` text for command history / tooltips.
- When chaining operations (e.g., property changes within the same dialog),
  batch them by wrapping the sequence in a checkpoint.
- Clear the redo stack whenever a new operation is executed to avoid branching histories.
- Run unit tests similar to `UndoRedoExample` in the examples directory to
  verify complex workflows.

## Further reading

- `UndoRedoExample` project in the `examples` folder of Blackwood.Core — a console-based illustration of the API.

