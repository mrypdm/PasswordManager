using System.Threading.Tasks;
using Moq;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Alphabets;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Tests.Services;

/// <summary>
/// Tests for <see cref="PasswordService"/>
/// </summary>
public class PasswordServiceTests
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
    public void GeneratePassword_CommonWay_ShouldGeneratePassword()
    {
        // arrange
        var length = 16;
        var alphabet = new Alphabet().WithNumbers().WithLowerLetters();
        var password = "0123456789abcdef";

        _generatorMock
            .Setup(m => m.Generate(length))
            .Returns(password);

        var service = CreateService();

        // act
        var res = service.GeneratePassword(length, alphabet);

        // assert
        Assert.That(res, Is.EqualTo(password));
        _generatorFactoryMock.Verify(m => m.Create(alphabet), Times.Once);
        _generatorMock.Verify(m => m.Generate(length), Times.Once);
    }

    [Test]
    public async Task CheckPassword_CommonWay_ShouldCheckPassword()
    {
        // arrange
        var password = "0123";
        var service = CreateService();

        _checkerMock
            .Setup(m => m.CheckAsync(password, default))
            .ReturnsAsync(new PasswordCheckStatus(PasswordCompromisation.Compromised, PasswordStrength.VeryLow));

        // act
        var res = await service.CheckPasswordAsync(password, Alphabet.Empty, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(res.IsCompromised, Is.EqualTo(PasswordCompromisation.Compromised));
            Assert.That(res.Strength, Is.EqualTo(PasswordStrength.VeryLow));
        });
        _checkerFactoryMock.Verify(m => m.Create(Alphabet.Empty), Times.Once);
        _checkerMock.Verify(m => m.CheckAsync(password, default), Times.Once);
    }

    private PasswordService CreateService()
    {
        return new PasswordService(_generatorFactoryMock.Object, _checkerFactoryMock.Object);
    }
}
