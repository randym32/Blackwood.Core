// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

/* The project is trying to use the standard .Net tools and bridge to that, in
   such a way the more commonly maintained code is simple, and clearer.  The
   codes is that is in this file, with a boiler plate, complex bridge code.

   The complexity of this proxy makes me vomit a bit.

   It's not even particularly fast, not that one should prioritize that.
*/
using System.ComponentModel;
using System.Reflection;

namespace Blackwood;

/// <summary>
/// A dynamic object that exposes select object properties or static properties
/// preferences to the PropertyGrid.
/// </summary>
/// <remarks>
/// This helper class enables (in a PropertyGrid) display and editing of
/// - application-wide Settings
/// - project Settings
/// - object properties
///
/// The Windows Forms PropertyGrid control
/// operates on object instances, using reflection to show and edit properties.
/// For global Settings, these are defined as static properties in various
/// classes (e.g., for application Settings or preferences).  PropertyGrid is
/// not directly able to edit static properties, so this wrapper bridges the gap.
///
/// How it works:
/// - The client provides objects and class to scan; the items with properties
///   that have a DescriptionAttribute (and are public and settable)
/// - ProxyPropertiesObject implements ICustomTypeDescriptor, providing the
///   necessary hooks for the PropertyGrid to dynamically discover all "virtual"
///   properties corresponding to these static properties.
/// - When values are changed in the PropertyGrid, ProxyPropertiesObject
///   forwards gets and sets to the corresponding static property.
/// - As a result, the PropertyGrid UI can be used to inspect and edit all
///   static application preferences at runtime, greatly simplifying management
///   of user-configurable Settings.
///
/// For inspecting and fiddling with an instances properties, this acts as a
/// Synthetic wrapper
/// This is used to select the subset properties that are of interest to be edited with a PropertyGrid.
/// </remarks>
[System.Text.Json.Serialization.JsonConverter(typeof(PropertiesJsonConverter))]
public class ProxyPropertiesObject : ICustomTypeDescriptor
{
    /// <summary>
    /// The object that we should modify.
    /// </summary>
    internal readonly object? instance;

    /// <summary>
    /// Initializes a new instance of the ProxyPropertiesObject class.
    /// </summary>
    public ProxyPropertiesObject(object? instance=null)
    {
        this.instance = instance;
    }

    /// <summary>
    /// Initializes a new instance of the ProxyPropertiesObject class.
    /// </summary>
    /// <param name="classProxy">The class proxy to copy the descriptors from.</param>
    public ProxyPropertiesObject(ProxyPropertiesObject classProxy, object instance)
    {
        // Copy the descriptors from the class proxy
        descriptors_ = classProxy.descriptors_;
        this.instance = instance;
    }

    #region Collection of Descriptors
    /// <summary>
    /// The list of properties that can be modified.
    /// </summary>
    readonly List<ProxyPropertyDescriptor> descriptors_ = [];

    /// <summary>
    /// Returns the number of property descriptors available.
    /// </summary>
    public int DescriptorCount()
    {
        return descriptors_.Count;
    }

    /// <summary>
    /// Adds a property to the list of properties that can be modified.
    /// </summary>
    /// <param name="descriptor">The property descriptor.</param>
    internal void Add(ProxyPropertyDescriptor descriptor)
    {
        // Add this to the list of things that can be modified
        descriptors_.Add(descriptor);
    }

    /// <summary>
    /// Adds the properties for object instances.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="predicate">A predicate to determine whether a property or field should be added.</param>
    /// <remarks>The proxy object need not have an instance, if it is preparing for multiple.</remarks>
    public void AddInstanceProperties(Type type, Func<MemberInfo, bool>? predicate = null)
    {
        // Get the binding flags, using instance if an object is provided,
        AddPropertiesFor(type, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic, predicate);
    }

    /// <summary>
    /// Adds the properties for a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="predicate">A predicate to determine whether a property or field should be added.</param>
    public void AddClassProperties(Type type, Func<MemberInfo, bool>? predicate = null)
    {
        // static otherwise (meaning it is a static property of the class)
        AddPropertiesFor(type, BindingFlags.Static, predicate);
    }


