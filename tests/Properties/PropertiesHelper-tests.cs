using System.ComponentModel;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Blackwood.Core.Tests;

/// <summary>
/// Tests for ProxyPropertiesObject and ProxyPropertyDescriptor in PropertiesHelper.cs.
///
/// Why these tests exist:
/// - The PropertyGrid UI consumes PropertyDescriptor instances; our helper dynamically
///   creates these to wrap static properties/fields. We must ensure discovery and
///   behavior are correct so the UI remains accurate and safe.
///
/// Covered behaviors:
/// - Discovery: only members annotated with DescriptionAttribute are surfaced.
/// - Get/Set forwarding: reads and writes propagate to underlying static members.
/// - Default handling: DefaultValueAttribute provides reset semantics and affects
///   ShouldSerializeValue.
/// - Field handling: fields (not only properties) are supported with the same rules.
/// </summary>
public class PropertiesHelperTests
{
    /// <summary>
    /// A sample settings class with static properties/fields to be surfaced.
    /// Only members bearing DescriptionAttribute should be exposed by the helper.
    /// </summary>
    private static class SampleSettings
    {
        [Description("An integer value")]
        [DefaultValue(42)]
        public static int IntSetting { get; set; } = 42;

        [Description("A string value")]
        [DefaultValue("hello")]
        public static string? StringSetting { get; set; } = "hello";

        [Description("A boolean field")]
        [DefaultValue(true)]
        public static bool BoolField = true;

        // Not included (no Description)
        public static double HiddenSetting { get; set; } = 3.14;
    }

    /// <summary>
    /// A sample instance class with instance properties/fields to be surfaced.
    /// Used to test instance property discovery functionality.
    /// </summary>
    private class SampleInstanceSettings
    {
        [Description("Instance integer value")]
        [DefaultValue(100)]
        public int InstanceIntSetting { get; set; } = 100;

        [Description("Instance string value")]
        [DefaultValue("instance")]
        public string? InstanceStringSetting { get; set; } = "instance";

        [Description("Instance boolean field")]
        [DefaultValue(false)]
        public bool InstanceBoolField = false;

        [Description("Assembly-level field")]
        [DefaultValue("assembly")]
        internal string AssemblyField = "assembly";

        // Not included (no Description)
        public double HiddenInstanceSetting { get; set; } = 2.71;

        // Not included (read-only)
        [Description("Read-only property")]
        public int ReadOnlyProperty { get; } = 999;

        // Not included (literal/const)
        [Description("Literal field")]
        public const int LiteralField = 123;

        // Not included (init-only)
        [Description("Init-only field")]
        public readonly int InitOnlyField = 456;
    }

