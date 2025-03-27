using System.Threading.Tasks;

using PasswordManager.Abstractions;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Tests;

/// <summary>
/// Tests for <see cref="NistPasswordChecker"/>
/// </summary>
public class NistPasswordCheckerTests
{
    [Test]
    [TestCase("qwerty", PasswordStrength.VeryLow, 14)]
    [TestCase("qwertyyyy", PasswordStrength.Low, 19.5)]
    [TestCase("verylargestriiiiiiiiiiiiiiiing", PasswordStrength.Medium, 46)]
    [TestCase("Rhfcjxyst:erbL;ekbb912", PasswordStrength.Medium, 44)]
    [TestCase("Shrek", PasswordStrength.VeryLow, 12)]
    [TestCase("ShrekISlon768", PasswordStrength.Low, 31.5)]
    public async Task CommonTest(string password, PasswordStrength strength, double score)
    {
        // arrange
        var checker = new NistPasswordChecker();

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
