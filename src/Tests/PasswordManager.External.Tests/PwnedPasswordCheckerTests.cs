using System.Threading.Tasks;

using Flurl.Http.Testing;

using PasswordManager.Abstractions;
using PasswordManager.External.Checkers;
using PasswordManager.External.Exceptions;

namespace PasswordManager.External.Tests;

/// <summary>
/// Tests for <see cref="PwnedPasswordChecker"/>
/// </summary>
public class PwnedPasswordCheckerTests
{
    [Test]
    public async Task CheckHttpRequestIsMade()
    {
        // arrange
        var httpTest = new HttpTest();
        httpTest.RespondWith("empty body");

        var checker = new PwnedPasswordChecker();

        // act
        var result = await checker.CheckPasswordAsync("qwerty123", default);

        // assert
        httpTest.ShouldHaveCalled("https://api.pwnedpasswords.com/range/5CEC1").Times(1);
    }

    [Test]
    public async Task CheckHandleServerErrors()
    {
        // arrange
        var httpTest = new HttpTest();
        httpTest.RespondWith("error", 500);

        var checker = new PwnedPasswordChecker();

        // act
        var result = await checker.CheckPasswordAsync("qwerty123", default);

        // assert
        httpTest.ShouldHaveCalled("https://api.pwnedpasswords.com/range/5CEC1").Times(1);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCompromised, Is.EqualTo(PasswordCompromisation.Unknown));
            Assert.That(result.Strength, Is.EqualTo(PasswordStrength.Unknown));
            Assert.That(result.Score, Is.EqualTo(-1));
        });
    }

    [Test]
    public void CheckNotServerErrorThrows()
    {
        // arrange
        var httpTest = new HttpTest();
        httpTest.RespondWith("error", 400);

        var checker = new PwnedPasswordChecker();

        // act
        // assert
        Assert.ThrowsAsync<PwnedRequestException>(() => checker.CheckPasswordAsync("qwerty123", default));
        httpTest.ShouldHaveCalled("https://api.pwnedpasswords.com/range/5CEC1").Times(1);
    }

    [Test]
    [Category("Integration")]
    public async Task CheckCompromisedPassword()
    {
        // arrange
        var checker = new PwnedPasswordChecker();

        // act
        var result = await checker.CheckPasswordAsync("qwerty123", default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCompromised, Is.EqualTo(PasswordCompromisation.Compromised));
            Assert.That(result.Strength, Is.EqualTo(PasswordStrength.VeryLow));
            Assert.That(result.Score, Is.EqualTo(0));
        });
    }
}
