using Blackwood;
using NUnit.Framework;

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
public class FNV1a_64Tests
{
    [Test]
    public void FNV1a_64_EmptyString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "";
        ulong expectedHash = 14695981039346656037;

        // Act
        ulong actualHash = Match.FNV1a_64(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }

    [Test]
    public void FNV1a_64_SingleCharacter_ReturnsCorrectHash()
    {
        // Arrange
        string input = "a";
        ulong expectedHash = 0xcbf29ce484222325 ^ 0x61; // 0x61 is the ASCII value for 'a'
        expectedHash *= 0x100000001b3;

        // Act
        ulong actualHash = Match.FNV1a_64(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }


    [Test]
    public void FNV1a_64_SimpleString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "hello";
        ulong expectedHash = 0xa430d84680aabd0b;

        // Act
        ulong actualHash = Match.FNV1a_64(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }

    [Test]
    public void FNV1a_64_UppercaseString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "HELLO";
        ulong expectedHash = 0xa0b400b98ea8182b;

        // Act
        ulong actualHash = Match.FNV1a_64(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }

    [Test]
    public void FNV1a_64_NumericString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "123456";
        ulong expectedHash = 0xf6e3ed7e0e67290a;

        // Act
        ulong actualHash = Match.FNV1a_64(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }

    [Test]
    public void FNV1a_64_SpecialCharactersString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "!@#$%^&*()";
        ulong expectedHash = 0x4a7f734b06da6fa7;

        // Act
        ulong actualHash = Match.FNV1a_64(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }

    [Test]
    public void FNV1a_64_DifferentStrings_ReturnDifferentHashes()
    {
        // Arrange
        string input1 = "hello";
        string input2 = "world";

        // Act
        ulong hash1 = Match.FNV1a_64(input1);
        ulong hash2 = Match.FNV1a_64(input2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void FNV1a_64_CaseInsensitiveComparison_ReturnsDifferentHashes()
    {
        // Arrange
        string input1 = "hello";
        string input2 = "HELLO";

        // Act
        ulong hash1 = Match.FNV1a_64(input1);
        ulong hash2 = Match.FNV1a_64(input2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void FNV1a_64_LongString_ReturnsCorrectHash()
    {
        // Arrange
        string input = "This is a very long string used for testing the FNV-1a 64-bit hash function.";
        // The expected hash value should be precomputed using a known correct implementation of FNV-1a 64-bit hash function.
        ulong expectedHash = 0x94f445d80b4a9c79;

        // Act
        ulong actualHash = Match.FNV1a_64(input);

        // Assert
        Assert.That(expectedHash, Is.EqualTo(actualHash));
    }
}
