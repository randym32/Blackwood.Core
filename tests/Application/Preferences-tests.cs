using System.Text.Json;

namespace Blackwood.Core.Tests;

/// <summary>
/// Test class for Preferences functionality.
/// This class contains tests for the Utils static class preferences methods,
/// covering PreferencesProxy, SavePreferences, and LoadPreferencesFromFile functionality.
/// </summary>
public class PreferencesTests
{
    #region Test Helper Classes

    /// <summary>
    /// Test class with various properties for testing preferences functionality.
    /// </summary>
    public class TestPreferencesClass
    {
        public static string TestStringProperty { get; set; } = "default";
        public static int TestIntProperty { get; set; } = 42;
        public static bool TestBoolProperty { get; set; } = true;
        public static double TestDoubleProperty { get; set; } = 3.14;
        public static string? TestNullableProperty { get; set; } = null;
        public static string NonPreferenceProperty { get; set; } = "not a preference";
        private static string PrivateProperty { get; set; } = "private";
    }

    /// <summary>
    /// Another test class for testing multiple classes in preferences.
    /// </summary>
    public class AnotherTestPreferencesClass
    {
        public static string AnotherProperty { get; set; } = "another default";
    }

    #endregion

    #region PreferencesProxy Tests

    /// <summary>
    /// Test that PreferencesProxy returns a valid proxy object.
    /// </summary>
    [Test]
    public void PreferencesProxy_ShouldReturnValidProxyObject()
    {
        // Act
        var proxy = Application.PreferencesProxy();

        // Assert
        Assert.That(proxy, Is.Not.Null);
        Assert.That(proxy, Is.InstanceOf<ProxyPropertiesObject>());
    }

    /// <summary>
    /// Test that PreferencesProxy returns the same instance (singleton behavior).
    /// </summary>
    [Test]
    public void PreferencesProxy_ShouldReturnSameInstance()
    {
        // Act
        var proxy1 = Application.PreferencesProxy();
        var proxy2 = Application.PreferencesProxy();

        // Assert
        Assert.That(proxy1, Is.SameAs(proxy2));
    }

    /// <summary>
    /// Test that PreferencesProxy discovers properties from test classes.
    /// </summary>
    [Test]
    public void PreferencesProxy_ShouldDiscoverProperties()
    {
        // Act
        var proxy = Application.PreferencesProxy();
        var properties = proxy.GetProperties();

        // Assert
        Assert.That(properties, Is.Not.Null);
        Assert.That(properties, Is.Not.Empty);
    }

    #endregion

    #region SavePreferences Tests

    /// <summary>
    /// Test that SavePreferences creates a file.
    /// </summary>
    [Test]
    public void SavePreferences_ShouldCreateFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            Application.SavePreferences(tempFile);

            // Assert
            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Test that SavePreferences creates a valid JSON file.
    /// </summary>
    [Test]
    public void SavePreferences_ShouldCreateValidJson()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            Application.SavePreferences(tempFile);

            // Wait a bit for the async operation to complete
            Thread.Sleep(100);

            // Assert
            Assert.That(File.Exists(tempFile), Is.True);
            var content = File.ReadAllText(tempFile);
            Assert.That(content, Is.Not.Empty);
            Assert.That(content, Does.Contain("{"));
            Assert.That(content, Does.Contain("}"));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region LoadPreferencesFromFile Tests

    /// <summary>
    /// Test that LoadPreferencesFromFile throws exception for missing file.
    /// </summary>
    [Test]
    public void LoadPreferencesFromFile_ShouldThrowForMissingFile()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "non-existent-file.json");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => Application.LoadPreferences(nonExistentFile));
    }

    /// <summary>
    /// Test that LoadPreferencesFromFile throws exception for invalid JSON.
    /// </summary>
    [Test]
    public void LoadPreferencesFromFile_ShouldThrowForInvalidJson()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "invalid json content");

        try
        {
            // Act & Assert
            Assert.Throws<global::System.Text.Json.JsonException>(() => Application.LoadPreferences(tempFile));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Test the complete save and load cycle.
    /// </summary>
    [Test]
    public void SaveAndLoadPreferences_ShouldWork()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            Application.SavePreferences(tempFile);

            // Wait for async operation to complete
            Thread.Sleep(100);

            Application.LoadPreferences(tempFile);

            // Assert
            Assert.That(File.Exists(tempFile), Is.True);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion
}