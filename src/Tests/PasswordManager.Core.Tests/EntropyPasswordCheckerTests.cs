using System;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Tests;

/// <summary>
/// Tests for <see cref="EntropyPasswordChecker"/>
/// </summary>
public class EntropyPasswordCheckerTests
{
    [Test]
    [TestCase("qwerty", PasswordStrength.VeryLow, 28.2)]
    [TestCase("qwertyyyy", PasswordStrength.VeryLow, 42.3)]
    [TestCase("verylargestriiiiiiiiiiiiiiiing", PasswordStrength.VeryHigh, 141.0)]
    public async Task CheckEntropy_OnlyLowerLetters(string password, PasswordStrength strength, double score)
    {
        // arrange
        var alphabet = new Alphabet().WithLowerLetters();
        var checker = new EntropyPasswordChecker(alphabet);

        // act
        var result = await checker.CheckAsync(password, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCompromised, Is.EqualTo(PasswordCompromisation.Unknown));
            Assert.That(result.Strength, Is.EqualTo(strength));
            Assert.That(result.Score, Is.EqualTo(score).Within(0.1));
        });
    }

    [Test]
    [TestCase("Rhfcjxyst:erbL;ekbb912", PasswordStrength.VeryHigh, 143.5)]
    [TestCase("Shrek", PasswordStrength.VeryLow, 32.6)]
    [TestCase("ShrekISlon768", PasswordStrength.Medium, 84.8)]
    public async Task CheckEntropy_FullAlphabet(string password, PasswordStrength strength, double score)
    {
        // arrange
        var alphabet = new Alphabet()
            .WithLowerLetters()
            .WithUpperLetters()
            .WithNumbers()
            .WithCharacters();
        var checker = new EntropyPasswordChecker(alphabet);

        // act
        var result = await checker.CheckAsync(password, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCompromised, Is.EqualTo(PasswordCompromisation.Unknown));
            Assert.That(result.Strength, Is.EqualTo(strength));
            Assert.That(score, Is.EqualTo(result.Score).Within(0.1));
        });
    }

    [Test]
    public void CheckEntropy_WrongAlphabet_ShouldThrow()
    {
        // arrange
        var alphabet = new Alphabet().WithNumbers();
        var checker = new EntropyPasswordChecker(alphabet);

        // act
        // assert
        Assert.ThrowsAsync<InvalidOperationException>(() => checker.CheckAsync("letters", default));
    }
}
