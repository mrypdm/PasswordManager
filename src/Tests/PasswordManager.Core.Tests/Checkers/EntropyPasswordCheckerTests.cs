using System.Threading.Tasks;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Alphabets;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Tests.Checkers;

/// <summary>
/// Tests for <see cref="EntropyPasswordChecker"/>
/// </summary>
public class EntropyPasswordCheckerTests
{
    [Test]
    [TestCase("qwerty", PasswordStrength.VeryLow)]
    [TestCase("qwertyyyy", PasswordStrength.VeryLow)]
    [TestCase("verylargestriiiiiiiiiiiiiiiing", PasswordStrength.VeryHigh)]
    public async Task Check_OnlyLowerLetters_ShouldCalculateByEntropy(string password, PasswordStrength strength)
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
        });
    }

    [Test]
    [TestCase("Rhfcjxyst:erbL;ekbb912", PasswordStrength.VeryHigh)]
    [TestCase("Shrek", PasswordStrength.VeryLow)]
    [TestCase("ShrekISlon768", PasswordStrength.Medium)]
    public async Task Check_FullAlphabet_ShouldCalculateByEntropy(string password, PasswordStrength strength)
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
        });
    }

    [Test]
    public async Task Check_WrongAlphabet_ShouldReturnUnknown()
    {
        // arrange
        var alphabet = new Alphabet().WithNumbers();
        var checker = new EntropyPasswordChecker(alphabet);

        // act
        var res = await checker.CheckAsync("letters", default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(res.IsCompromised, Is.EqualTo(PasswordCompromisation.Unknown));
            Assert.That(res.Strength, Is.EqualTo(PasswordStrength.Unknown));
        });
    }
}
