using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Services;
using PasswordManager.Core.Alphabets;
using PasswordManager.Web.Controllers.Api;
using PasswordManager.Web.Models.Requests;

namespace PasswordManager.Web.Tests;

/// <summary>
/// Tests for <see cref="PasswordsApiController"/>
/// </summary>
public class PasswordsApiControllerTests
{
    private readonly Mock<IPasswordService> _serviceMock = new();

    [SetUp]
    public void SetUp()
    {
        _serviceMock.Reset();
    }

    [Test]
    public async Task GeneratePassword_ZeroLength_ShouldReturnBadRequest()
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
    public async Task GeneratePassword_EmptyAlphabet_ShouldReturnBadRequest()
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
    public async Task GeneratePassword_CommonWay_ShouldGeneratePasswordAndCheckIt()
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

        _serviceMock
            .Setup(m => m.GeneratePassword(request.Length, It.Is<IAlphabet>(m => CheckAlphabet(m, alphabet))))
            .Returns(password);
        _serviceMock
            .Setup(m => m.CheckPasswordAsync(password, It.Is<IAlphabet>(m => CheckAlphabet(m, alphabet)), default))
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
        _serviceMock.Verify(
            m => m.GeneratePassword(request.Length, It.Is<IAlphabet>(m => CheckAlphabet(m, alphabet))),
            Times.Once);
        _serviceMock.Verify(
            m => m.CheckPasswordAsync(password, It.Is<IAlphabet>(m => CheckAlphabet(m, alphabet)), default),
            Times.Once);
    }

    [Test]
    public async Task CheckPassword_EmptyPassword_ShouldReturnBadRequest()
    {
        // arrange
        var request = new VerifyPasswordRequest()
        {
            Password = "",
        };
        var controller = CreateController();

        // act
        var res = await controller.CheckPasswordAsync(request, default);

        // assert
        Assert.That(res.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CheckPassword_CommonWay_ShouldCheckPassword()
    {
        // arrange
        var request = new VerifyPasswordRequest()
        {
            Password = "0123456789abcdef",
        };
        var controller = CreateController();

        _serviceMock
            .Setup(m => m.CheckPasswordAsync(request.Password, Alphabet.Empty, default))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryLow));

        // act
        var res = await controller.CheckPasswordAsync(request, default);

        // assert
        Assert.That(res.Value, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.Value.IsCompomised, Is.True);
            Assert.That(res.Value.Strength, Is.EqualTo("very low"));
        });
        _serviceMock.Verify(m => m.CheckPasswordAsync(request.Password, Alphabet.Empty, default), Times.Once);
    }

    private PasswordsApiController CreateController()
    {
        return new PasswordsApiController(_serviceMock.Object);
    }

    private static bool CheckAlphabet(IAlphabet actual, IAlphabet expected)
    {
        return !actual.GetCharacters().Except(expected.GetCharacters()).Any();
    }
}
