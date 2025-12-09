// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Blackwood;


/// <summary>
/// This is a collection of attributes to describe the metadata of an item.
/// </summary>
/// <remarks>
/// This is distinct from the properties.  Attributes are meant for the
/// management of the design, esp by the user.  Properties are intended to be
/// used by the code, and possible set by the code.
/// </remarks>
public partial class ItemAttributes
{
    #region Non-Editable Management Properties
    /// <summary>
    /// The globally unique identifier for the item.
    /// </summary>
    /// <remarks>
    /// This is used to allow nodes and their components to refer to other.
    /// It's needed internally allow getting at it.
    /// </remarks>
    [Description("The globally unique identifier for this item.")]
    [Browsable(false)]
    public Guid Guid = Guid.NewGuid();

    /// <summary>
    /// The kind of thing that it is.
    /// </summary>
    [Browsable(false)]
    public string? TypeName;

    /// <summary>
    /// The date and time that this item was created.
    /// </summary>
    [Category("Information")]
    [Description("The date and time that this item was created.")]
    [Browsable(false)]
    public DateTime CreatedUTC = DateTime.UtcNow;

    /// <summary>
    /// The date and time that this item was last modified.
    /// </summary>
    [Category("Information")]
    [Description("The date and time that this item was last modified.")]
    [Browsable(false)]
    public DateTime LastModifiedUTC = DateTime.UtcNow;
    #endregion

    #region Potentially Editable Management Properties
    /// <summary>
    /// The name of the item.
    /// </summary>
    string Name_ = "Untitled";

    /// <summary>
    /// The name of the item.
    /// </summary>
    /// <remarks>
    /// If the name is null or whitespace, it will be set to "Untitled".
    /// </remarks>
    [Category("Information")]
    [Description("The name of the item.")]
    public string Name
    {
        get => Name_;
        set
        {
            // Default the name to "Untitled", if given a null or whitespace,
            Name_ = string.IsNullOrWhiteSpace(value) ? "Untitled" : value.Trim();
        }
    }


    /// <summary>
    /// The author of the item.
    /// </summary>
    [Category("Information")]
    [Description("The author of the item.")]
    public string? Author;

    /// <summary>
    /// The author of the item.
    /// </summary>
    [Category("Information")]
    [Description("The email address of the author.")]
    public System.Net.Mail.MailAddress? AuthorEmail;

    /// <summary>
    /// Gets or sets the version of the item.
    /// This property indicates the current version number of the project, which can be used
    /// to track changes, support migrations, or verify compatibility with different application features.
    /// </summary>
    [Category("Information")]
    [Description("The version of the item.")]
    public Version Version = new(0, 0, 0, 0);

    /// <summary>
    /// The description of the item.
    /// </summary>
    [Category("Information")]
    [Description("The description of the item.")]
    public string? Description;

    /// <summary>
    /// The link of the item.
    /// </summary>
    [Category("Information")]
    [Description("The link of the item.")]
    public Uri? Link;

    /// <summary>
    /// A link to the documentation of the item.
    /// </summary>
    [Category("Information")]
    [Description("A link to the documentation of the item.")]
    public Uri? Documentation;

    /// <summary>
    /// Comments for the item.
    /// </summary>
    [Category("Information")]
    [Description("Notes about the item.")]
    public string? Comments;
    #endregion

    /// <summary>
    /// Creates a shallow copy of the current <see cref="ItemAttributes"/>.
    /// </summary>
    /// <returns>A cloned <see cref="ItemAttributes"/> instance with the same
    /// property values.</returns>
    public ItemAttributes Clone()
    {
        return new ItemAttributes
        {
            Name            = this.Name,
            Author          = this.Author,
            Version         = new Version(this.Version.Major, this.Version.Minor, this.Version.Build, this.Version.Revision),
            CreatedUTC      = this.CreatedUTC,
            LastModifiedUTC = this.LastModifiedUTC,
            Description     = this.Description,
            Link            = this.Link != null ? new Uri(this.Link.ToString()) : null,
            Documentation   = this.Documentation != null ? new Uri(this.Documentation.ToString()) : null,
            Comments        = this.Comments
        };
    }
}
