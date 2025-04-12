using System;
using System.Linq;
using PasswordManager.Core.Alphabets;
using PasswordManager.Core.Generators;

namespace PasswordManager.Core.Tests.Generators;

/// <summary>
/// Tests for <see cref="PasswordGenerator"/>
/// </summary>
public class PasswordGeneratorTests
{
    [Test]
    public void Ctor_EmptyAlphabet_ShouldThrow()
    {
        // arrange
        // act
        // assert
        Assert.Throws<ArgumentException>(() => new PasswordGenerator(Alphabet.Empty));
    }

    [Test]
    public void Generate_ShouldGenerateWithCorrectLength()
    {
        // arrange
        var length = 16;
        var generator = new PasswordGenerator(new Alphabet().WithLowerLetters());

        // act
        var password = generator.Generate(length);

        // assert
        Assert.That(password, Has.Length.EqualTo(length));
    }

    [Test]
    public void Generate_ShouldGenerateWithCorrectAlphabet()
    {
        // arrange
        var length = 16;
        var alphabet = new Alphabet().WithNumbers();
        var generator = new PasswordGenerator(alphabet);

        // act
        var password = generator.Generate(length);

        // assert
        Assert.That(password.ToHashSet().Except(alphabet.GetCharacters()).Any(), Is.False);
    }

    [Test]
    public void Generate_ShouldGenerateDifferentPasswords()
    {
        // arrange
        var length = 16;
        var alphabet = new Alphabet().WithNumbers();
        var generator = new PasswordGenerator(alphabet);

        // act
        var password0 = generator.Generate(length);
        var password1 = generator.Generate(length);

        // assert
        Assert.That(password0, Is.Not.EqualTo(password1));
    }
}