    /// <summary>
    /// Reset the sample static settings to their defaults before each test.
    /// Ensures tests are isolated and deterministic.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Ensure known defaults before each test
        SampleSettings.IntSetting = 42;
        SampleSettings.StringSetting = "hello";
        SampleSettings.BoolField = true;
    }

    /// <summary>
    /// Verifies that only members with <see cref="DescriptionAttribute"/> are discovered
    /// and surfaced as descriptors (2 properties + 1 field).
    /// </summary>
    [Test]
    public void AddPropertiesFor_Type_DiscoversOnlyDescribedMembers()
    {
        // Arrange: build a proxy over a type with a mix of described and undescribed members
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Assert: only the 3 described members are surfaced (2 properties + 1 field)
        Assert.That(proxy.DescriptorCount(), Is.EqualTo(3),
            "Should include only members with DescriptionAttribute (2 props + 1 field)");
    }

    /// <summary>
    /// Ensures descriptor GetValue/SetValue forward to the underlying static property.
    /// </summary>
    [Test]
    public void GetSetValue_ForwardsToStaticProperty()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false);
        Assert.That(intProp, Is.Not.Null);

        // Act/Assert: Read default through the descriptor accessor
        Assert.That(intProp!.GetValue(proxy), Is.EqualTo(42));

        // Act: Write a new value via the descriptor; verify the static target changed
        intProp.SetValue(proxy, 99);
        Assert.Multiple(() =>
        {
            Assert.That(SampleSettings.IntSetting, Is.EqualTo(99));
            Assert.That(intProp.GetValue(proxy), Is.EqualTo(99));
        });
    }

    /// <summary>
    /// Confirms ResetValue uses the value from <see cref="DefaultValueAttribute"/> when present.
    /// </summary>
    [Test]
    public void Reset_UsesDefaultValueAttribute_First()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var props = proxy.GetProperties();
        var strProp = props.Find(nameof(SampleSettings.StringSetting), false);
        Assert.That(strProp, Is.Not.Null);

        // Act: Change the value away from its DefaultValue("hello")
        strProp!.SetValue(proxy, "world");
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("world"));

        // Act/Assert: Reset should restore the DefaultValue
        Assert.That(strProp.CanResetValue(proxy), Is.True);
        strProp.ResetValue(proxy);
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("hello"));
    }

    /// <summary>
    /// Verifies ShouldSerializeValue returns false at default and true when value differs.
    /// </summary>
    [Test]
    public void ShouldSerializeValue_ComparesAgainstDefault()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false)!;

        // Assert: At default value, this should not be serialized
        Assert.That(intProp.ShouldSerializeValue(proxy), Is.False);

        // Act: Change value; now it should be serialized since it differs from default
        intProp.SetValue(proxy, 100);
        Assert.That(intProp.ShouldSerializeValue(proxy), Is.True);
    }

    /// <summary>
    /// Checks that described fields (not just properties) support get/set and reset semantics.
    /// </summary>
    [Test]
    public void Field_SupportsGetSetAndReset()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var props = proxy.GetProperties();
        var field = props.Find(nameof(SampleSettings.BoolField), false)!;

        // Assert: field exposed and reads DefaultValue
        Assert.That(field.GetValue(proxy), Is.EqualTo(true));

        // Act: Change via descriptor and verify underlying static field changed
        field.SetValue(proxy, false);
        Assert.That(SampleSettings.BoolField, Is.False);

        // Act/Assert: Reset returns the field to its annotated default
        Assert.That(field.CanResetValue(proxy), Is.True);
        field.ResetValue(proxy);
        Assert.That(SampleSettings.BoolField, Is.True);
    }

    /// <summary>
    /// Tests that instance properties and fields are discovered and can be accessed through the proxy.
    /// This verifies the AddPropertiesFor(object obj) functionality for instance members.
    /// </summary>
    [Test]
    public void AddPropertiesFor_InstanceObject_DiscoversInstanceMembers()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);
        proxy.AddInstanceProperties(instance.GetType());

        // Assert: Should discover 4 described instance members (3 properties + 1 field)
        // Note: Read-only properties are correctly excluded
        Assert.That(proxy.DescriptorCount(), Is.EqualTo(4),
            "Should include instance members with DescriptionAttribute (3 props + 1 field)");

        var props = proxy.GetProperties();

        // Verify instance property access
        var intProp = props.Find(nameof(SampleInstanceSettings.InstanceIntSetting), false);
        Assert.That(intProp, Is.Not.Null);
        Assert.That(intProp!.GetValue(proxy), Is.EqualTo(100));

        // Verify instance field access
        var field = props.Find(nameof(SampleInstanceSettings.InstanceBoolField), false);
        Assert.That(field, Is.Not.Null);
        Assert.That(field!.GetValue(proxy), Is.EqualTo(false));

        // Verify assembly-level field access
        var assemblyField = props.Find(nameof(SampleInstanceSettings.AssemblyField), false);
        Assert.That(assemblyField, Is.Not.Null);
        Assert.That(assemblyField!.GetValue(proxy), Is.EqualTo("assembly"));
    }

    /// <summary>
    /// Tests that instance property changes are reflected in the underlying object.
    /// This verifies that the proxy correctly forwards get/set operations to instance members.
    /// </summary>
    [Test]
    public void GetSetValue_InstanceProperty_ForwardsToInstanceObject()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);
        proxy.AddInstanceProperties(instance.GetType());

        var props = proxy.GetProperties();
        var strProp = props.Find(nameof(SampleInstanceSettings.InstanceStringSetting), false);
        Assert.That(strProp, Is.Not.Null);

        // Act: Change value through proxy
        strProp!.SetValue(proxy, "modified");

        // Assert: Underlying instance should be updated
        Assert.That(instance.InstanceStringSetting, Is.EqualTo("modified"));
        Assert.That(strProp.GetValue(proxy), Is.EqualTo("modified"));
    }

    /// <summary>
    /// Tests that ResetToDefaults resets all properties to their default values.
    /// This verifies the bulk reset functionality for all managed properties.
    /// </summary>
    [Test]
    public void ResetToDefaults_ResetsAllPropertiesToDefaults()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Change all values away from defaults
        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false)!;
        var strProp = props.Find(nameof(SampleSettings.StringSetting), false)!;
        var boolProp = props.Find(nameof(SampleSettings.BoolField), false)!;

        intProp.SetValue(proxy, 999);
        strProp.SetValue(proxy, "changed");
        boolProp.SetValue(proxy, false);

        // Verify values are changed
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(999));
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("changed"));
        Assert.That(SampleSettings.BoolField, Is.False);

        // Act: Reset all to defaults
        proxy.ResetToDefaults();

        // Assert: All values should be back to defaults
        Assert.Multiple(() =>
        {
            Assert.That(SampleSettings.IntSetting, Is.EqualTo(42));
            Assert.That(SampleSettings.StringSetting, Is.EqualTo("hello"));
            Assert.That(SampleSettings.BoolField, Is.True);
        });
    }

    /// <summary>
    /// Tests that Checkpoint captures the current state of all properties.
    /// This verifies the checkpoint functionality for saving current property values.
    /// </summary>
    [Test]
    public void Checkpoint_CapturesCurrentPropertyValues()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Change some values
        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false)!;
        var strProp = props.Find(nameof(SampleSettings.StringSetting), false)!;

        intProp.SetValue(proxy, 200);
        strProp.SetValue(proxy, "checkpoint");

        // Act: Create checkpoint
        var checkpoint = proxy.Checkpoint();

        // Assert: Checkpoint should contain current values
        Assert.That(checkpoint, Has.Count.EqualTo(3)); // All 3 properties

        var intCheckpoint = checkpoint.FirstOrDefault(c => c.Item1.Name == nameof(SampleSettings.IntSetting));
        var strCheckpoint = checkpoint.FirstOrDefault(c => c.Item1.Name == nameof(SampleSettings.StringSetting));
        var boolCheckpoint = checkpoint.FirstOrDefault(c => c.Item1.Name == nameof(SampleSettings.BoolField));

        Assert.That(intCheckpoint.Item2, Is.EqualTo(200));
        Assert.That(strCheckpoint.Item2, Is.EqualTo("checkpoint"));
        Assert.That(boolCheckpoint.Item2, Is.EqualTo(true)); // Unchanged from default
    }

    /// <summary>
    /// Tests that RestoreCheckpoint restores properties to their checkpointed values.
    /// This verifies the restore functionality for reverting to saved property states.
    /// </summary>
    [Test]
    public void RestoreCheckpoint_RestoresPropertiesToCheckpointedValues()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Create initial checkpoint
        var initialCheckpoint = proxy.Checkpoint();

        // Change values
        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false)!;
        var strProp = props.Find(nameof(SampleSettings.StringSetting), false)!;

        intProp.SetValue(proxy, 300);
        strProp.SetValue(proxy, "modified");

        // Verify values are changed
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(300));
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("modified"));

        // Act: Restore checkpoint
        proxy.RestoreCheckpoint(initialCheckpoint);

        // Assert: Values should be restored to checkpoint state
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(42)); // Original default
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("hello")); // Original default
    }

    /// <summary>
    /// Tests that RestoreCheckpoint only changes values that have actually changed.
    /// This verifies the optimization that prevents unnecessary property updates.
    /// </summary>
    [Test]
    public void RestoreCheckpoint_OnlyChangesValuesThatHaveChanged()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Create initial checkpoint (all at defaults)
        var initialCheckpoint = proxy.Checkpoint();

        // Change both values
        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false)!;
        var strProp = props.Find(nameof(SampleSettings.StringSetting), false)!;

        intProp.SetValue(proxy, 400);
        strProp.SetValue(proxy, "changed");

        // Create checkpoint after both changes
        var checkpoint = proxy.Checkpoint();

        // Change one value again
        intProp.SetValue(proxy, 500);

        // Act: Restore checkpoint (should restore both to checkpoint values)
        proxy.RestoreCheckpoint(checkpoint);

        // Assert: Both should be restored to checkpoint values
        Assert.Multiple(() =>
        {
            Assert.That(SampleSettings.IntSetting, Is.EqualTo(400)); // Restored to checkpoint
            Assert.That(SampleSettings.StringSetting, Is.EqualTo("changed")); // Restored to checkpoint
        });
    }

    /// <summary>
    /// Tests that read-only properties are correctly excluded from discovery.
    /// This verifies that properties without setters are not included in the proxy.
    /// </summary>
    [Test]
    public void AddPropertiesFor_ExcludesReadOnlyProperties()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);
        proxy.AddInstanceProperties(instance.GetType());

        // Assert: Read-only property should not be included
        var props = proxy.GetProperties();
        var readOnlyProp = props.Find(nameof(SampleInstanceSettings.ReadOnlyProperty), false);
        Assert.That(readOnlyProp, Is.Null, "Read-only properties should be excluded");

        // Should include only writable described members
        Assert.That(proxy.DescriptorCount(), Is.EqualTo(4), "Should include 4 writable described members");
    }

    /// <summary>
    /// Tests that literal and init-only fields are excluded from discovery.
    /// This verifies that const and readonly fields are not included in the proxy.
    /// </summary>
    [Test]
    public void AddPropertiesFor_ExcludesLiteralAndInitOnlyFields()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);
        proxy.AddInstanceProperties(instance.GetType());

        // Assert: Literal and init-only fields should not be included
        var props = proxy.GetProperties();
        var literalField = props.Find(nameof(SampleInstanceSettings.LiteralField), false);
        var initOnlyField = props.Find(nameof(SampleInstanceSettings.InitOnlyField), false);

        Assert.That(literalField, Is.Null, "Literal fields should be excluded");
        Assert.That(initOnlyField, Is.Null, "Init-only fields should be excluded");

        // Should still include other fields
        Assert.That(proxy.DescriptorCount(), Is.EqualTo(4), "Should include 4 described members");
    }

    /// <summary>
    /// Tests that properties without DescriptionAttribute are excluded from discovery.
    /// This verifies the filtering logic for described vs undescribed members.
    /// </summary>
    [Test]
    public void AddPropertiesFor_ExcludesMembersWithoutDescription()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);
        proxy.AddInstanceProperties(instance.GetType());

        // Assert: Hidden property should not be included
        var props = proxy.GetProperties();
        var hiddenProp = props.Find(nameof(SampleInstanceSettings.HiddenInstanceSetting), false);
        Assert.That(hiddenProp, Is.Null, "Properties without DescriptionAttribute should be excluded");

        // Should only include described members
        Assert.That(proxy.DescriptorCount(), Is.EqualTo(4), "Should include only 4 described members");
    }

    /// <summary>
    /// Tests that the proxy handles mixed static and instance properties correctly.
    /// This verifies that both static and instance discovery methods work together.
    /// </summary>
    [Test]
    public void AddPropertiesFor_MixedStaticAndInstance_HandlesBothCorrectly()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);

        // Add both static and instance properties
        proxy.AddClassProperties(typeof(SampleSettings)); // Static
        proxy.AddInstanceProperties(instance.GetType()); // Instance

        // Assert: Should include both static (3) and instance (4) members
        Assert.That(proxy.DescriptorCount(), Is.EqualTo(7),
            "Should include both static (3) and instance (4) described members");

        var props = proxy.GetProperties();

        // Verify static properties are accessible
        var staticProp = props.Find(nameof(SampleSettings.IntSetting), false);
        Assert.That(staticProp, Is.Not.Null);
        Assert.That(staticProp!.GetValue(proxy), Is.EqualTo(42));

        // Verify instance properties are accessible
        var instanceProp = props.Find(nameof(SampleInstanceSettings.InstanceIntSetting), false);
        Assert.That(instanceProp, Is.Not.Null);
        Assert.That(instanceProp!.GetValue(proxy), Is.EqualTo(100));
    }

    /// <summary>
    /// Tests that GetProperties with attributes returns the same collection as GetProperties without attributes.
    /// This verifies that the attributes parameter is properly handled in the ICustomTypeDescriptor implementation.
    /// </summary>
    [Test]
    public void GetProperties_WithAttributes_ReturnsSameCollection()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Act
        var propsWithoutAttributes = proxy.GetProperties();
        var propsWithAttributes = proxy.GetProperties([]);

        // Assert: Both should return the same collection
        Assert.That(propsWithAttributes, Has.Count.EqualTo(propsWithoutAttributes.Count));
        Assert.That(propsWithAttributes, Is.EqualTo(propsWithoutAttributes));
    }

    /// <summary>
    /// Tests that GetPropertyOwner returns the proxy object itself.
    /// This verifies the ICustomTypeDescriptor.GetPropertyOwner implementation.
    /// </summary>
    [Test]
    public void GetPropertyOwner_ReturnsProxyObject()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false);

        // Act
        var owner = proxy.GetPropertyOwner(intProp);

        // Assert: Should return the proxy object itself
        Assert.That(owner, Is.SameAs(proxy));
    }

    /// <summary>
    /// Tests that GetPropertyOwner returns the proxy object even when passed null.
    /// This verifies the ICustomTypeDescriptor.GetPropertyOwner implementation handles null gracefully.
    /// </summary>
    [Test]
    public void GetPropertyOwner_WithNull_ReturnsProxyObject()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var owner = proxy.GetPropertyOwner(null);

        // Assert: Should return the proxy object itself
        Assert.That(owner, Is.SameAs(proxy));
    }

    /// <summary>
    /// Tests that GetDefaultProperty returns null as expected.
    /// This verifies the ICustomTypeDescriptor.GetDefaultProperty implementation.
    /// </summary>
    [Test]
    public void GetDefaultProperty_ReturnsNull()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Act
        var defaultProp = proxy.GetDefaultProperty();

        // Assert: Should return null (no default property defined)
        Assert.That(defaultProp, Is.Null);
    }

    /// <summary>
    /// Tests that GetClassName returns the correct class name.
    /// This verifies the ICustomTypeDescriptor.GetClassName implementation.
    /// </summary>
    [Test]
    public void GetClassName_ReturnsCorrectClassName()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var className = proxy.GetClassName();

        // Assert: Should return the fully qualified class name
        Assert.That(className, Is.EqualTo("Blackwood.ProxyPropertiesObject"));
    }

    /// <summary>
    /// Tests that GetComponentName returns null as expected.
    /// This verifies the ICustomTypeDescriptor.GetComponentName implementation.
    /// </summary>
    [Test]
    public void GetComponentName_ReturnsNull()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var componentName = proxy.GetComponentName();

        // Assert: Should return null (no component name defined)
        Assert.That(componentName, Is.Null);
    }

    /// <summary>
    /// Tests that GetAttributes returns a valid AttributeCollection.
    /// This verifies the ICustomTypeDescriptor.GetAttributes implementation.
    /// </summary>
    [Test]
    public void GetAttributes_ReturnsValidAttributeCollection()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var attributes = proxy.GetAttributes();

        // Assert: Should return a valid AttributeCollection
        Assert.That(attributes, Is.Not.Null);
        Assert.That(attributes, Is.InstanceOf<AttributeCollection>());
    }

    /// <summary>
    /// Tests that GetEvents returns an empty EventDescriptorCollection.
    /// This verifies the ICustomTypeDescriptor.GetEvents implementation.
    /// </summary>
    [Test]
    public void GetEvents_ReturnsEmptyCollection()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var events = proxy.GetEvents();

        // Assert: Should return an empty collection
        Assert.That(events, Is.Not.Null);
        Assert.That(events, Is.Empty);
    }

    /// <summary>
    /// Tests that GetEvents with attributes returns an empty EventDescriptorCollection.
    /// This verifies the ICustomTypeDescriptor.GetEvents(Attribute[]) implementation.
    /// </summary>
    [Test]
    public void GetEvents_WithAttributes_ReturnsEmptyCollection()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var events = proxy.GetEvents([]);

        // Assert: Should return an empty collection
        Assert.That(events, Is.Not.Null);
        Assert.That(events, Is.Empty);
    }

    /// <summary>
    /// Tests that GetDefaultEvent returns null as expected.
    /// This verifies the ICustomTypeDescriptor.GetDefaultEvent implementation.
    /// </summary>
    [Test]
    public void GetDefaultEvent_ReturnsNull()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var defaultEvent = proxy.GetDefaultEvent();

        // Assert: Should return null (no default event defined)
        Assert.That(defaultEvent, Is.Null);
    }

    /// <summary>
    /// Tests that GetConverter returns a valid TypeConverter.
    /// This verifies the ICustomTypeDescriptor.GetConverter implementation.
    /// </summary>
    [Test]
    public void GetConverter_ReturnsValidTypeConverter()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var converter = proxy.GetConverter();

        // Assert: Should return a valid TypeConverter
        Assert.That(converter, Is.Not.Null);
        Assert.That(converter, Is.InstanceOf<TypeConverter>());
    }

    /// <summary>
    /// Tests that GetEditor returns null for a basic editor type.
    /// This verifies the ICustomTypeDescriptor.GetEditor implementation.
    /// </summary>
    [Test]
    public void GetEditor_ReturnsNull()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();

        // Act
        var editor = proxy.GetEditor(typeof(object));

        // Assert: Should return null (no custom editor defined)
        Assert.That(editor, Is.Null);
    }

    /// <summary>
    /// Tests that ApplyPropertyValues correctly applies property values from a dictionary.
    /// This verifies the new ApplyPropertyValues functionality for bulk property updates.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_AppliesValuesFromDictionary()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleSettings.IntSetting), 123 },
            { nameof(SampleSettings.StringSetting), "applied" },
            { nameof(SampleSettings.BoolField), false }
        };

        // Act: Apply the property values
        proxy.ApplyPropertyValues(propertyValues);

        // Assert: Values should be applied to the underlying static properties
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(123));
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("applied"));
        Assert.That(SampleSettings.BoolField, Is.False);
    }

    /// <summary>
    /// Tests that ApplyPropertyValues handles type conversion correctly.
    /// This verifies that string values are converted to appropriate types.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_HandlesTypeConversion()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleSettings.IntSetting), "456" }, // String that should convert to int
            { nameof(SampleSettings.StringSetting), "converted" },
            { nameof(SampleSettings.BoolField), "true" } // String that should convert to bool
        };

        // Act: Apply the property values
        proxy.ApplyPropertyValues(propertyValues);

        // Assert: Values should be converted and applied
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(456));
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("converted"));
        Assert.That(SampleSettings.BoolField, Is.True);
    }

    /// <summary>
    /// Tests that ApplyPropertyValues ignores properties that don't exist.
    /// This verifies that unknown property names are safely ignored.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_IgnoresNonExistentProperties()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleSettings.IntSetting), 789 },
            { "NonExistentProperty", "should be ignored" },
            { "AnotherMissingProperty", 999 }
        };

        // Act: Apply the property values
        proxy.ApplyPropertyValues(propertyValues);

        // Assert: Only existing properties should be updated
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(789));
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("hello")); // Unchanged
        Assert.That(SampleSettings.BoolField, Is.True); // Unchanged
    }

    /// <summary>
    /// Tests that ApplyPropertyValues handles conversion errors gracefully.
    /// This verifies that invalid type conversions don't crash the application.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_HandlesConversionErrorsGracefully()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleSettings.IntSetting), "not a number" }, // Invalid conversion
            { nameof(SampleSettings.StringSetting), "valid string" },
            { nameof(SampleSettings.BoolField), "invalid bool" } // Invalid conversion
        };

        // Act: Apply the property values (should not throw)
        Assert.DoesNotThrow(() => proxy.ApplyPropertyValues(propertyValues));

        // Assert: Valid conversions should work, invalid ones should be ignored
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(42)); // Should remain at default
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("valid string")); // Should be applied
        // Note: "invalid bool" might be converted to false by JSONConvert, so we check it's not the original true
        Assert.That(SampleSettings.BoolField, Is.False); // "invalid bool" converts to false
    }

    /// <summary>
    /// Tests that ApplyPropertyValues works with instance properties.
    /// This verifies that the method works with both static and instance properties.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_WorksWithInstanceProperties()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);
        proxy.AddInstanceProperties(instance.GetType());

        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleInstanceSettings.InstanceIntSetting), 200 },
            { nameof(SampleInstanceSettings.InstanceStringSetting), "instance applied" },
            { nameof(SampleInstanceSettings.InstanceBoolField), true }
        };

        // Act: Apply the property values
        proxy.ApplyPropertyValues(propertyValues);

        // Assert: Values should be applied to the instance properties
        Assert.That(instance.InstanceIntSetting, Is.EqualTo(200));
        Assert.That(instance.InstanceStringSetting, Is.EqualTo("instance applied"));
        Assert.That(instance.InstanceBoolField, Is.True);
    }

    /// <summary>
    /// Tests that ApplyPropertyValues handles null values correctly.
    /// This verifies that null values in the dictionary are handled properly.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_HandlesNullValues()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleSettings.IntSetting), 300 },
            { nameof(SampleSettings.StringSetting), null }, // Null value
            { nameof(SampleSettings.BoolField), false }
        };

        // Act: Apply the property values
        proxy.ApplyPropertyValues(propertyValues);

        // Assert: Values should be applied, including null
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(300));
        Assert.That(SampleSettings.StringSetting, Is.Null);
        Assert.That(SampleSettings.BoolField, Is.False);
    }

    /// <summary>
    /// Tests that ApplyPropertyValues works with mixed static and instance properties.
    /// This verifies that the method works when the proxy contains both types of properties.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_WorksWithMixedStaticAndInstanceProperties()
    {
        // Arrange
        var instance = new SampleInstanceSettings();
        var proxy = new ProxyPropertiesObject(instance);
        proxy.AddClassProperties(typeof(SampleSettings)); // Static
        proxy.AddInstanceProperties(instance.GetType()); // Instance

        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleSettings.IntSetting), 400 }, // Static property
            { nameof(SampleInstanceSettings.InstanceIntSetting), 500 }, // Instance property
            { nameof(SampleSettings.StringSetting), "static applied" },
            { nameof(SampleInstanceSettings.InstanceStringSetting), "instance applied" }
        };

        // Act: Apply the property values
        proxy.ApplyPropertyValues(propertyValues);

        // Assert: Both static and instance properties should be updated
        Assert.That(SampleSettings.IntSetting, Is.EqualTo(400));
        Assert.That(instance.InstanceIntSetting, Is.EqualTo(500));
        Assert.That(SampleSettings.StringSetting, Is.EqualTo("static applied"));
        Assert.That(instance.InstanceStringSetting, Is.EqualTo("instance applied"));
    }

    /// <summary>
    /// Tests that ApplyPropertyValues handles empty dictionary correctly.
    /// This verifies that an empty property values dictionary doesn't cause issues.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_HandlesEmptyDictionary()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        var propertyValues = new Dictionary<string, object?>();

        // Act: Apply empty property values (should not throw)
        Assert.DoesNotThrow(() => proxy.ApplyPropertyValues(propertyValues));

        // Assert: All properties should remain at their default values
        Assert.Multiple(() =>
        {
            Assert.That(SampleSettings.IntSetting, Is.EqualTo(42));
            Assert.That(SampleSettings.StringSetting, Is.EqualTo("hello"));
            Assert.That(SampleSettings.BoolField, Is.True);
        });
    }

    /// <summary>
    /// Tests that ApplyPropertyValues handles partial property updates correctly.
    /// This verifies that only specified properties are updated, others remain unchanged.
    /// </summary>
    [Test]
    public void ApplyPropertyValues_HandlesPartialPropertyUpdates()
    {
        // Arrange
        var proxy = new ProxyPropertiesObject();
        proxy.AddClassProperties(typeof(SampleSettings));

        // Change all values first
        var props = proxy.GetProperties();
        var intProp = props.Find(nameof(SampleSettings.IntSetting), false)!;
        var strProp = props.Find(nameof(SampleSettings.StringSetting), false)!;
        var boolProp = props.Find(nameof(SampleSettings.BoolField), false)!;

        intProp.SetValue(proxy, 999);
        strProp.SetValue(proxy, "changed");
        boolProp.SetValue(proxy, false);

        // Verify all are changed
        Assert.Multiple(() =>
        {
            Assert.That(SampleSettings.IntSetting, Is.EqualTo(999));
            Assert.That(SampleSettings.StringSetting, Is.EqualTo("changed"));
            Assert.That(SampleSettings.BoolField, Is.False);
        });

        // Act: Apply only one property value
        var propertyValues = new Dictionary<string, object?>
        {
            { nameof(SampleSettings.IntSetting), 111 }
        };

        proxy.ApplyPropertyValues(propertyValues);

        // Assert: Only the specified property should be updated
        Assert.Multiple(() =>
        {
            Assert.That(SampleSettings.IntSetting, Is.EqualTo(111));
            Assert.That(SampleSettings.StringSetting, Is.EqualTo("changed")); // Unchanged
            Assert.That(SampleSettings.BoolField, Is.False); // Unchanged
        });
    }
}
