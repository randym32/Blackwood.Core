using System.ComponentModel;
using Blackwood;

namespace PreferencesExample;

/// <summary>
/// Example application settings that can be managed through the preferences system.
/// </summary>
public static class AppSettings
{
    // Each property uses attributes so the preferences proxy can discover metadata.
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

    [Description("Window width in pixels")]
    [DefaultValue(1024)]
    [Category("Window")]
    [DisplayName("Window Width")]
    public static int WindowWidth { get; set; } = 1024;

    [Description("Window height in pixels")]
    [DefaultValue(768)]
    [Category("Window")]
    [DisplayName("Window Height")]
    public static int WindowHeight { get; set; } = 768;
}

class Program
{
    static void Main(string[] args)
    {
        // Simple banner for console output
        Console.WriteLine("Preferences Example");
        Console.WriteLine("==================\n");

        // Load preferences from a JSON file if one exists
        var settingsFile = "settings.json";
        try
        {
            Application.LoadPreferences(settingsFile);
            Console.WriteLine($"Loaded preferences from {settingsFile}");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"No existing preferences file found. Using defaults.");
        }

        // Display current settings
        Console.WriteLine("\nCurrent Settings:");
        Console.WriteLine($"  Theme: {AppSettings.Theme}");
        Console.WriteLine($"  Max Recent Files: {AppSettings.MaxRecentFiles}");
        Console.WriteLine($"  Auto Update: {AppSettings.AutoUpdate}");
        Console.WriteLine($"  Window Size: {AppSettings.WindowWidth}x{AppSettings.WindowHeight}");

        // Modify settings as if a user changed them in a UI
        Console.WriteLine("\nModifying settings...");
        AppSettings.Theme = "Dark";
        AppSettings.MaxRecentFiles = 15;
        AppSettings.WindowWidth = 1280;
        AppSettings.WindowHeight = 960;

        Console.WriteLine("\nUpdated Settings:");
        Console.WriteLine($"  Theme: {AppSettings.Theme}");
        Console.WriteLine($"  Max Recent Files: {AppSettings.MaxRecentFiles}");
        Console.WriteLine($"  Window Size: {AppSettings.WindowWidth}x{AppSettings.WindowHeight}");

        // Persist the changes
        Application.SavePreferences(settingsFile);
        Console.WriteLine($"\nSaved preferences to {settingsFile}");

        // Obtain the proxy that exposes the static preference properties
        var preferences = Application.PreferencesProxy();

        // Demonstrate checkpoint and restore workflow (useful for preference dialogs)
        Console.WriteLine("\nDemonstrating checkpoint/restore...");
        var checkpoint = preferences.Checkpoint();
        Console.WriteLine("Checkpoint created");

        AppSettings.Theme = "High Contrast";
        AppSettings.MaxRecentFiles = 20;
        Console.WriteLine($"After changes - Theme: {AppSettings.Theme}, Max Recent Files: {AppSettings.MaxRecentFiles}");

        Console.WriteLine("Restoring checkpoint...");
        preferences.RestoreCheckpoint(checkpoint);
        Console.WriteLine("Checkpoint restored");
        Console.WriteLine($"After restore - Theme: {AppSettings.Theme}, Max Recent Files: {AppSettings.MaxRecentFiles}");

        // Reset all preferences to their attribute-defined defaults
        Console.WriteLine("\nResetting to defaults...");
        Console.WriteLine("Resetting to defaults...");
        preferences.ResetToDefaults();
        Console.WriteLine("\nSettings after reset:");
        Console.WriteLine($"  Theme: {AppSettings.Theme}");
        Console.WriteLine($"  Max Recent Files: {AppSettings.MaxRecentFiles}");
        Console.WriteLine($"  Auto Update: {AppSettings.AutoUpdate}");
        Console.WriteLine($"  Window Size: {AppSettings.WindowWidth}x{AppSettings.WindowHeight}");
    }
}

