using System;
using System.Linq;

namespace PasswordManager.Aes.Tests;

/// <summary>
/// Tests for <see cref="AesKeyGenerator"/>
/// </summary>
public class AesKeyGeneratorTests
{
    [Test]
    public void Generate_OneValueForSameSettings()
    {
        // arrange
        var generator = new AesKeyGenerator([0, 1, 2, 3, 4, 5, 6], 100);

        // act
        var key1 = generator.Generate("password");
        var key2 = generator.Generate("password");

        // assert
        Assert.That(key1.SequenceEqual(key2), Is.True, "Keys for one password must be equal");
    }

    [Test]
    public void Generate_DifferentValuesForDifferentPasswords()
    {
        // arrange
        var generator = new AesKeyGenerator([0, 1, 2, 3, 4, 5, 6], 100);

        // act
        var key1 = generator.Generate("password1");
        var key2 = generator.Generate("password2");

        // assert
        Assert.That(key1.SequenceEqual(key2), Is.False, "Keys for different passwords must be different");
    }

    [Test]
    public void Generate_DifferentValuesForDifferentSettings()
    {
        // arrange
        var generator1 = new AesKeyGenerator([0, 1, 2, 3, 4, 5, 6], 100);
        var generator2 = new AesKeyGenerator([0, 1, 2, 3, 4, 5, 7], 100);

        // act
        var key1 = generator1.Generate("password");
        var key2 = generator2.Generate("password");

        // assert
        Assert.That(key1.SequenceEqual(key2), Is.False, "Keys for different settings must be different");
    }

    [Test]
    public void Generate_InvalidPassword_Throw()
    {
        // arrange
        var generator = new AesKeyGenerator([0, 1, 2, 3, 4, 5, 6], 100);

        // act
        // assert
        Assert.Throws<ArgumentNullException>(() => generator.Generate(null));
        Assert.Throws<ArgumentNullException>(() => generator.Generate(""));
        Assert.Throws<ArgumentNullException>(() => generator.Generate(" "));
    }

    [Test]
    public void Ctor_NullSalt_Throw()
    {
        // arrange
        // act
        // assert
        Assert.Throws<ArgumentNullException>(() => new AesKeyGenerator(null, 100));
    }
}
