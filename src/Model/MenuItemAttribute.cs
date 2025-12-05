namespace Blackwood;

/// <summary>
/// Attribute used to describe a menu item contribution.
/// </summary>
/// <remarks>
/// Note: Attribute derived classes are not supported by the System.Text.Json
/// serializer.  Use the MenuItem class instead for items that need to be
/// serialized and deserialized.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class MenuItemAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuItemAttribute"/> class.
    /// </summary>
    /// <param name="menuPath">The hierarchical menu path (e.g. "File>New").</param>
    /// <param name="quickKey">Optional keyboard shortcut (e.g. "Ctrl+N").</param>
    /// <param name="toolTip">Optional tooltip text.</param>
    /// <param name="separatorBefore">Whether to insert a separator before the item.</param>
    /// <param name="disabled">Whether the item should start disabled.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="menuPath"/>
    /// is null or whitespace.</exception>
    public MenuItemAttribute(
        string menuPath,
        string? quickKey = null,
        string? toolTip = null,
        bool separatorBefore = false,
        bool disabled = false)
    {
        // Validate the menu path
        if (string.IsNullOrWhiteSpace(menuPath))
            throw new ArgumentException("Menu path is required.", nameof(menuPath));

        // Set the properties
        MenuPath  = menuPath;
        QuickKey  = quickKey;
        ToolTip   = toolTip;
        SeparatorBefore = separatorBefore;
        Disabled  = disabled;
    }

    #region Properties
    /// <summary>
    /// The hierarchical menu path for the item (e.g. "File>Open").
    /// </summary>
    /// <remarks>
    /// The menu path is the path to the menu item.
    /// The '>' character is used to separate the menu items.
    /// </remarks>
    public string MenuPath { get; }

    /// <summary>
    /// The keyboard shortcut to activate the menu item, in human-readable form
    /// (e.g. "Ctrl+N").
    /// Optional.
    /// </summary>
    /// <remarks>
    /// The keyboard shortcut is a combination of the control key and the letter key.
    /// For example, the keyboard shortcut for the 'New' menu item is 'Ctrl+N'.
    /// </remarks>
    public string? QuickKey { get; }

    /// <summary>
    /// The tooltip text displayed for the menu item.
    /// Optional.
    /// </summary>
    /// <remarks>
    /// The tooltip is the text that appears when the mouse hovers over the menu item.
    /// </remarks>
    public string? ToolTip { get; }

    /// <summary>
    /// Indicates whether a separator should be added before the menu item.
    /// </summary>
    /// <remarks>
    /// If true, a separator will placed before the item.
    /// </remarks>
    public bool SeparatorBefore { get; }

    /// <summary>
    /// Indicates whether the menu item should start disabled.
    /// </summary>
    /// <remarks>
    /// If true, the menu item will be Disabled.
    /// </remarks>
    public bool Disabled { get; }

    /// <summary>
    /// An optional object that can be associated with the menu item.
    /// </summary>
    public object? Tag { get; set; }
    #endregion

    /// <summary>
    /// Exports this menu item as an object containing its metadata for serialization.
    /// </summary>
    /// <returns>A new <see cref="MenuItem"/> object with the same metadata.</returns>
    public MenuItem MenuItem()
    {
        return new MenuItem()
        {
            MenuPath   = MenuPath,
            QuickKey   = QuickKey ?? string.Empty,
            ToolTip    = ToolTip ?? string.Empty,
            SeparatorBefore = SeparatorBefore,
            Disabled   = Disabled,
            Tag        = Tag ?? string.Empty
        };
    }
}
