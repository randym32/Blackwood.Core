using Blackwood;

/// <summary>
/// Key Points in These Tests:
/// 1.Empty String: Ensures that an empty string returns the expected FNV-1a hash.
/// 2. Single Character: Verifies that a single character string returns the
/// expected hash.
/// 
/// 3. Simple String: Ensures that a simple string like "hello" returns the
/// expected hash.
/// 
/// 4. Different Strings: Verifies that different strings return different hashes.
/// 
/// 5. Case Insensitive Comparison: Ensures that strings with different cases
/// return different hashes.
/// 
/// 6. Long String: Verifies that a longer string returns the expected hash.
/// </summary>
[TestFixture]
public class FNV1aTests
{
    [Test]
    public void FNV1a_EmptyString_ReturnsExpectedHash()
    {
        // Arrange
        string input = "";
        uint expectedHash = 2166136261;

        // Act
        uint result = Match.FNV1a(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(result));
    }

    [Test]
    public void FNV1a_SingleCharacter_ReturnsExpectedHash()
    {
        // Arrange
        string input = "a";
        uint expectedHash = 3826002220;

        // Act
        uint result = Match.FNV1a(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(result));
    }

    [Test]
    public void FNV1a_SimpleString_ReturnsExpectedHash()
    {
        // Arrange
        string input = "hello";
        uint expectedHash = 1335831723;

        // Act
        uint result = Match.FNV1a(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(result));
    }

    [Test]
    public void FNV1a_UppercaseString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "HELLO";
        // The expected hash value should be precomputed using a known correct implementation of FNV-1a 32-bit hash function.
        uint expectedHash = 0x32543b0b;

        // Act
        uint actualHash = Match.FNV1a(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }


    [Test]
    public void FNV1a_NumericString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "123456";
        // The expected hash value should be precomputed using a known correct implementation of FNV-1a 32-bit hash function.
        uint expectedHash = 0x9995b6aa;

        // Act
        uint actualHash = Match.FNV1a(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }


    [Test]
    public void FNV1a_SpecialCharactersString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "!@#$%^&*()";
        // The expected hash value should be precomputed using a known correct implementation of FNV-1a 32-bit hash function.
        uint expectedHash = 0x408eb4a7;

        // Act
        uint actualHash = Match.FNV1a(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }

    [Test]
    public void FNV1a_DifferentStrings_ReturnDifferentHashes()
    {
        // Arrange
        string input1 = "hello";
        string input2 = "world";

        // Act
        uint hash1 = Match.FNV1a(input1);
        uint hash2 = Match.FNV1a(input2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void FNV1a_CaseInsensitiveComparison_ReturnsDifferentHashes()
    {
        // Arrange
        string input1 = "hello";
        string input2 = "HELLO";

        // Act
        uint hash1 = Match.FNV1a(input1);
        uint hash2 = Match.FNV1a(input2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void FNV1a_LongString_ReturnsExpectedHash()
    {
        // Arrange
        string input = "The quick brown fox jumps over the lazy dog";
        // Generated using https://md5calc.com/hash/fnv1a32/
        uint expectedHash = 0x048fff90;

        // Act
        uint result = Match.FNV1a(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(result));
    }

}
