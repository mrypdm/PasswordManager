using System.Threading.Tasks;
using Moq;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Tests.Checkers;

/// <summary>
/// Tests for <see cref="EntropyPasswordChecker"/>
/// </summary>
public class CombinedPasswordCheckerTests
{
    private readonly Mock<IPasswordChecker> _checkerMock = new();

    [SetUp]
    public void SetUp()
    {
        _checkerMock.Reset();
    }

    [Test]
    public async Task Check_ShouldCalculateMinOfCheckResults()
    {
        // arrange
        var password = "password";
        _checkerMock
            .SetupSequence(m => m.CheckAsync(password, default))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryHigh))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.NotCompromised, PasswordStrength.VeryLow));
        var checker = new CombinedPasswordChecker([_checkerMock.Object, _checkerMock.Object]);

        // act
        var res = await checker.CheckAsync(password, default);

        // assert
        _checkerMock.Verify(m => m.CheckAsync(password, default), Times.Exactly(2));
        Assert.Multiple(() =>
        {
            Assert.That(res.IsCompromised, Is.EqualTo(PasswordCompromisation.Compromised));
            Assert.That(res.Strength, Is.EqualTo(PasswordStrength.VeryLow));
        });
    }
}
