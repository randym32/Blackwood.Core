namespace Blackwood.Core.Tests;

/// <summary>
/// Test suite for the ItemAttributes functionality in Blackwood.Core.
/// Tests cover property initialization, validation, cloning, and edge cases.
/// This test class validates the ItemAttributes class behavior in various scenarios.
/// </summary>
[TestFixture]
public class ItemAttributesTests
{
    #region Test Setup and Teardown

    /// <summary>
    /// Setup method that runs before each test to ensure a clean state.
    /// This method is called by NUnit before each test method execution.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // Ensure we start with a clean state for each test
        // This helps prevent test interference and ensures reliable test results
    }

    /// <summary>
    /// Teardown method that runs after each test to clean up resources.
    /// This method is called by NUnit after each test method execution.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Clean up any resources that might have been created during the test
        // This ensures that tests don't interfere with each other
    }

    #endregion

    #region Constructor and Initialization Tests

    /// <summary>
    /// Tests that the ItemAttributes constructor initializes all properties correctly.
    /// This verifies the default initialization behavior.
    /// </summary>
    [Test]
    public void ItemAttributes_Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange & Act
        var attributes = new ItemAttributes();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributes.Name, Is.EqualTo("Untitled"), "Default name should be 'Untitled'");
            Assert.That(attributes.Guid, Is.Not.EqualTo(Guid.Empty), "Guid should be generated");
            Assert.That(attributes.CreatedUTC, Is.Not.EqualTo(DateTime.MinValue), "CreatedUTC should be set");
            Assert.That(attributes.LastModifiedUTC, Is.Not.EqualTo(DateTime.MinValue), "LastModifiedUTC should be set");
            Assert.That(attributes.Version, Is.Not.Null, "Version should be initialized");
            Assert.That(attributes.Version.Major, Is.Zero, "Default version major should be 0");
            Assert.That(attributes.Version.Minor, Is.Zero, "Default version minor should be 0");
            Assert.That(attributes.Version.Build, Is.Zero, "Default version build should be 0");
            Assert.That(attributes.Version.Revision, Is.Zero, "Default version revision should be 0");
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes constructor initializes nullable properties as null.
    /// This verifies that optional properties start as null.
    /// </summary>
    [Test]
    public void ItemAttributes_Constructor_InitializesNullablePropertiesAsNull()
    {
        // Arrange & Act
        var attributes = new ItemAttributes();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributes.Author, Is.Null, "Author should start as null");
            Assert.That(attributes.Description, Is.Null, "Description should start as null");
            Assert.That(attributes.Link, Is.Null, "Link should start as null");
            Assert.That(attributes.Documentation, Is.Null, "Documentation should start as null");
            Assert.That(attributes.Comments, Is.Null, "Comments should start as null");
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes constructor generates unique GUIDs.
    /// This verifies that each instance gets a unique identifier.
    /// </summary>
    [Test]
    public void ItemAttributes_Constructor_GeneratesUniqueGuids()
    {
        // Arrange & Act
        var attributes1 = new ItemAttributes();
        var attributes2 = new ItemAttributes();

        // Assert
        Assert.That(attributes1.Guid, Is.Not.EqualTo(attributes2.Guid), "Each instance should have a unique Guid");
    }

    /// <summary>
    /// Tests that the ItemAttributes constructor sets timestamps close to creation time.
    /// This verifies that timestamps are set appropriately.
    /// </summary>
    [Test]
    public void ItemAttributes_Constructor_SetsTimestampsCorrectly()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var attributes = new ItemAttributes();

        // Arrange
        var afterCreation = DateTime.UtcNow;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributes.CreatedUTC, Is.GreaterThanOrEqualTo(beforeCreation), "CreatedUTC should be after creation start");
            Assert.That(attributes.CreatedUTC, Is.LessThanOrEqualTo(afterCreation), "CreatedUTC should be before creation end");
            Assert.That(attributes.LastModifiedUTC, Is.GreaterThanOrEqualTo(beforeCreation), "LastModifiedUTC should be after creation start");
            Assert.That(attributes.LastModifiedUTC, Is.LessThanOrEqualTo(afterCreation), "LastModifiedUTC should be before creation end");
        }
    }

    #endregion

    #region Name Property Tests

    /// <summary>
    /// Tests that the Name property can be set and retrieved correctly.
    /// This verifies the basic functionality of the Name property.
    /// </summary>
    [Test]
    public void Name_ValidValue_CanBeSetAndRetrieved()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var expectedName = "Test Item";

        // Act
        attributes.Name = expectedName;

        // Assert
        Assert.That(attributes.Name, Is.EqualTo(expectedName), "Name should be set and retrieved correctly");
    }

    /// <summary>
    /// Tests that the Name property trims whitespace from values.
    /// This verifies that leading and trailing whitespace is removed.
    /// </summary>
    [Test]
    public void Name_WithWhitespace_TrimsCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var nameWithWhitespace = "  Test Item  ";
        var expectedName = "Test Item";

        // Act
        attributes.Name = nameWithWhitespace;

        // Assert
        Assert.That(attributes.Name, Is.EqualTo(expectedName), "Name should trim whitespace correctly");
    }

    /// <summary>
    /// Tests that the Name property handles null values by setting to "Untitled".
    /// This verifies the null handling behavior.
    /// </summary>
    [Test]
    public void Name_NullValue_SetsToUntitled()
    {
        // Arrange
        var attributes = new ItemAttributes
        {
            // Act - Set Name to null
            Name = null!
        };

        // Assert
        Assert.That(attributes.Name, Is.EqualTo("Untitled"), "Name should be set to 'Untitled' when null is provided");
    }

    /// <summary>
    /// Tests that the Name property handles empty string by setting to "Untitled".
    /// This verifies the empty string handling behavior.
    /// </summary>
    [Test]
    public void Name_EmptyString_SetsToUntitled()
    {
        // Arrange & Act
        var attributes = new ItemAttributes
        {
            Name = ""
        };

        // Assert
        Assert.That(attributes.Name, Is.EqualTo("Untitled"), "Name should be set to 'Untitled' when empty string is provided");
    }

    /// <summary>
    /// Tests that the Name property handles whitespace-only string by setting to "Untitled".
    /// This verifies the whitespace-only string handling behavior.
    /// </summary>
    [Test]
    public void Name_WhitespaceOnlyString_SetsToUntitled()
    {
        // Arrange & Act
        var attributes = new ItemAttributes
        {
            Name = "   "
        };

        // Assert
        Assert.That(attributes.Name, Is.EqualTo("Untitled"), "Name should be set to 'Untitled' when whitespace-only string is provided");
    }

    /// <summary>
    /// Tests that the Name property handles various whitespace scenarios.
    /// This verifies whitespace handling.
    /// </summary>
    [Test]
    public void Name_VariousWhitespaceScenarios_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var testCases = new[]
        {
            ("\t", "Untitled"),
            ("\n", "Untitled"),
            ("\r", "Untitled"),
            ("\t\n\r", "Untitled"),
            ("  \t  ", "Untitled"),
            ("Test\tName", "Test\tName"), // Internal whitespace should be preserved
            ("Test\nName", "Test\nName"), // Internal whitespace should be preserved
            ("Test\rName", "Test\rName")  // Internal whitespace should be preserved
        };

        foreach (var (input, expected) in testCases)
        {
            // Act
            attributes.Name = input;

            // Assert
            Assert.That(attributes.Name, Is.EqualTo(expected), $"Name '{input}' should result in '{expected}'");
        }
    }

    /// <summary>
    /// Tests that the Name property handles very long strings correctly.
    /// This verifies that long strings are handled properly.
    /// </summary>
    [Test]
    public void Name_VeryLongString_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var longName = new string('A', 10000);

        // Act
        attributes.Name = longName;

        // Assert
        Assert.That(attributes.Name, Is.EqualTo(longName), "Very long name should be handled correctly");
    }

    /// <summary>
    /// Tests that the Name property handles Unicode characters correctly.
    /// This verifies that international characters are handled properly.
    /// </summary>
    [Test]
    public void Name_UnicodeCharacters_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var unicodeNames = new[]
        {
            "файл",           // Cyrillic
            "文件",            // Chinese
            "ファイル",         // Japanese
            "ملف",            // Arabic
            "αρχείο",         // Greek
            "archivo",        // Spanish
            "Test 测试",       // Mixed
            "Test 🚀 Name"    // With emoji
        };

        foreach (var unicodeName in unicodeNames)
        {
            // Act
            attributes.Name = unicodeName;

            // Assert
            Assert.That(attributes.Name, Is.EqualTo(unicodeName), $"Unicode name '{unicodeName}' should be handled correctly");
        }
    }

    #endregion

    #region Other Property Tests

    /// <summary>
    /// Tests that the Author property can be set and retrieved correctly.
    /// This verifies the basic functionality of the Author property.
    /// </summary>
    [Test]
    public void Author_CanBeSetAndRetrieved()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var expectedAuthor = "John Doe";

        // Act
        attributes.Author = expectedAuthor;

        // Assert
        Assert.That(attributes.Author, Is.EqualTo(expectedAuthor), "Author should be set and retrieved correctly");
    }

    /// <summary>
    /// Tests that the Author property can be set to null.
    /// This verifies that nullable string properties work correctly.
    /// </summary>
    [Test]
    public void Author_CanBeSetToNull()
    {
        // Arrange
        var attributes = new ItemAttributes
        {
            Author = "Initial Author"
        };

        // Act
        attributes.Author = null;

        // Assert
        Assert.That(attributes.Author, Is.Null, "Author should be null after being set to null");
    }

    /// <summary>
    /// Tests that the Version property can be set and retrieved correctly.
    /// This verifies the basic functionality of the Version property.
    /// </summary>
    [Test]
    public void Version_CanBeSetAndRetrieved()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var expectedVersion = new Version(1, 2, 3, 4);

        // Act
        attributes.Version = expectedVersion;

        // Assert
        Assert.That(attributes.Version, Is.EqualTo(expectedVersion), "Version should be set and retrieved correctly");
    }

    /// <summary>
    /// Tests that the Version property can be set to null.
    /// This verifies that nullable Version properties work correctly.
    /// </summary>
    [Test]
    public void Version_CanBeSetToNull()
    {
        // Arrange
        var attributes = new ItemAttributes
        {
            Version = new Version(1, 0, 0, 0)
        };

        // Act
        attributes.Version = null!;

        // Assert
        Assert.That(attributes.Version, Is.Null, "Version should be null after being set to null");
    }

    /// <summary>
    /// Tests that the Description property can be set and retrieved correctly.
    /// This verifies the basic functionality of the Description property.
    /// </summary>
    [Test]
    public void Description_CanBeSetAndRetrieved()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var expectedDescription = "This is a test description";

        // Act
        attributes.Description = expectedDescription;

        // Assert
        Assert.That(attributes.Description, Is.EqualTo(expectedDescription), "Description should be set and retrieved correctly");
    }

    /// <summary>
    /// Tests that the Link property can be set and retrieved correctly.
    /// This verifies the basic functionality of the Link property.
    /// </summary>
    [Test]
    public void Link_CanBeSetAndRetrieved()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var expectedLink = new Uri("https://example.com");

        // Act
        attributes.Link = expectedLink;

        // Assert
        Assert.That(attributes.Link, Is.EqualTo(expectedLink), "Link should be set and retrieved correctly");
    }

    /// <summary>
    /// Tests that the Documentation property can be set and retrieved correctly.
    /// This verifies the basic functionality of the Documentation property.
    /// </summary>
    [Test]
    public void Documentation_CanBeSetAndRetrieved()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var expectedDocumentation = new Uri("https://docs.example.com");

        // Act
        attributes.Documentation = expectedDocumentation;

        // Assert
        Assert.That(attributes.Documentation, Is.EqualTo(expectedDocumentation), "Documentation should be set and retrieved correctly");
    }

    /// <summary>
    /// Tests that the Comments property can be set and retrieved correctly.
    /// This verifies the basic functionality of the Comments property.
    /// </summary>
    [Test]
    public void Comments_CanBeSetAndRetrieved()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var expectedComments = "These are test comments";

        // Act
        attributes.Comments = expectedComments;

        // Assert
        Assert.That(attributes.Comments, Is.EqualTo(expectedComments), "Comments should be set and retrieved correctly");
    }

    /// <summary>
    /// Tests that all nullable properties can be set to null.
    /// This verifies null handling.
    /// </summary>
    [Test]
    public void NullableProperties_CanAllBeSetToNull()
    {
        // Arrange
        var attributes = new ItemAttributes
        {
            Author = "Test Author",
            Description = "Test Description",
            Link = new Uri("https://example.com"),
            Documentation = new Uri("https://docs.example.com"),
            Comments = "Test Comments"
        };

        // Act
        attributes.Author = null;
        attributes.Description = null;
        attributes.Link = null;
        attributes.Documentation = null;
        attributes.Comments = null;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributes.Author, Is.Null, "Author should be null");
            Assert.That(attributes.Description, Is.Null, "Description should be null");
            Assert.That(attributes.Link, Is.Null, "Link should be null");
            Assert.That(attributes.Documentation, Is.Null, "Documentation should be null");
            Assert.That(attributes.Comments, Is.Null, "Comments should be null");
        }
    }

    #endregion

    #region Clone Method Tests

    /// <summary>
    /// Tests that the Clone method creates a copy with the same property values.
    /// This verifies the basic cloning functionality.
    /// </summary>
    [Test]
    public void Clone_CreatesCopyWithSameValues()
    {
        // Arrange
        var original = new ItemAttributes
        {
            Name = "Test Item",
            Author = "Test Author",
            Description = "Test Description",
            Comments = "Test Comments",
            Version = new Version(1, 2, 3, 4),
            Link = new Uri("https://example.com"),
            Documentation = new Uri("https://docs.example.com")
        };

        // Act
        var cloned = original.Clone();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(cloned.Name, Is.EqualTo(original.Name), "Cloned name should match original");
            Assert.That(cloned.Author, Is.EqualTo(original.Author), "Cloned author should match original");
            Assert.That(cloned.Description, Is.EqualTo(original.Description), "Cloned description should match original");
            Assert.That(cloned.Comments, Is.EqualTo(original.Comments), "Cloned comments should match original");
            Assert.That(cloned.Version, Is.EqualTo(original.Version), "Cloned version should match original");
            Assert.That(cloned.CreatedUTC, Is.EqualTo(original.CreatedUTC), "Cloned CreatedUTC should match original");
            Assert.That(cloned.LastModifiedUTC, Is.EqualTo(original.LastModifiedUTC), "Cloned LastModifiedUTC should match original");
        }
    }

    /// <summary>
    /// Tests that the Clone method creates a copy with different Guid.
    /// This verifies that the cloned instance gets a new unique identifier.
    /// </summary>
    [Test]
    public void Clone_CreatesCopyWithDifferentGuid()
    {
        // Arrange
        var original = new ItemAttributes();

        // Act
        var cloned = original.Clone();

        // Assert
        Assert.That(cloned.Guid, Is.Not.EqualTo(original.Guid), "Cloned instance should have a different Guid");
    }

    /// <summary>
    /// Tests that the Clone method handles null values correctly.
    /// This verifies that null properties are cloned correctly.
    /// </summary>
    [Test]
    public void Clone_WithNullValues_HandlesCorrectly()
    {
        // Arrange
        var original = new ItemAttributes
        {
            Author = null,
            Description = null,
            Link = null,
            Documentation = null,
            Comments = null
        };

        // Act
        var cloned = original.Clone();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(cloned.Author, Is.Null, "Cloned author should be null");
            Assert.That(cloned.Description, Is.Null, "Cloned description should be null");
            Assert.That(cloned.Link, Is.Null, "Cloned link should be null");
            Assert.That(cloned.Documentation, Is.Null, "Cloned documentation should be null");
            Assert.That(cloned.Comments, Is.Null, "Cloned comments should be null");
            Assert.That(cloned.Version, Is.EqualTo(original.Version), "Cloned version should match original");
        }
    }

    /// <summary>
    /// Tests that the Clone method creates new Uri instances for Link and Documentation.
    /// This verifies that reference types are properly cloned.
    /// </summary>
    [Test]
    public void Clone_CreatesNewUriInstances()
    {
        // Arrange
        var original = new ItemAttributes
        {
            Link = new Uri("https://example.com"),
            Documentation = new Uri("https://docs.example.com")
        };

        // Act
        var cloned = original.Clone();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(cloned.Link, Is.Not.SameAs(original.Link), "Cloned link should be a different Uri instance");
            Assert.That(cloned.Documentation, Is.Not.SameAs(original.Documentation), "Cloned documentation should be a different Uri instance");
            Assert.That(cloned.Link, Is.EqualTo(original.Link), "Cloned link should have the same value as original");
            Assert.That(cloned.Documentation, Is.EqualTo(original.Documentation), "Cloned documentation should have the same value as original");
        }
    }

    /// <summary>
    /// Tests that the Clone method creates new Version instances.
    /// This verifies that Version objects are properly cloned.
    /// </summary>
    [Test]
    public void Clone_CreatesNewVersionInstance()
    {
        // Arrange
        var original = new ItemAttributes
        {
            Version = new Version(1, 2, 3, 4)
        };

        // Act
        var cloned = original.Clone();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(cloned.Version, Is.Not.SameAs(original.Version), "Cloned version should be a different Version instance");
            Assert.That(cloned.Version, Is.EqualTo(original.Version), "Cloned version should have the same value as original");
        }
    }

    /// <summary>
    /// Tests that the Clone method creates independent copies.
    /// This verifies that changes to the clone don't affect the original.
    /// </summary>
    [Test]
    public void Clone_CreatesIndependentCopies()
    {
        // Arrange
        var original = new ItemAttributes
        {
            Name = "Original Name",
            Author = "Original Author",
            Description = "Original Description"
        };

        // Act
        var cloned = original.Clone();
        cloned.Name = "Cloned Name";
        cloned.Author = "Cloned Author";
        cloned.Description = "Cloned Description";

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(original.Name, Is.EqualTo("Original Name"), "Original name should not be affected by clone changes");
            Assert.That(original.Author, Is.EqualTo("Original Author"), "Original author should not be affected by clone changes");
            Assert.That(original.Description, Is.EqualTo("Original Description"), "Original description should not be affected by clone changes");
            Assert.That(cloned.Name, Is.EqualTo("Cloned Name"), "Cloned name should be changed");
            Assert.That(cloned.Author, Is.EqualTo("Cloned Author"), "Cloned author should be changed");
            Assert.That(cloned.Description, Is.EqualTo("Cloned Description"), "Cloned description should be changed");
        }
    }

    #endregion

    #region Edge Case Tests

    /// <summary>
    /// Tests that the ItemAttributes handles very long string values correctly.
    /// This verifies that long strings are handled properly in all string properties.
    /// </summary>
    [Test]
    public void ItemAttributes_VeryLongStringValues_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var longString = new string('A', 50000);

        // Act
        attributes.Name = longString;
        attributes.Author = longString;
        attributes.Description = longString;
        attributes.Comments = longString;

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributes.Name, Is.EqualTo(longString), "Very long name should be handled correctly");
            Assert.That(attributes.Author, Is.EqualTo(longString), "Very long author should be handled correctly");
            Assert.That(attributes.Description, Is.EqualTo(longString), "Very long description should be handled correctly");
            Assert.That(attributes.Comments, Is.EqualTo(longString), "Very long comments should be handled correctly");
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes handles special characters in string values correctly.
    /// This verifies that special characters are handled properly.
    /// </summary>
    [Test]
    public void ItemAttributes_SpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var specialStrings = new[]
        {
            "Test with special chars: !@#$%^&*()",
            "Test with quotes: \"double\" and 'single'",
            "Test with backslashes: \\ and forward slashes: /",
            "Test with newlines:\nLine 1\nLine 2",
            "Test with tabs:\tTab1\tTab2",
            "Test with carriage returns:\rCarriage1\rCarriage2"
        };

        foreach (var specialString in specialStrings)
        {
            // Act
            attributes.Name = specialString;
            attributes.Author = specialString;
            attributes.Description = specialString;
            attributes.Comments = specialString;

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(attributes.Name, Is.EqualTo(specialString), $"Special characters in name should be handled correctly: {specialString}");
                Assert.That(attributes.Author, Is.EqualTo(specialString), $"Special characters in author should be handled correctly: {specialString}");
                Assert.That(attributes.Description, Is.EqualTo(specialString), $"Special characters in description should be handled correctly: {specialString}");
                Assert.That(attributes.Comments, Is.EqualTo(specialString), $"Special characters in comments should be handled correctly: {specialString}");
            }
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes handles various Version formats correctly.
    /// This verifies that different Version constructors work properly.
    /// </summary>
    [Test]
    public void ItemAttributes_VariousVersionFormats_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var versions = new[]
        {
            new Version(1, 0),
            new Version(1, 0, 0),
            new Version(1, 0, 0, 0),
            new Version(2, 1, 3, 4),
            new Version(0, 0, 0, 0)
        };

        foreach (var version in versions)
        {
            // Act
            attributes.Version = version;

            // Assert
            Assert.That(attributes.Version, Is.EqualTo(version), $"Version {version} should be handled correctly");
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes handles various URI formats correctly.
    /// This verifies that different URI formats work properly.
    /// </summary>
    [Test]
    public void ItemAttributes_VariousUriFormats_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var uris = new[]
        {
            new Uri("https://example.com"),
            new Uri("http://example.com"),
            new Uri("ftp://example.com"),
            new Uri("file:///path/to/file"),
            new Uri("mailto:test@example.com"),
            new Uri("https://example.com/path?query=value#fragment")
        };

        foreach (var uri in uris)
        {
            // Act
            attributes.Link = uri;
            attributes.Documentation = uri;

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(attributes.Link, Is.EqualTo(uri), $"URI {uri} should be handled correctly for Link");
                Assert.That(attributes.Documentation, Is.EqualTo(uri), $"URI {uri} should be handled correctly for Documentation");
            }
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes handles rapid property changes correctly.
    /// This verifies that rapid changes don't cause issues.
    /// </summary>
    [Test]
    public void ItemAttributes_RapidPropertyChanges_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();

        // Act - Rapid changes
        for (int i = 0; i < 1000; i++)
        {
            attributes.Name = $"Name {i}";
            attributes.Author = $"Author {i}";
            attributes.Description = $"Description {i}";
            attributes.Comments = $"Comments {i}";
        }

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributes.Name, Is.EqualTo("Name 999"), "Final name should be correct after rapid changes");
            Assert.That(attributes.Author, Is.EqualTo("Author 999"), "Final author should be correct after rapid changes");
            Assert.That(attributes.Description, Is.EqualTo("Description 999"), "Final description should be correct after rapid changes");
            Assert.That(attributes.Comments, Is.EqualTo("Comments 999"), "Final comments should be correct after rapid changes");
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes handles concurrent access correctly.
    /// This verifies that the class is thread-safe for basic operations.
    /// </summary>
    [Test]
    public void ItemAttributes_ConcurrentAccess_HandlesCorrectly()
    {
        // Arrange
        var attributes = new ItemAttributes();
        var tasks = new Task[5];

        // Act
        tasks[0] = Task.Run(() => attributes.Name = "Concurrent Name");
        tasks[1] = Task.Run(() => attributes.Author = "Concurrent Author");
        tasks[2] = Task.Run(() => attributes.Description = "Concurrent Description");
        tasks[3] = Task.Run(() => attributes.Comments = "Concurrent Comments");
        tasks[4] = Task.Run(() => attributes.Version = new Version(1, 0, 0, 0));

        Task.WaitAll(tasks);

        // Assert - At least one of the concurrent operations should have succeeded
        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributes.Name, Is.Not.Null, "Name should be set by concurrent operation");
            Assert.That(attributes.Author, Is.Not.Null, "Author should be set by concurrent operation");
            Assert.That(attributes.Description, Is.Not.Null, "Description should be set by concurrent operation");
            Assert.That(attributes.Comments, Is.Not.Null, "Comments should be set by concurrent operation");
            Assert.That(attributes.Version, Is.Not.Null, "Version should be set by concurrent operation");
        }
    }

    #endregion

    #region Integration Tests

    /// <summary>
    /// Tests the complete workflow of ItemAttributes creation, modification, and cloning.
    /// This verifies the end-to-end functionality of the ItemAttributes system.
    /// </summary>
    [Test]
    public void ItemAttributes_CompleteWorkflow_WorksCorrectly()
    {
        // Arrange
        var original = new ItemAttributes
        {
            Name = "Complete Test Item",
            Author = "Test Author",
            Description = "This is a complete test of the ItemAttributes workflow",
            Comments = "These are test comments for the workflow",
            Version = new Version(1, 0, 0, 0),
            Link = new Uri("https://example.com/test"),
            Documentation = new Uri("https://docs.example.com/test")
        };

        // Act - Clone the item
        var cloned = original.Clone();

        // Act - Modify the clone
        cloned.Name = "Modified Clone";
        cloned.Author = "Modified Author";

        using (Assert.EnterMultipleScope())
        {
            // Assert - Original should be unchanged
            Assert.That(original.Name, Is.EqualTo("Complete Test Item"), "Original name should be unchanged");
            Assert.That(original.Author, Is.EqualTo("Test Author"), "Original author should be unchanged");

            // Assert - Clone should have modified values
            Assert.That(cloned.Name, Is.EqualTo("Modified Clone"), "Cloned name should be modified");
            Assert.That(cloned.Author, Is.EqualTo("Modified Author"), "Cloned author should be modified");

            // Assert - Other properties should be the same
            Assert.That(cloned.Description, Is.EqualTo(original.Description), "Cloned description should match original");
            Assert.That(cloned.Comments, Is.EqualTo(original.Comments), "Cloned comments should match original");
            Assert.That(cloned.Version, Is.EqualTo(original.Version), "Cloned version should match original");
            Assert.That(cloned.Link, Is.EqualTo(original.Link), "Cloned link should match original");
            Assert.That(cloned.Documentation, Is.EqualTo(original.Documentation), "Cloned documentation should match original");
        }
    }

    /// <summary>
    /// Tests that the ItemAttributes works correctly in realistic usage scenarios.
    /// This verifies that the system works with typical attribute patterns.
    /// </summary>
    [Test]
    public void ItemAttributes_RealisticUsageScenario_WorksCorrectly()
    {
        // Arrange & Act - Set realistic project attributes
        var projectAttributes = new ItemAttributes
        {
            Name = "My Awesome Project",
            Author = "John Developer",
            Description = "A project management system with advanced features",
            Comments = "This project was created to solve complex business problems",
            Version = new Version(2, 1, 0, 0),
            Link = new Uri("https://github.com/user/awesome-project"),
            Documentation = new Uri("https://docs.awesome-project.com")
        };

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(projectAttributes.Name, Is.EqualTo("My Awesome Project"), "Realistic project name should be set correctly");
            Assert.That(projectAttributes.Author, Is.EqualTo("John Developer"), "Realistic project author should be set correctly");
            Assert.That(projectAttributes.Description, Is.EqualTo("A project management system with advanced features"), "Realistic project description should be set correctly");
            Assert.That(projectAttributes.Comments, Is.EqualTo("This project was created to solve complex business problems"), "Realistic project comments should be set correctly");
            Assert.That(projectAttributes.Version, Is.EqualTo(new Version(2, 1, 0, 0)), "Realistic project version should be set correctly");
            Assert.That(projectAttributes.Link, Is.EqualTo(new Uri("https://github.com/user/awesome-project")), "Realistic project link should be set correctly");
            Assert.That(projectAttributes.Documentation, Is.EqualTo(new Uri("https://docs.awesome-project.com")), "Realistic project documentation should be set correctly");
        }
    }

    #endregion
}
