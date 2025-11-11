namespace Blackwood.Core.Tests;


/// <summary>
/// Test class for NameHelper functionality.
/// for converting C# names to labels and fitting text to specified widths.
/// </summary>
public class NameHelperTests
{
    #region ConvertCNameToLabel Tests

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly converts PascalCase names to readable labels.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithPascalCase_ConvertsCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("TestProperty", "Test property"),
            ("MyVariableName", "My variable name"),
            ("SomeLongPropertyName", "Some long property name"),
            ("SimpleName", "Simple name"),
            ("A", "A")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles camelCase names.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithCamelCase_ConvertsCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("testProperty", "Test property"),
            ("myVariableName", "My variable name"),
            ("someLongPropertyName", "Some long property name"),
            ("simpleName", "Simple name"),
            ("a", "A")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles names with underscores.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithUnderscores_ConvertsCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("test_property", "Test property"),
            ("my_variable_name", "My variable name"),
            ("some_long_property_name", "Some long property name"),
            ("simple_name", "Simple name"),
            ("a_b", "A b"),
            ("test_property_name", "Test property name")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles mixed case with underscores.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithMixedCaseAndUnderscores_ConvertsCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("Test_Property", "Test property"),
            ("My_Variable_Name", "My variable name"),
            ("Some_Long_Property_Name", "Some long property name"),
            ("Simple_Name", "Simple name"),
            ("Test_Property_Name", "Test property name"),
            ("camelCase_With_Underscore", "Camel case with underscore")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly removes Hungarian notation prefixes.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithHungarianNotation_RemovesPrefixes()
    {
        // Arrange
        var testCases = new[]
        {
            ("gTestProperty", "Test property"),
            ("mMyVariable", "My variable"),
            ("gSomeLongPropertyName", "Some long property name"),
            ("mSimpleName", "Simple name"),
            ("gA", "A"),
            ("mB", "B")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles Hungarian notation with underscores.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithHungarianNotationAndUnderscores_RemovesPrefixes()
    {
        // Arrange
        var testCases = new[]
        {
            ("g_test_property", "Test property"),
            ("m_my_variable", "My variable"),
            ("g_some_long_property_name", "Some long property name"),
            ("m_simple_name", "Simple name"),
            ("g_a", "A"),
            ("m_b", "B")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles Hungarian notation with mixed case.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithHungarianNotationAndMixedCase_RemovesPrefixes()
    {
        // Arrange
        var testCases = new[]
        {
            ("gTest_Property", "Test property"),
            ("mMy_Variable", "My variable"),
            ("gSome_Long_Property_Name", "Some long property name"),
            ("mSimple_Name", "Simple name"),
            ("gA_B", "A b"),
            ("mB_C", "B c")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles empty and null strings.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithEmptyOrNullStrings_HandlesCorrectly()
    {
        // Arrange & Act & Assert
        Assert.That(Utils.ConvertCNameToLabel(""), Is.EqualTo(""), "Empty string should return empty string");
        Assert.Throws<ArgumentNullException>(() => Utils.ConvertCNameToLabel(null!), "Null string should throw ArgumentNullException");
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles single character names.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithSingleCharacters_HandlesCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("A", "A"),
            ("a", "A"),
            ("g", ""), // Hungarian notation prefix only
            ("m", ""), // Hungarian notation prefix only
            ("G", ""), // Uppercase G is also treated as Hungarian notation prefix
            ("M", "")  // Uppercase M is also treated as Hungarian notation prefix
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles names with numbers.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithNumbers_HandlesCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("TestProperty1", "Test property1"),
            ("MyVariable2Name", "My variable2 name"), // Numbers do trigger word boundaries
            ("Some3Long4Property5Name", "Some3 long4 property5 name"),
            ("Simple1Name", "Simple1 name"),
            ("Test1_Property2", "Test1 property2"),
            ("gTest1Property", "Test1 property") // Hungarian notation with numbers
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    /// <summary>
    /// Tests that ConvertCNameToLabel correctly handles names with special characters.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("Test$Property", "Test$property"), // Special characters don't trigger word boundaries
            ("My@Variable", "My@variable"),
            ("Some#Long$Property%Name", "Some#long$property%name"), // Special characters don't trigger word boundaries
            ("Simple&Name", "Simple&name"),
            ("Test_Property@Name", "Test property@name")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    #endregion

    #region Edge Cases and Integration Tests

    /// <summary>
    /// Tests that ConvertCNameToLabel handles complex real-world naming scenarios.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithComplexRealWorldNames_HandlesCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("XMLHttpRequest", "Xml http request"), // Consecutive uppercase letters are split at word boundaries
            ("HTMLParser", "Html parser"), // Consecutive uppercase letters are split at word boundaries
            ("SQLConnection", "Sql connection"),
            ("HTTPResponse", "Http response"),
            ("JSONSerializer", "Json serializer"),
            ("gXMLHttpRequest", "Xml http request"), // Hungarian notation
            ("mHTMLParser", "Html parser"), // Hungarian notation
            ("TestXMLHttpRequest", "Test xml http request"), // Consecutive uppercase letters are split at word boundaries
            ("MyHTMLParser", "My html parser") // Consecutive uppercase letters are split at word boundaries
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }


    /// <summary>
    /// Tests that ConvertCNameToLabel handles names with consecutive uppercase letters correctly.
    /// </summary>
    [Test]
    public void ConvertCNameToLabel_WithConsecutiveUppercase_HandlesCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("XMLParser", "Xml parser"), // Consecutive uppercase letters are split at word boundaries
            ("HTMLGenerator", "Html generator"),
            ("SQLDatabase", "Sql database"),
            ("HTTPClient", "Http client"),
            ("JSONObject", "Json object"),
            ("XMLHttpRequest", "Xml http request"), // Consecutive uppercase letters are split at word boundaries
            ("HTMLParser", "Html parser")
        };

        // Act & Assert
        foreach (var (input, expected) in testCases)
        {
            var result = Utils.ConvertCNameToLabel(input);
            Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
        }
    }

    #endregion
}