    /// <summary>
    /// Gets the properties for a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="instance">The instance of the type.</param>
    /// <param name="predicate">A predicate to determine whether a property or field should be added.</param>
    void AddPropertiesFor(Type type, BindingFlags flags, Func<MemberInfo, bool>? predicate = null)
    {
        // Get all properties
        var properties = type.GetProperties(BindingFlags.Public | flags);
        foreach (var property in properties)
            try
            {
                // Check if the property has a DescriptionAttribute
                var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr == null || !property.CanRead || !property.CanWrite)
                    continue;

                // If there is a predicate, and it returns false, skip this property
                if (null != predicate && !predicate(property))
                    continue;

                // Store the default value (prefer DefaultValueAttribute, fallback to current value)
                Add(ProxyPropertyDescriptor.Create(property));
            }
            catch
            {
                // If we can't get the value, skip this property
            }

        // Get all fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | flags)
                         .Where(f => f.IsPublic || f.IsAssembly);
        foreach (var field in fields)
            try
            {
                // Check if the field has a DescriptionAttribute
                var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr == null || field.IsLiteral || field.IsInitOnly)
                    continue;

                // If there is a predicate, and it returns false, skip this field
                if (null != predicate && !predicate(field))
                    continue;

                // Store the default value (prefer DefaultValueAttribute, fallback to current value)
                Add(ProxyPropertyDescriptor.Create(field));
            }
            catch
            {
                // If we can't get the value, skip this field
            }
    }
    #endregion

    #region Changing Property Values
    /// <summary>
    /// Resets all properties to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        foreach (var descriptor in descriptors_)
        {
            descriptor.ResetValue(this);
        }
    }

    /// <summary>
    /// Captures the current, non-default values of all properties as a checkpoint.
    /// </summary>
    /// <returns>A list of the properties and their values.</returns>
    public List<(ProxyPropertyDescriptor, object?)> Checkpoint()
    {
        // Create the checkpoint
        List<(ProxyPropertyDescriptor, object?)> checkpoint = [];

        // Go thru and save the state of the variables
        foreach (var prop in descriptors_)
        {
            // Get the value
            var value = prop.GetValue(this);
            checkpoint.Add((prop, value));
        }

        // Return the checkpoint
        return checkpoint;
    }


    /// <summary>
    /// Restores the properties to their values at the time of the checkpoint.
    /// </summary>
    /// <param name="checkpoint">The checkpoint to restore.</param>
    public void RestoreCheckpoint(List<(ProxyPropertyDescriptor, object?)> checkpoint)
    {
        // Restore properties to their backup values
        foreach (var (prop, value) in checkpoint)
        {
            // If the value has changed, set it back
            if (prop.GetValue(this) != value)
                prop.SetValue(this, value);
        }
    }


    /// <summary>
    /// Applies property values to a ProxyPropertiesObject.
    /// This is a basic implementation that attempts to set values on existing properties.
    /// </summary>
    /// <param name="proxy">The ProxyPropertiesObject to apply values to.</param>
    /// <param name="propertyValues">The property values to apply.</param>
    internal void ApplyPropertyValues(Dictionary<string, object?> propertyValues)
    {
        var properties = GetProperties();

        // Apply each of the passed property values to the properties
        foreach (var kvp in propertyValues)
        {
            // Look up the property; if it is no longer present, skip it
            var prop = properties.Find(kvp.Key, false);
            if (prop == null) continue;
            try
            {
                // Convert the value to the property type if needed
                var convertedValue = JSONConvert.ConvertToType(kvp.Value, prop.PropertyType);
                prop.SetValue(this, convertedValue);
            }
            catch (Exception ex)
            {
                // Log or handle conversion errors as needed
                // For now, we'll silently skip properties that can't be converted
                System.Diagnostics.Debug.WriteLine($"Failed to set property {kvp.Key}: {ex.Message}");
            }
        }
    }
    #endregion


    #region ICustomTypeDescriptor methods
    /// <summary>
    /// Gets the attributes for this object.
    /// </summary>
    /// <returns>An AttributeCollection.</returns>
    public AttributeCollection GetAttributes()
    {
        return TypeDescriptor.GetAttributes(this, true);
    }

    /// <summary>
    /// Gets the class name for this object.
    /// </summary>
    /// <returns>The class name.</returns>
    public string? GetClassName()
    {
        return TypeDescriptor.GetClassName(this, true);
    }

    /// <summary>
    /// Gets the component name for this object.
    /// </summary>
    /// <returns>The component name.</returns>
    public string? GetComponentName()
    {
        return TypeDescriptor.GetComponentName(this, true);
    }

    /// <summary>
    /// Gets the type converter for this object.
    /// </summary>
    /// <returns>A TypeConverter.</returns>
    public TypeConverter GetConverter()
    {
        return TypeDescriptor.GetConverter(this, true);
    }

    /// <summary>
    /// Gets the default event for this object.
    /// </summary>
    /// <returns>An EventDescriptor.</returns>
    public EventDescriptor? GetDefaultEvent()
    {
        return TypeDescriptor.GetDefaultEvent(this, true);
    }

    /// <summary>
    /// Gets the default property for this object.
    /// </summary>
    /// <returns>A PropertyDescriptor.</returns>
    public PropertyDescriptor? GetDefaultProperty()
    {
        return null;
    }

    /// <summary>
    /// Gets the editor for this object.
    /// </summary>
    /// <param name="editorBaseType">The base type of the editor.</param>
    /// <returns>An object that is the editor.</returns>
    public object? GetEditor(Type editorBaseType)
    {
        return TypeDescriptor.GetEditor(this, editorBaseType, true);
    }

    /// <summary>
    /// Gets the events for this object.
    /// </summary>
    /// <param name="attributes">The attributes to filter by.</param>
    /// <returns>An EventDescriptorCollection.</returns>
    public EventDescriptorCollection GetEvents(Attribute[]? attributes)
    {
        return TypeDescriptor.GetEvents(this, attributes, true);
    }

    /// <summary>
    /// Gets the events for this object.
    /// </summary>
    /// <returns>An EventDescriptorCollection.</returns>
    public EventDescriptorCollection GetEvents()
    {
        return TypeDescriptor.GetEvents(this, true);
    }

    /// <summary>
    /// Gets the properties for this object.
    /// </summary>
    /// <param name="attributes">The attributes to filter by.</param>
    /// <returns>A PropertyDescriptorCollection.</returns>
    public PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
    {
        return new PropertyDescriptorCollection(descriptors_.ToArray());
    }

    /// <summary>
    /// Gets the properties for this object.
    /// </summary>
    /// <returns>A PropertyDescriptorCollection.</returns>
    public PropertyDescriptorCollection GetProperties()
    {
        return GetProperties(null);
    }

    /// <summary>
    /// Gets the owner of this object.
    /// </summary>
    /// <param name="pd">The property descriptor.</param>
    /// <returns>The owner object.</returns>
    public object? GetPropertyOwner(PropertyDescriptor? pd)
    {
        return this;
    }
    #endregion
}



