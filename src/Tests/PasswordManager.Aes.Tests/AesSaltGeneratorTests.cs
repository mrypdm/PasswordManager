using System.Linq;
using PasswordManager.Aes.Generators;

namespace PasswordManager.Aes.Tests;

/// <summary>
/// Tests for <see cref="AesSaltGenerator"/>
/// </summary>
public class AesSaltGeneratorTests
{
    [Test]
    public void Generate_ShouldGenerateWithCorrectSize()
    {
        // arrange
        var generator = new AesSaltGenerator();

        // act
        var salt = generator.Generate();

        // assert
        Assert.That(salt, Has.Length.EqualTo(AesConstants.BlockSize));
    }

    [Test]
    public void Generate_ShouldGenerateDifferentSalts()
    {
        // arrange
        var generator = new AesSaltGenerator();

        // act
        var salt1 = generator.Generate();
        var salt2 = generator.Generate();

        // assert
        Assert.That(salt1.SequenceEqual(salt2), Is.Not.True, "Salts should be different");
    }
}
