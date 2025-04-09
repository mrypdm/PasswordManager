using System.Threading.Tasks;

using Flurl.Http.Testing;
using PasswordManager.Abstractions.Models;
using PasswordManager.External.Checkers;
using PasswordManager.External.Exceptions;

namespace PasswordManager.External.Tests;

/// <summary>
/// Tests for <see cref="PwnedPasswordChecker"/>
/// </summary>
public class PwnedPasswordCheckerTests
{
    [Test]
    public async Task CheckPwned_ShouldMakeHttpRequest()
    {
        // arrange
        var httpTest = new HttpTest();
        httpTest.RespondWith("empty body");

        var checker = new PwnedPasswordChecker();

        // act
        await checker.CheckAsync("qwerty123", default);

        // assert
        httpTest.ShouldHaveCalled("https://api.pwnedpasswords.com/range/5CEC1").Times(1);
    }

    [Test]
    public async Task CheckPwned_ShouldHandleServerError()
    {
        // arrange
        var httpTest = new HttpTest();
        httpTest.RespondWith("error", 500);

        var checker = new PwnedPasswordChecker();

        // act
        var result = await checker.CheckAsync("qwerty123", default);

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
    public void CheckPwned_ShouldThrowOnNotServerError()
    {
        // arrange
        var httpTest = new HttpTest();
        httpTest.RespondWith("error", 400);

        var checker = new PwnedPasswordChecker();

        // act
        // assert
        Assert.ThrowsAsync<PwnedRequestException>(() => checker.CheckAsync("qwerty123", default));
        httpTest.ShouldHaveCalled("https://api.pwnedpasswords.com/range/5CEC1").Times(1);
    }

    [Test]
    [Ignore("This test makes requests to the real service, which may have consequences, so the test is disabled by default.")]
    public async Task CheckPwned_CompromisedPassword_ShouldReturnCompomisedResult()
    {
        // arrange
        var checker = new PwnedPasswordChecker();

        // act
        var result = await checker.CheckAsync("qwerty123", default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsCompromised, Is.EqualTo(PasswordCompromisation.Compromised));
            Assert.That(result.Strength, Is.EqualTo(PasswordStrength.VeryLow));
            Assert.That(result.Score, Is.EqualTo(0));
        });
    }
}
