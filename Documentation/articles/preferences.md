# The Preferences System

[`PreferencesProxy`](../api/Blackwood.Core.Utils.md#Blackwood_System_Utils_PreferencesProxy)
provides a proxy for application preferences. It automatically discovers
static properties in your code that are meant for users to change,
eliminating the need to register them individually.


## Attributes

The preferences use the following attributes (on static properties):

- **DescriptionAttribute**: Provides a description of the property. This
  attribute is **required** and marks properties to be exposed.
- **DefaultValueAttribute**: Specifies the default value of the property.
  This is used when resetting preferences to their initial state.
  *Recommended*
- **CategoryAttribute**: Defines the category of the property, used by
  interfaces to group related properties. *Recommended*
- **DisplayNameAttribute**: Provides a custom display name for the property
  in the preferences interface. *Optional*

## How It Works

Typically, your application will use static properties to store configuration
values, defaults, and user preferences.

To allow user editng, a UI component such as the Windows Forms PropertyGrid
is typically used to provide an interface to inspect and change these settings.
However they are designed to work with object instances and using reflection to
access their properties values, names and descriptions.

Since static properties cannot be edited directly through these tools, the
preferences system provides a wrapper.  The wrapper presents static properties
as instance properties.  This allows UI editors like PropertyGrid to display
and modify static application settings seamlessly.

How it works:

1. **Assembly Scanning**: The system automatically scans the loaded
   assemblies, collecting static properties that have a
   `DescriptionAttribute` (and are public and settable).

2. **Property Discovery**: `ProxyPropertiesObject` implements
   `ICustomTypeDescriptor`, providing the necessary hooks for the
   `PropertyGrid` to dynamically discover all of the properties
   corresponding to these static properties.

3. **Runtime Property Forwarding**: When values are changed in the
   `PropertyGrid`, `ProxyPropertiesObject` forwards gets and sets to the
   corresponding static property in your application.


`ProxyPropertiesObject` represents each of the static properties as a
property using `ProxyPropertyDescriptor`. The PropertyGrid is able to display
and edit these virtual properties, and the `ProxyPropertyDescriptor` forwards
the gets and sets to the corresponding static property.

## Example Usage

Here's how you can define preferences in your application:

```csharp
public static class ApplicationSettings
{
    [Description("The theme used for the application interface")]
    [DefaultValue("Light")]
    [Category("Appearance")]
    [DisplayName("Application Theme")]
    public static string Theme { get; set; } = "Light";

    [Description("Maximum number of recent files to display")]
    [DefaultValue(10)]
    [Category("General")]
    [DisplayName("Recent Files Count")]
    public static int MaxRecentFiles { get; set; } = 10;

    [Description("Enable automatic updates")]
    [DefaultValue(true)]
    [Category("Updates")]
    [DisplayName("Auto Update")]
    public static bool AutoUpdate { get; set; } = true;
}
```

## Working with Preferences in Your Application

### Restoring the preferences (at startup)

At program startup -- before creating any objects, forms, etc. -- you should
load the preferences. This is done by calling
[`LoadPreferences()`](xref:Blackwood.Core.PreferencesProxy.LoadPreferences):

```csharp
// Load preferences from a file
Application.PreferencesProxy().LoadPreferences("settings.json");
```

The preferences will be loaded from the specified file and applied to your
static properties. The preferences should be loaded early, since some
configuration can only take effect at startup/object creation time.

### Saving the values

You should save the user's preferences after changes (or before exiting the
application), by calling the
[`SavePreferences()`](xref:Blackwood.Core.PreferencesProxy.SavePreferences)
method.


```csharp
// Save current preferences to a file
Application.PreferencesProxy().SavePreferences("settings.json");
```

### Resetting and Reverting Preferences

The preferences system also provides methods to help manage user
changes to preferences at runtime:

