# Properties Wrapper

This guide explains how to create a property proxy for an object. A property proxy
is used to allow tools like `PropertyGrid` to modify a specific object while
restricting which properties (and fields) can be changed. This is useful when
you want to expose only a subset of an object's properties for editing.

**When to Use a Proxy**. Property proxies are intended for scenarios where you
need UI controlled access to an object's properties, such as in property editors
or when integrating with undo/redo systems. For normal application code, direct
property access remains clearer and more efficient.

## Creating a Property Proxy

`ProxyPropertiesObject` allows property access to be intercepted or wrapped for
a given target object. This class allows you to construct a proxy that exposes
only a defined set of properties or fields, and can forward property changes to
the underlying object, useful for editor scenarios, UI property grids, or when
you want fine-grained control over which properties are exposed or editable.

Here's a basic example of using `ProxyPropertiesObject` to wrap an existing
object and expose selected properties:

```csharp
// Suppose you have some target object:
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
}

// You can expose just the Name and Age properties through a proxy:
var person = new Person { Name = "Alex", Age = 30, IsActive = true };

// Specify which properties will be exposed via the proxy.
var proxy = new ProxyPropertiesObject(person, new[] { "Name", "Age" });

// You can bind this proxy to a PropertyGrid or access properties dynamically:
var name = proxy["Name"]; // get: returns "Alex"
proxy["Age"] = 31;        // set: updates the original person's Age

// The IsActive property is not visible/editable via the proxy.
```

This pattern allows you to define exactly which properties are available for
editing and can be helpful for UI data binding, validation, or restricting
changes. `ProxyPropertiesObject` may also be extended with validation and
undo/redo mechanisms as needed by your workflow.


## Adding Validation

You can extend the proxy to validate values before updating. This allows you to
reject invalid values early and keep validation logic separate from the main
object's core responsibilities.


## Integrating With Undo and Redo

When using property proxies with undo/redo functionality, use an `UndoRedoController`
to manage the undo/redo stack. Capture the old value before applying the new value,
then record an `UndoRedo` entry using `AppendUndo`. This ensures every change can
be undone and redone.

Here's an example of updating a property through a proxy with undo/redo support:

```csharp
// Create a controller to manage undo/redo operations
var undoController = new UndoRedoController();

// Set up the proxy and target object
var person = new Person { Name = "Alex", Age = 30 };
var proxy = new ProxyPropertiesObject(person, new[] { "Name", "Age" });

// Update a property with undo/redo tracking
void UpdatePropertyWithUndo(string propertyName, object newValue)
{
    // Capture the old value before making changes
    var oldValue = proxy[propertyName];

    // Apply the new value
    proxy[propertyName] = newValue;

    // Record the undo/redo entry
    undoController.AppendUndo(new UndoRedo(
        description: $"Change {propertyName}",
        undo: () => proxy[propertyName] = oldValue,
        redo: () => proxy[propertyName] = newValue));
}

// Use the helper to update properties
UpdatePropertyWithUndo("Age", 31);  // Age changes from 30 to 31
UpdatePropertyWithUndo("Name", "Bob");  // Name changes from "Alex" to "Bob"

// Undo the last change
undoController.Undo();  // Name reverts to "Alex"

// Redo the change
undoController.Redo();  // Name changes back to "Bob"

// Undo both changes
undoController.Undo();  // Name reverts to "Alex"
undoController.Undo();  // Age reverts to 30
```

The `UndoRedoController` provides `Undo()` and `Redo()` methods to navigate through
the change history, and `UndoItem` and `RedoItem` properties to check what operations
are available. You can wire these methods to menu items or keyboard shortcuts in your UI.
