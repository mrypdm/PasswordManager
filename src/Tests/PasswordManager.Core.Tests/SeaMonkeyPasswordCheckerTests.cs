using System.Threading.Tasks;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Tests;

/// <summary>
/// Tests for <see cref="SeaMonkeyPasswordChecker"/>
/// </summary>
public class SeaMonkeyPasswordCheckerTests
{
    [Test]
    [TestCase("qwerty", PasswordStrength.VeryLow)]
    [TestCase("qwertyyyy", PasswordStrength.VeryLow)]
    [TestCase("verylargestriiiiiiiiiiiiiiiing", PasswordStrength.VeryLow)]
    [TestCase("Rhfcjxyst:erbL;ekbb912", PasswordStrength.High)]
    [TestCase("Shrek", PasswordStrength.VeryLow)]
    [TestCase("ShrekISlon768", PasswordStrength.Medium)]
    public async Task CheckSeaMonkey(string password, PasswordStrength strength)
    {
        // arrange
        var checker = new SeaMonkeyPasswordChecker();

        // act
        var result = await checker.CheckAsync(password, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCompromised, Is.EqualTo(PasswordCompromisation.Unknown));
            Assert.That(result.Strength, Is.EqualTo(strength));
        });
    }
}
