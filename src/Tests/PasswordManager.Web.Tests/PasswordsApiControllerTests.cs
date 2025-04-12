using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Alphabets;
using PasswordManager.Web.Controllers.Api;
using PasswordManager.Web.Models.Requests;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref="PasswordsApiController"/>
/// </summary>
public class PasswordsApiControllerTests
{
    private readonly Mock<IPasswordGeneratorFactory> _generatorFactoryMock = new();
    private readonly Mock<IPasswordGenerator> _generatorMock = new();
    private readonly Mock<IPasswordCheckerFactory> _checkerFactoryMock = new();
    private readonly Mock<IPasswordChecker> _checkerMock = new();

    [SetUp]
    public void SetUp()
    {
        _generatorFactoryMock.Reset();
        _generatorMock.Reset();
        _checkerFactoryMock.Reset();
        _checkerMock.Reset();

        _generatorFactoryMock
            .Setup(m => m.Create(It.IsAny<IAlphabet>()))
            .Returns(_generatorMock.Object);
        _checkerFactoryMock
            .Setup(m => m.Create(It.IsAny<IAlphabet>()))
            .Returns(_checkerMock.Object);
    }

    [Test]
    public async Task Generate_ZeroLength_ShouldReturnBadRequest()
    {
        // arrange
        var request = new GeneratePasswordRequest()
        {
            Length = 0,
        };
        var controller = CreateController();

        // act
        var res = await controller.GeneratePasswordAsync(request, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Generate_EmptyAlphabet_ShouldReturnBadRequest()
    {
        // arrange
        var request = new GeneratePasswordRequest()
        {
            Length = 16,
        };
        var controller = CreateController();

        // act
        var res = await controller.GeneratePasswordAsync(request, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Generate_CommonWay_ShouldGeneratePasswordAndCheckIt()
    {
        // arrange
        var request = new GeneratePasswordRequest()
        {
            Length = 16,
            UseLowerLetters = true,
            UseNumbers = true,
        };
        var password = "0123456789abcdef";
        var alphabet = new Alphabet().WithNumbers().WithLowerLetters();

        _generatorMock
            .Setup(m => m.Generate(request.Length))
            .Returns(password);
        _checkerMock
            .Setup(m => m.CheckAsync(password, default))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryLow));

        var controller = CreateController();

        // act
        var res = await controller.GeneratePasswordAsync(request, default);

        // assert
        Assert.That(res.Value, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.Value.Password, Is.EqualTo(password));
            Assert.That(res.Value.CheckStatus.IsCompomised, Is.True);
            Assert.That(res.Value.CheckStatus.Strength, Is.EqualTo("very low"));
        });
        _generatorFactoryMock.Verify(m => m.Create(It.Is<IAlphabet>(m => CheckAlphabet(m, alphabet))), Times.Once);
        _generatorMock.Verify(m => m.Generate(request.Length), Times.Once);
        _checkerFactoryMock.Verify(m => m.Create(It.Is<IAlphabet>(m => CheckAlphabet(m, alphabet))), Times.Once);
        _checkerMock.Verify(m => m.CheckAsync(password, default), Times.Once);
    }

    [Test]
    public async Task Varify_EmptyPassword_ShouldReturnBadRequest()
    {
        // arrange
        var request = new VerifyPasswordRequest()
        {
            Password = "",
        };
        var controller = CreateController();

        // act
        var res = await controller.VerifyPasswordAsync(request, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Varify_CommonWay_ShouldCheckPassword()
    {
        // arrange
        var request = new VerifyPasswordRequest()
        {
            Password = "0123456789abcdef",
        };
        var controller = CreateController();

        _checkerMock
            .Setup(m => m.CheckAsync(request.Password, default))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryLow));

        // act
        var res = await controller.VerifyPasswordAsync(request, default);

        // assert
        Assert.That(res.Value, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.Value.IsCompomised, Is.True);
            Assert.That(res.Value.Strength, Is.EqualTo("very low"));
        });
        _checkerFactoryMock.Verify(m => m.Create(Alphabet.Empty), Times.Once);
        _checkerMock.Verify(m => m.CheckAsync(request.Password, default), Times.Once);
    }

    [Test]
    public async Task Varify_ShouldCalculateMinOfCheckResult()
    {
        // arrange
        var request = new VerifyPasswordRequest()
        {
            Password = "0123456789abcdef",
        };
        var controller = new PasswordsApiController(
            _generatorFactoryMock.Object,
            [_checkerFactoryMock.Object, _checkerFactoryMock.Object]);

        _checkerMock
            .SetupSequence(m => m.CheckAsync(request.Password, default))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryHigh))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.NotCompromised, PasswordStrength.VeryLow));

        // act
        var res = await controller.VerifyPasswordAsync(request, default);

        // assert
        Assert.That(res.Value, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.Value.IsCompomised, Is.True);
            Assert.That(res.Value.Strength, Is.EqualTo("very low"));
        });
        _checkerFactoryMock.Verify(m => m.Create(Alphabet.Empty), Times.Exactly(2));
        _checkerMock.Verify(m => m.CheckAsync(request.Password, default), Times.Exactly(2));
    }

    private PasswordsApiController CreateController()
    {
        return new PasswordsApiController(_generatorFactoryMock.Object, [_checkerFactoryMock.Object]);
    }

    private static bool CheckAlphabet(IAlphabet actual, IAlphabet expected)
    {
        return !actual.GetCharacters().Except(expected.GetCharacters()).Any();
    }
}