/// <summary>
/// A dynamic property descriptor for preferences properties.
/// </summary>
/// <remarks>
/// This class is used to represent a "virtual" property in the ProxyPropertiesObject.
/// It holds the name, description, grouping, etc, with hooks for mediate the
/// access to the value.  This can be used to proxy to static properties,
/// fields, virtual properties, etc.
/// </remarks>
public class ProxyPropertyDescriptor : PropertyDescriptor
{
    /// <summary>
    /// The type of the property.
    /// </summary>
    private readonly Type propertyType_;

    /// <summary>
    /// The default value for the property.
    /// </summary>
    private readonly object? defaultValue_;

    /// <summary>
    /// The setter for the property, takes target object and value as parameters.
    /// </summary>
    private readonly Action<object?, object?> setter_;

    /// <summary>
    /// The getter for the property.
    /// </summary>
    private readonly Func<object?, object?> getter_;


    /// <summary>
    /// Creates the attributes for the property descriptor.
    /// </summary>
    /// <param name="memberInfo">The MemberInfo for the property.</param>
    /// <returns>An array of attributes.</returns>
    private static Attribute[] CreateAttributes(MemberInfo memberInfo)
    {
        // The plan here is to return the attribute as is, if there is an
        // attribute that gives the display name.
        if (null != memberInfo.GetCustomAttribute<DisplayNameAttribute>())
            return memberInfo.GetCustomAttributes().ToArray();

        // There isn't a display label, we'll create one and include any existing attributes
        var attributes = new List<Attribute>(memberInfo.GetCustomAttributes())
        {
            new DisplayNameAttribute(Utils.ConvertCNameToLabel(memberInfo.Name))
        };

        return attributes.ToArray();
    }

