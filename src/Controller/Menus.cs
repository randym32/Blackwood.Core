// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text.Json;

namespace Blackwood;

/// <summary>
/// A serializable class for menus.
/// </summary>
public class Menus
{
    #region Menu Templates
    /// <summary>
    /// A dictionary of menu item names and their preferred names and shortcut keys.
    /// </summary>
    public static readonly Dictionary<string, string> MenuStripTemplate = new()
    {
        { "file", "&File" },
        { "edit", "&Edit" },
        { "view", "&View" },
        { "tools", "&Tools" },
        { "project", "&Project" },
        { "help", "&Help" },
    };

    /// <summary>
    /// Loads the menu template from the file.
    /// </summary>
    /// <param name="stream">The stream to load from</param>
    static void LoadMenuTemplate(Stream stream)
    {
        if (null == stream)
            return;

        // Try to deserialize the menu definition
        var menus = JsonSerializer.Deserialize<Menus>(stream, JSONDeserializer.JSONOptions);
        stream.Dispose();
        if (menus == null)
            return;

        // Next go thru and build the templates
        if (!string.IsNullOrEmpty(menus.MenuStrip))
        {
            foreach (var item in menus.MenuStrip.Split('|'))
            {
                // Normalize the keys by removing the accelerator key marker and
                // using a normlized case
                var key = item.ToLower().Replace("&", "").Trim();
                if (!MenuStripTemplate.ContainsKey(key))
                    MenuStripTemplate[key] = item.Trim();
            }
        }
        if (null != menus.MenuItems)
            MenuItem.AddTemplates(menus.MenuItems);
    }


    /// <summary>
    /// Loads the menu templates from the file.
    /// </summary>
    /// <param name="applicationName">The name of the application.</param>
    /// <remarks>
    /// The menu templates are loaded from the file "menuTemplates.json" in the
    /// customization folder, and in the embedded resources.
    /// </remarks>
    public static void LoadMenuTemplates(string applicationName)
    {
        // Load the menu template
        var fileName = "menuTemplates.json";
        Stream? stream;
        try
        {
            // Allowing customization, try user specific first
            var workspacePath = Path.Combine(
                FS.CommonApplicationDataPath,
                applicationName,
                fileName
            );
            // Load the workspace state from the file
            stream = File.OpenRead(workspacePath);
            if (null != stream)
                LoadMenuTemplate(stream);
        }
        catch
        {
            // Scan assemblies back starting with the application assembly to
            // find the embedded resources
            foreach (var asm in Application.Assemblies())
                try
                {
                    var er = new EmbeddedResources(asm);
                    // Open the assemblies JSON embedded resource
                    stream = er.Stream(fileName);
                    if (null != stream)
                        LoadMenuTemplate(stream);
                }
                catch
                {
                    // Ignore and proceed to next assembly
                }
        }
    }

    /// <summary>
    /// Loads the menu templates for the application.
    /// </summary>
    static Menus()
    {
        var name = Blackwood.Application.Name;
        if (null != name)
            LoadMenuTemplates(name);
    }
    #endregion


    #region Fields
    /// <summary>
    /// The different items on the menu strip.
    /// </summary>
    public string? MenuStrip;

    /// <summary>
    /// The menu items to add to the menu strip.
    /// </summary>
    /// <remarks>
    /// The menu items are the menu items to add to the menu strip.
    /// </remarks>
    public List<MenuItem>? MenuItems;

    #endregion

}
