// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text.Json;
namespace Blackwood;

public static partial class Application
{
    /// <summary>
    /// The object that provides the properties to the PropertyGrid.
    /// </summary>
    static ProxyPropertiesObject? preferencesObject_;

    /// <summary>
    /// Loads the Application static properties marked as preferences.
    /// </summary>
    /// <returns>The ProxyPropertiesObject that provides the properties to the PropertyGrid.</returns>
    public static ProxyPropertiesObject PreferencesProxy()
    {
        if (null != preferencesObject_)
            return preferencesObject_;

        preferencesObject_ = new ProxyPropertiesObject();

        // Get all assemblies in the current application domain
        // Ensure all referenced assemblies are loaded to avoid missing types
        foreach (var assemblyName in AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(a => a.GetReferencedAssemblies()).Distinct())
        {
            try { AppDomain.CurrentDomain.Load(assemblyName); } catch { }
        }
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            // Skip dynamic/generated .NET assemblies (like 'System', 'Microsoft', etc)
            var name = assembly.GetName().Name;
            if (string.IsNullOrEmpty(name) ||
                name.StartsWith("System"   , StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("mscorlib" , StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                // Get all types in the assembly
                foreach (var type in assembly.GetTypes())
                {
                    // Get all static properties for the type
                    preferencesObject_.AddClassProperties(type);
                }
            }
            catch
            {
                // Skip assemblies that can't be loaded
            }
        }

        return preferencesObject_;
    }

    /// <summary>
    /// The standard JSON serializer options with the PropertiesJsonConverter.
    /// </summary>
    static readonly JsonSerializerOptions standardJsonSerializerOptions_ =
     new(JSONDeserializer.JSONOptions)
     {
         Converters = { new PropertiesJsonConverter() }
     };

    /// <summary>
    /// Serializes the current preferences in the background and saves them to a file.
    /// </summary>
    /// <param name="filePath">The path to the file to save the preferences to.</param>
    /// <remarks>
    /// The preferences are saved to a file in the JSON format.
    /// </remarks>
    public static void SavePreferences(string filePath)
    {
        // Use a background thread to avoid blocking the UI
        Util.Save(filePath, (stream) =>
        {
            JsonSerializer.Serialize(stream, PreferencesProxy(), standardJsonSerializerOptions_);
        });
    }

    /// <summary>
    /// Loads the preferences from a file.
    /// </summary>
    /// <param name="filePath">The path to the file to load the preferences from.</param>
    /// <remarks>
    /// The preferences are loaded from a file in the JSON format.
    /// </remarks>
    public static void LoadPreferencesFromFile(string filePath)
    {
        // Load the preferences from the file
        try
        {
            using var stream = File.OpenRead(filePath);
            var preferences = JsonSerializer.Deserialize<Dictionary<string, object?>>(stream, standardJsonSerializerOptions_);
            if (preferences == null) return;

            // Apply the deserialized values to the properties.
            // This will convert the values to the correct types.
            PreferencesProxy().ApplyPropertyValues(preferences);
        }
        catch (JsonException)
        {
            // Re-throw JsonException to allow callers to handle invalid JSON
            throw;
        }
        catch (FileNotFoundException)
        {
            // Re-throw FileNotFoundException to allow callers to handle missing files
            throw;
        }
        catch
        {
            // Silently ignore other exceptions (permission errors, etc.)
        }
    }
}
