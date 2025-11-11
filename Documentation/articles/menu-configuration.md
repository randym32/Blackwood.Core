# Menu Configuration

Blackwood.Core provides a menu configuration system for defining application
menus using JSON files. This article explains how to configure menus for your
application.

Note: the menu definitions are not connected to a menu system.  Another assembly
such as `Blackwood.WinForms` is needed for thate.  This provides the foundation
for the definition.

## Overview

The menu configuration system supports:

- Defining menu strip items (top-level menus)
- Configuring menu items with keyboard shortcuts, tooltips, and separators
- Loading menu templates from JSON files or embedded resources
- Accessing menu templates programmatically to build your UI

Menu templates are loaded from a `menuTemplates.json` file, which can be placed
in the application's customization folder or embedded as a resource in your
assembly.

## JSON File Structure

The menu configuration file uses a JSON structure with two main sections:

```json
{
  "MenuStrip": "File|Edit|View|Project|Tools|Help",
  "MenuItems": [
    {
      "MenuPath": "File>&New",
      "QuickKey": "Ctrl+N",
      "ToolTip": "Create a new file"
    }
  ]
}
```

### MenuStrip

The `MenuStrip` property defines the top-level menu items.  Menu items are
separated by the pipe character (`|`).

```json
"MenuStrip": "File|Edit|View|Project|Tools|Help"
```

Each menu strip item can include an accelerator key marker (`&`) to indicate
the keyboard shortcut letter.  For example, `&File` displays as "**F**ile"
with the F underlined.

The menu strip items are normalized (converted to lowercase and accelerator
markers removed) for internal storage. If a menu strip item is not already in
the default template, it will be added automatically.

### MenuItems

The `MenuItems` property is an array of menu item objects.  Each menu item
defines a menu path and optional properties.

## Menu Item Properties

Each menu item in the `MenuItems` array can have the following properties:

### MenuPath (Required)

The `MenuPath` property defines the hierarchical path to the menu item.  Use the
`>` character to separate menu levels.

```json
"MenuPath": "File>&New"
"MenuPath": "File>Recent &Files"
"MenuPath": "Edit>Cu&t"
```

The menu path can include accelerator key markers (`&`) to indicate keyboard
shortcuts.  The accelerator marker should be placed before the letter that will
be underlined.

**Examples:**
- `"File>&New"` - Creates a "New" item under "File" with "N" as the accelerator
- `"File>Recent &Files"` - Creates a "Recent Files" item with "F" as the accelerator
- `"Edit>Cu&t"` - Creates a "Cut" item with "t" as the accelerator

### QuickKey (Optional)

The `QuickKey` property defines the keyboard shortcut for the menu item.

```json
"QuickKey": "Ctrl+N"
"QuickKey": "Ctrl+Shift+P"
"QuickKey": "Alt+F4"
"QuickKey": "Delete"
```

**Common keyboard shortcut formats:**
- `Ctrl+Key` - Control key combination (e.g., `Ctrl+N`, `Ctrl+S`)
- `Ctrl+Shift+Key` - Control and Shift combination (e.g., `Ctrl+Shift+P`)
- `Alt+Key` - Alt key combination (e.g., `Alt+F4`)
- `Key` - Single key (e.g., `Delete`, `F1`)

### ToolTip (Optional)

The `ToolTip` property provides help text that appears when hovering over the
menu item.

```json
"ToolTip": "Create a new file"
"ToolTip": "Open an existing file"
```

### SeparatorBefore (Optional)

The `SeparatorBefore` property, when set to `true`, adds a separator line before
the menu item.

```json
"SeparatorBefore": true
```

This is useful for grouping related menu items visually.

### Disabled (Optional)

The `Disabled` property, when set to `true`, marks the menu item as disabled.

```json
"Disabled": true
```

### Tag (Optional)

The `Tag` property allows you to associate an object with the menu item for
custom use.

```json
"Tag": "custom-data"
```

## Complete Example

Here's a complete example of a menu configuration file:

