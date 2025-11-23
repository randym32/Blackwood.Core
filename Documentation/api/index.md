# API Documentation

This file summarizes the namespaces and classes in this assembly.



### Blackwood Namespace

The [Blackwood namespace](xref:Blackwood) is used to contain extra helper procedures and classes.

| Component                                                    | Description                                    |
|--------------------------------------------------------------|------------------------------------------------|
| [Application](xref:Blackwood.Application)                    | A class to hold a Application name and path related  utilities.       |
| [ApplicationForwarder&lt;TMessage&gt;](xref:Blackwood.ApplicationForwarder`1) | A named-pipe helper that enforces single-instance behavior and forwards serialized messages (for example, file-open arguments) between processes. |
| [ChartAnnotation&lt;IndexType&gt;](xref:Blackwood.ChartAnnotation`1) | Represents an annotation span on a chart where index and duration share the same type. |
| [ChartAnnotation&lt;IndexType, DurationType&gt;](xref:Blackwood.ChartAnnotation`2) | Represents an annotation span on a chart with separate types for index and duration. |
| [ChartPoint&lt;IndexType&gt;](xref:Blackwood.ChartPoint`1) | Represents a single data point in a chart with an independent variable (index) and dependent variable (y-value). |
| [ItemAttributes](xref:Blackwood.ItemAttributes)              | Attributes for menu items. |
| [MenuItem](xref:Blackwood.MenuItem)                          | Represents a menu item in the menu system. |
| [MenuItemAttribute](xref:Blackwood.MenuItemAttribute)        | Attribute for marking methods as menu items. |
| [Menus](xref:Blackwood.Menus)                                | Menu system for building application menus from JSON templates. |
| [PropertiesJsonConverter](xref:Blackwood.PropertiesJsonConverter) | JSON converter for proxy properties objects. |
| [ProxyPropertiesObject](xref:Blackwood.ProxyPropertiesObject) | Property proxy for dynamic property access. |
| [ProxyPropertyDescriptor](xref:Blackwood.ProxyPropertyDescriptor) | Property descriptor for proxy properties. |
| [ReduceArray&lt;T&gt;](xref:Blackwood.ReduceArray`1)         | Implements the Douglas-Peucker algorithm to reduce point counts in large datasets while preserving visual fidelity. |
| [UndoRedo](xref:Blackwood.UndoRedo)                          | Represents an undo/redo operation. |
| [UndoRedoController](xref:Blackwood.UndoRedoController)      | Controller for managing undo/redo operations. |
| [Utils](xref:Blackwood.Utils)                                | A helper class of miscellaneous utilities. |


