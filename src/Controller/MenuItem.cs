// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Blackwood;

/// <summary>
/// This class represents a menu item, used to serialize and deserialize menu
/// items to and from a JSON file.
/// </summary>
public class MenuItem
{
    #region Menu Templates
    /// <summary>
    /// A dictionary of menu templates.
    /// </summary>
    /// <remarks>
    /// These store extra information to fill in automatically for menu items.
    ///
    /// The key is the normalized menu path, with the accelerator key marker
    /// removed and the case normalized.
    /// </remarks>
    static readonly Dictionary<string, MenuItem> Templates = [];

    /// <summary>
    /// Adds the menu templates to the template cache.
    /// </summary>
    /// <param name="menuItems">The menu item templates.</param>
    /// <remarks>
    /// The dictionary is used to store the templates for menu items.
    /// </remarks>
    public static void AddTemplates(IEnumerable<MenuItem> menuItems)
    {
        // Next go thru and build the templates
        foreach (var item in menuItems)
        {
            // Normalize the key by removing the accelerator key marker and
            // using a normlized case
            var key = item.MenuPath?.ToLower();
            if (string.IsNullOrEmpty(key)) continue;
            key = key.Replace("&", "");

            // If a template wasnt already defined
            if (!Templates.ContainsKey(key))
                Templates[key] = item;
        }
    }

    /// <summary>
    /// Gets a template item from the template cache.
    /// </summary>
    /// <param name="key">The key to look up the template item.</param>
    /// <returns>The template item, or null if not found.</returns>
    public static MenuItem? GetTemplate(string key)
    {
        // Look up a template item to fill in the extra bits automatically
        Templates.TryGetValue(key, out var template);
        return template;
    }
    #endregion


    #region Fields
    /// <summary>
    /// The menu path for the item.
    /// </summary>
    /// <remarks>
    /// The menu path is the path to the menu item.
    /// The '>' character is used to separate the menu items.
    /// </remarks>
    public string? MenuPath;

    /// <summary>
    /// The keyboard shortcut to activate the menu item.
    /// </summary>
    /// <remarks>
    /// The keyboard shortcut is a combination of the control key and the letter key.
    /// For example, the keyboard shortcut for the 'New' menu item is 'Ctrl+N'.
    /// </remarks>
    public string? QuickKey;

    /// <summary>
    /// If true, a separator will placed before the item.
    /// </summary>
    public bool SeparatorBefore;

    /// <summary>
    /// The tooltip for the item.
    /// </summary>
    /// <remarks>
    /// The tooltip is the text that appears when the mouse hovers over the menu item.
    /// </remarks>
    public string? ToolTip;

    /// <summary>
    /// An optional object that can be associated with the menu item.
    /// </summary>
    public object? Tag;

    /// <summary>
    /// If true, the menu item will be Disabled.
    /// </summary>
    public bool Disabled;
    #endregion

}

