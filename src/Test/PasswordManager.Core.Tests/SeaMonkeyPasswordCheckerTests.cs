using System.Threading.Tasks;

using PasswordManager.Abstractions;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Tests;

/// <summary>
/// Tests for <see cref="SeaMonkeyPasswordChecker"/>
/// </summary>
public class SeaMonkeyPasswordCheckerTests
{
    [Test]
    [TestCase("qwerty", PasswordStrength.VeryLow, 30)]
    [TestCase("qwertyyyy", PasswordStrength.VeryLow, 30)]
    [TestCase("verylargestriiiiiiiiiiiiiiiing", PasswordStrength.VeryLow, 30)]
    [TestCase("Rhfcjxyst:erbL;ekbb912", PasswordStrength.High, 110)]
    [TestCase("Shrek", PasswordStrength.VeryLow, 40)]
    [TestCase("ShrekISlon768", PasswordStrength.Medium, 90)]
    public async Task CommonTest(string password, PasswordStrength strength, double score)
    {
        // arrange
        var checker = new SeaMonkeyPasswordChecker();

        // act
        var result = await checker.CheckPasswordAsync(password, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCompromised, Is.EqualTo(PasswordCompromisation.Unknown));
            Assert.That(result.Strength, Is.EqualTo(strength));
            Assert.That(result.Score, Is.EqualTo(score).Within(0.1));
        });
    }
}