- [**ResetToDefaults()**](xref:Blackwood.Core.PreferencesProxy.ResetToDefaults) &mdash; Resets all preference values back to their
  original default values (as indicated by the `[DefaultValue]` attribute on
  each property).
- [**Checkpoint()**](xref:Blackwood.Core.PreferencesProxy.Checkpoint) &mdash; Saves the current set of values as a checkpoint.
  You can later revert changes by restoring the checkpoint.
- [**RestoreCheckpoint()**](xref:Blackwood.Core.PreferencesProxy.RestoreCheckpoint) &mdash; Restores all preference values from a
  previously saved checkpoint.

Here’s how you use these methods:

```csharp
// Obtain the PreferencesProxy instance
var preferences = Application.PreferencesProxy();

// Change a preference value
ApplicationSettings.Theme = "Dark";

// Save a checkpoint (e.g., before making batch changes or opening a preferences dialog)
var checkpoint = preferences.Checkpoint();

// Change some settings
ApplicationSettings.Theme = "Light";
ApplicationSettings.MaxRecentFiles = 20;

// User cancels changes -- restore last checkpoint
preferences.RestoreCheckpoint(checkpoint);

// At this point, the preference values will be restored to what they were at the checkpoint:
string theme = ApplicationSettings.Theme; // Will be "Dark"
int recentCount = ApplicationSettings.MaxRecentFiles; // Whatever value it had originally

// If you want to reset all preferences to default values:
preferences.ResetToDefaults();

// All preferences will now be set to their default states ("Light", 10, true, etc.)
```

These methods are especially useful for providing "Apply," "Cancel," and
"Reset to Defaults" features in a preferences dialog:

- Call [`Checkpoint()`](xref:Blackwood.Core.PreferencesProxy.Checkpoint) just before launching the dialog.
- If the user clicks "OK" or "Apply," let changes stand and optionally
  save them via [`SavePreferences()`](xref:Blackwood.Core.PreferencesProxy.SavePreferences).
- If the user clicks "Cancel," call [`RestoreCheckpoint()`](xref:Blackwood.Core.PreferencesProxy.RestoreCheckpoint) to revert any edits.
- If the user clicks "Reset to Defaults," call [`ResetToDefaults()`](xref:Blackwood.Core.PreferencesProxy.ResetToDefaults).


## A Larger Example

Here's an example showing how to set up preferences, save them,
and load them:

```csharp
// Example preferences
public static class ApplicationSettings
{
    [Description("The theme used for the application interface")]
    [DefaultValue("Light")]
    [Category("Appearance")]
    [DisplayName("Application Theme")]
    public static string Theme { get; set; } = "Light";

    [Description("Maximum number of recent files to display")]
    [DefaultValue(10)]
    [Category("General")]
    [DisplayName("Recent Files Count")]
    public static int MaxRecentFiles { get; set; } = 10;

    [Description("Enable automatic updates")]
    [DefaultValue(true)]
    [Category("Updates")]
    [DisplayName("Auto Update")]
    public static bool AutoUpdate { get; set; } = true;
}

// In your application startup:
public void InitializeApplication()
{
    // Load preferences from file (if it exists)
    Application.PreferencesProxy().LoadPreferences("app_settings.json");

    // Your application will now use the loaded values
    Console.WriteLine($"Theme: {ApplicationSettings.Theme}");
    Console.WriteLine($"Max Recent Files: {ApplicationSettings.MaxRecentFiles}");
    Console.WriteLine($"Auto Update: {ApplicationSettings.AutoUpdate}");
}

// When user changes preferences in the UI:
public void OnPreferencesChanged()
{
    // Save current preferences to file
    Application.PreferencesProxy().SavePreferences("app_settings.json");
}
```

## Additional Resources

See [PreferencesProxy documentation](../api/Blackwood.Core.PreferencesProxy.md) and [ProxyPropertiesObject documentation](../api/Blackwood.Core.ProxyPropertiesObject.md).