    /// <summary>
    /// Creates a new instance of the ProxyPropertyDescriptor class.
    /// </summary>
    /// <param name="property">The PropertyInfo for the property.</param>
    /// <returns>A new instance of the ProxyPropertyDescriptor class.</returns>
    public static ProxyPropertyDescriptor Create( PropertyInfo property)
    {
        // Get the attributes for the property
        var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
        var defaultValue     = defaultValueAttr?.Value;

        // Create the descriptor around the property
        return new ProxyPropertyDescriptor( property.Name
                                          , property.PropertyType
                                          , CreateAttributes(property)
                                          , defaultValue
                                          , (tgt,value) => property.SetValue(tgt, value)
                                          , (tgt)       => property.GetValue(tgt));
    }

    /// <summary>
    /// Creates a new instance of the ProxyPropertyDescriptor class.
    /// </summary>
    /// <param name="field">The FieldInfo for the field.</param>
    /// <returns>A new instance of the ProxyPropertyDescriptor class.</returns>
    public static ProxyPropertyDescriptor Create(FieldInfo field)
    {
        var defaultValueAttr = field.GetCustomAttribute<DefaultValueAttribute>();
        var defaultValue     = defaultValueAttr?.Value;

        // Create the descriptor around the field
        return new ProxyPropertyDescriptor( field.Name
                                          , field.FieldType
                                          , CreateAttributes(field)
                                          , defaultValue
                                          , (tgt, value) => field.SetValue(tgt, value)
                                          , (tgt)        => field.GetValue(tgt));
    }

    /// <summary>
    /// Initializes a new instance of the ProxyPropertyDescriptor class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="propertyType">The property type.</param>
    /// <param name="attributes">The attributes for the property.</param>
    /// <param name="defaultValue">The default value for the property.</param>
    /// <param name="setter">The setter for the property.</param>
    /// <param name="getter">The getter for the property.</param>
    ProxyPropertyDescriptor( string       name
                           , Type         propertyType
                           , Attribute[]    attributes
                           , object?        defaultValue
                           , Action<object?,object?> setter
                           , Func<object?, object?>   getter
                           )
       : base(name, attributes)
    {
        propertyType_ = propertyType;
        setter_ = setter;
        getter_ = getter;
        defaultValue_ = defaultValue;
    }

    /// <summary>
    /// Gets the type of the component this property is bound to.
    /// </summary>
    public override Type ComponentType => typeof(ProxyPropertiesObject);

    /// <summary>
    /// Gets whether this property is read-only.
    /// </summary>
    public override bool IsReadOnly => false;

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public override Type PropertyType => propertyType_;

    /// <summary>
    /// Gets whether the property can be reset.
    /// </summary>
    /// <param name="component">The proxy object (ProxyPropertiesObject) that this property is part of.</param>
    /// <returns>True if the property can be reset.</returns>
    public override bool CanResetValue(object component)
    {
        // Can reset if we have a stored default value
        return defaultValue_ != null;
    }

    /// <summary>
    /// Gets the current value of the property.
    /// </summary>
    /// <param name="component">The proxy object (ProxyPropertiesObject) that this property is part of.</param>
    /// <returns>The current value.</returns>
    public override object? GetValue(object? component)
    {
        object? tgt = component;
        if (component is ProxyPropertiesObject po)
            tgt = po.instance;
        return getter_?.Invoke(tgt);
    }

    /// <summary>
    /// Resets the property value.
    /// </summary>
    /// <param name="component">The proxy object (ProxyPropertiesObject) that this property is part of.</param>
    public override void ResetValue(object component)
    {
        // Fallback to the stored original/default value
        if (null != defaultValue_ && GetValue(null) != defaultValue_)
            SetValue(component, defaultValue_);
    }

    /// <summary>
    /// Sets the value of the property.
    /// </summary>
    /// <param name="component">The proxy object (ProxyPropertiesObject) that this property is part of.</param>
    /// <param name="value">The new value.</param>
    public override void SetValue(object? component, object? value)
    {
        object? tgt = component;
        if (component is ProxyPropertiesObject po)
            tgt = po.instance;
        setter_?.Invoke(tgt, value);
    }

    /// <summary>
    /// Determines whether the property value should be serialized.
    /// </summary>
    /// <param name="component">The proxy object (ProxyPropertiesObject) that this property is part of.</param>
    /// <returns>True if the property should be serialized.</returns>
    public override bool ShouldSerializeValue(object component)
    {
        var currentValue = GetValue(component);

        // Don't serialize if the current value equals the default value
        if (currentValue == null && defaultValue_ == null)
            return false;

        if (currentValue != null && currentValue.Equals(defaultValue_))
            return false;

        return true;
    }
}