```json
{
  "MenuStrip": "File|Edit|View|Project|Tools|Help",
  "MenuItems": [
    {
      "MenuPath": "File>&New",
      "QuickKey": "Ctrl+N",
      "ToolTip": "Create a new file"
    },
    {
      "MenuPath": "File>&Open",
      "QuickKey": "Ctrl+O",
      "ToolTip": "Open an existing file"
    },
    {
      "MenuPath": "File>&Close",
      "QuickKey": "Ctrl+W",
      "SeparatorBefore": true,
      "ToolTip": "Close the current file"
    },
    {
      "MenuPath": "File>Recent Projects",
      "QuickKey": "Ctrl+Shift+P",
      "ToolTip": "Open a recently used project"
    },
    {
      "MenuPath": "File>Recent &Files",
      "QuickKey": "Ctrl+Shift+O",
      "ToolTip": "Open a recently used file"
    },
    {
      "MenuPath": "File>Save",
      "QuickKey": "Ctrl+S",
      "SeparatorBefore": true,
      "ToolTip": "Save the current file"
    },
    {
      "MenuPath": "File>Preferences",
      "ToolTip": "Open preferences configuration",
      "SeparatorBefore": true
    },
    {
      "MenuPath": "File>E&xit",
      "QuickKey": "Alt+F4",
      "SeparatorBefore": true,
      "ToolTip": "Exit the application"
    },
    {
      "MenuPath": "Edit>&Undo",
      "QuickKey": "Ctrl+Z",
      "ToolTip": "Undo the last action"
    },
    {
      "MenuPath": "Edit>&Redo",
      "QuickKey": "Ctrl+Y",
      "ToolTip": "Redo the last undone action"
    },
    {
      "MenuPath": "Edit>Cu&t",
      "QuickKey": "Ctrl+X",
      "ToolTip": "Cut the selected item to clipboard",
      "SeparatorBefore": true
    },
    {
      "MenuPath": "Edit>&Copy",
      "QuickKey": "Ctrl+C",
      "ToolTip": "Copy the selected item to clipboard"
    },
    {
      "MenuPath": "Edit>&Paste",
      "QuickKey": "Ctrl+V",
      "ToolTip": "Paste from clipboard"
    },
    {
      "MenuPath": "Edit>&Delete",
      "QuickKey": "Delete",
      "ToolTip": "Delete the selected item"
    },
    {
      "MenuPath": "Edit>Select &All",
      "QuickKey": "Ctrl+A",
      "SeparatorBefore": true,
      "ToolTip": "Select all items"
    },
    {
      "MenuPath": "Project>&Add",
      "QuickKey": "Ctrl+Shift+A",
      "ToolTip": "Add a new item to the project"
    },
    {
      "MenuPath": "Project>&Remove",
      "QuickKey": "Ctrl+R",
      "ToolTip": "Remove the selected item from the project"
    },
    {
      "MenuPath": "Project>&Properties",
      "QuickKey": "Alt+Enter",
      "ToolTip": "Open project properties"
    },
    {
      "MenuPath": "Tools>&Options",
      "ToolTip": "Open tool options configuration"
    },
    {
      "MenuPath": "Help>&View Help",
      "QuickKey": "Ctrl+F1",
      "ToolTip": "View help documentation"
    },
    {
      "MenuPath": "Help>&About",
      "ToolTip": "Show information about the application"
    }
  ]
}
```

## Loading Menu Templates

Menu templates are loaded automatically when you call `Menus.LoadMenuTemplates()`:

```csharp
using Blackwood;

// Load menu templates for the current application
Menus.LoadMenuTemplates();

// Or load for a specific application name
Menus.LoadMenuTemplates("MyApplication");
```

The system searches for `menuTemplates.json` in the following locations, in order:

1. **User customization folder**: `{CommonApplicationDataPath}/{ApplicationName}/menuTemplates.json`
2. **Embedded resources**: Searches all loaded assemblies for an embedded
   resource named `menuTemplates.json`

Users can customize menus by placing a file in their application data folder.
Default menus are provided as embedded resources.

## Accessing Menu Templates

After loading menu templates, you can access them programmatically:

```csharp
using Blackwood;

// Load menu templates first
Menus.LoadMenuTemplates();

// Get a menu item template using the normalized menu path
var template = MenuItem.GetTemplate("file>new");

if (template != null)
{
    Console.WriteLine($"Menu Path: {template.MenuPath}");
    Console.WriteLine($"Quick Key: {template.QuickKey}");
    Console.WriteLine($"Tool Tip: {template.ToolTip}");
    Console.WriteLine($"Separator Before: {template.SeparatorBefore}");
    Console.WriteLine($"Disabled: {template.Disabled}");
}
```

**Important:** Use the normalized menu path when looking up templates:
- Convert to lowercase
- Remove accelerator key markers (`&`)
- Use `>` to separate menu levels

For example:
- `"File>&New"` becomes `"file>new"`
- `"Edit>Cu&t"` becomes `"edit>cut"`
- `"File>Recent &Files"` becomes `"file>recent files"`

## Menu Strip Template

The menu strip template dictionary stores the display names for top-level
menu items:

```csharp
using Blackwood;

// Access the menu strip template dictionary
foreach (var kvp in Menus.MenuStripTemplate)
{
    Console.WriteLine($"Key: {kvp.Key}, Display Name: {kvp.Value}");
}
```

The dictionary uses normalized keys (lowercase, no accelerator markers) and
stores the display names with accelerator markers preserved.

## Customization

### User Customization

Users can customize menus by creating a `menuTemplates.json` file in their
application data folder:

**Windows:**
```
%APPDATA%\MyApplication\menuTemplates.json
```

**macOS/Linux:**
```
~/.config/MyApplication/menuTemplates.json
```

This file loads before embedded resources, enabling users to override default
menu configurations.

### Embedded Resources

To include menu templates as embedded resources in your assembly:

1. Add `menuTemplates.json` to your project
2. Set the file's build action to "Embedded Resource"
3. The file will be automatically discovered when `LoadMenuTemplates()` is called

## Best Practices

1. **Use consistent naming**: Use consistent menu path naming conventions throughout your application.

2. **Standard keyboard shortcuts**: Follow platform conventions for keyboard shortcuts (e.g., `Ctrl+N` for New, `Ctrl+S` for Save).

3. **Accelerator keys**: Use accelerator keys (`&`) for keyboard navigation. Choose letters that don't conflict with other menu items.

4. **Separators**: Use separators (`SeparatorBefore`) to group related menu items logically.

5. **Tooltips**: Provide descriptive tooltips for all menu items.

6. **Menu organization**: Organize menus hierarchically with clear parent-child relationships.

7. **Normalization**: Remember that menu paths are normalized for template lookup (lowercase, no accelerator markers).

## Next Steps

- Read the [Introduction](intro.md) for an overview of Blackwood.Core
- Check the [API Reference](../api/) for complete API documentation

