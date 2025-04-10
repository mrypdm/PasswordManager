using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using PasswordManager.Abstractions.Counters;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Options;
using PasswordManager.SecureData.Repositories;
using PasswordManager.SecureData.Services;
using PasswordManager.SecureData.Storages;

namespace PasswordManager.SecureData.Tests;

/// <summary>
/// Tests for <see cref="KeyService"/>
/// </summary>
public class KeyServiceTests
{
    private readonly Mock<IKeyDataRepository> _repositoryMock = new();
    private readonly Mock<IKeyStorage> _storageMock = new();
    private readonly Mock<IKeyGenerator> _generatorMock = new();
    private readonly Mock<IKeyValidator> _validatorMock = new();
    private readonly Mock<ICounter> _counterMock = new();
    private readonly Mock<IOptions<KeyServiceOptions>> _optionsMock = new();

    [SetUp]
    public void SetUp()
    {
        _repositoryMock.Reset();
        _storageMock.Reset();
        _generatorMock.Reset();
        _validatorMock.Reset();
        _counterMock.Reset();
    }

    [Test]
    public async Task InitKey_KeyDataNotExists_ShouldInitKeyData()
    {
        // arrange
        var password = "password";
        var timeout = TimeSpan.FromSeconds(10);
        var key = RandomNumberGenerator.GetBytes(32);

        _generatorMock.Setup(m => m.Generate(password)).Returns(key);
        _repositoryMock
            .Setup(m => m.ValidateKeyDataAsync(key, default))
            .ThrowsAsync(new KeyDataNotExistsException());

        var service = CreateService();

        // act
        await service.InitKeyAsync(password, timeout, default);

        // assert

        _generatorMock.Verify(m => m.Generate(password), Times.Once);
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _repositoryMock.Verify(m => m.ValidateKeyDataAsync(key, default), Times.Once);
        _repositoryMock.Verify(m => m.SetKeyDataAsync(key, default), Times.Once);
        _storageMock.Verify(m => m.InitStorage(key, timeout), Times.Once);
    }

    [Test]
    public async Task InitKey_KeyDataExists_ShouldValidateKeyData()
    {
        // arrange
        var password = "password";
        var timeout = TimeSpan.FromSeconds(10);
        var key = RandomNumberGenerator.GetBytes(32);

        _generatorMock.Setup(m => m.Generate(password)).Returns(key);

        var service = CreateService();

        // act
        await service.InitKeyAsync(password, timeout, default);

        // assert

        _generatorMock.Verify(m => m.Generate(password), Times.Once);
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _repositoryMock.Verify(m => m.ValidateKeyDataAsync(key, default), Times.Once);
        _repositoryMock.Verify(m => m.SetKeyDataAsync(key, default), Times.Never);
        _storageMock.Verify(m => m.InitStorage(key, timeout), Times.Once);
    }

    [Test]
    public void InitKey_InvalidKey_ShouldCount()
    {
        // arrange
        var password = "password";
        var timeout = TimeSpan.FromSeconds(10);
        var options = new KeyServiceOptions
        {
            MaxAttemptCounts = 5,
            BlockTimeout = TimeSpan.FromSeconds(5)
        };

        _repositoryMock
            .Setup(m => m.ValidateKeyDataAsync(It.IsAny<byte[]>(), default))
            .ThrowsAsync(new KeyValidationException());
        _optionsMock
            .Setup(m => m.Value)
            .Returns(options);

        var service = CreateService();

        // act
        Assert.ThrowsAsync<KeyValidationException>(() => service.InitKeyAsync(password, timeout, default));

        // assert
        _repositoryMock.Verify(
            m => m.ValidateKeyDataAsync(It.IsAny<byte[]>(), default),
            Times.Once);
        _counterMock.Verify(m => m.Increment(), Times.Once);
    }

    [Test]
    public void InitKey_InvalidKeyAndMaxAttempts_ShouldBlockStorage()
    {
        // arrange
        var password = "password";
        var timeout = TimeSpan.FromSeconds(10);
        var options = new KeyServiceOptions
        {
            MaxAttemptCounts = 5,
            BlockTimeout = TimeSpan.FromSeconds(5)
        };

        _repositoryMock
            .Setup(m => m.ValidateKeyDataAsync(It.IsAny<byte[]>(), default))
            .ThrowsAsync(new KeyValidationException());
        _counterMock
            .Setup(m => m.Count)
            .Returns(options.MaxAttemptCounts);
        _optionsMock
            .Setup(m => m.Value)
            .Returns(options);

        var service = CreateService();

        // act
        Assert.ThrowsAsync<KeyValidationException>(() => service.InitKeyAsync(password, timeout, default));

        // assert
        _repositoryMock.Verify(
            m => m.ValidateKeyDataAsync(It.IsAny<byte[]>(), default),
            Times.Once);
        _counterMock.Verify(m => m.Increment(), Times.Once);
        _counterMock.Verify(m => m.Count, Times.Once);
        _storageMock.Verify(m => m.Block(options.BlockTimeout), Times.Once);
    }

    [Test]
    public async Task InitKey_StorageBlocked_ShouldThrow()
    {
        // arrange
        var password = "password";
        var timeout = TimeSpan.FromSeconds(10);
        var service = CreateService();

        // act
        await service.InitKeyAsync(password, timeout, default);

        // assert
        _storageMock.Verify(m => m.ThrowIfBlocked(), Times.Once);
    }

    [Test]
    public async Task ChangeKeySettings_ShouldGenerateKeysAndReEncryptRepository()
    {
        // arrange
        var oldPassword = "oldPassword";
        var oldKey = RandomNumberGenerator.GetBytes(32);
        var newPassword = "newPassword";
        var newKey = RandomNumberGenerator.GetBytes(32);

        var newGeneratorMock = new Mock<IKeyGenerator>();
        newGeneratorMock.Setup(m => m.Generate(newPassword)).Returns(newKey);
        _generatorMock.Setup(m => m.Generate(oldPassword)).Returns(oldKey);

        var service = CreateService();

        // act
        await service.ChangeKeySettingsAsync(oldPassword, newPassword, newGeneratorMock.Object, default);

        // assert

        _generatorMock.Verify(m => m.Generate(oldPassword), Times.Once);
        newGeneratorMock.Verify(m => m.Generate(newPassword), Times.Once);
        _validatorMock.Verify(m => m.Validate(oldKey), Times.Once);
        _repositoryMock.Verify(m => m.ValidateKeyDataAsync(oldKey, default), Times.Once);
        _repositoryMock.Verify(m => m.ChangeKeyDataAsync(newKey, default), Times.Once);
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
    }

    [Test]
    public void ChangeKeySettings_NullGenerator_ShouldThrow()
    {
        // arrange
        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ChangeKeySettingsAsync(null, null, null, default));
    }

    [Test]
    public async Task IsKeyDataExists_ShouldCheckInRepository()
    {
        // arrange
        var service = CreateService();

        // act
        await service.IsKeyDataExistAsync(default);

        // assert
        _repositoryMock.Verify(m => m.IsKeyDataExistAsync(default), Times.Once);
    }

    [Test]
    public async Task ChangeKeyTimeout_ShouldChangeTimeoutInStorage()
    {
        // arrange
        var timeout = TimeSpan.FromSeconds(10);
        var service = CreateService();

        // act
        await service.ChangeKeyTimeoutAsync(timeout, default);

        // assert
        _storageMock.Verify(m => m.ChangeTimeout(timeout), Times.Once);
    }

    [Test]
    public async Task ClearKeyData_ShouldClearStorageAndClearRepository()
    {
        // arrange
        var service = CreateService();

        // act
        await service.ClearKeyDataAsync(default);

        // assert
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
        _repositoryMock.Verify(m => m.DeleteKeyDataAsync(default), Times.Once);
    }

    [Test]
    public async Task ClearKey_ShouldClearStorage()
    {
        // arrange
        var service = CreateService();

        // act
        await service.ClearKeyAsync(default);

        // assert
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
    }

    private KeyService CreateService()
    {
        return new KeyService(_repositoryMock.Object, _storageMock.Object, _generatorMock.Object,
            _validatorMock.Object, _counterMock.Object, _optionsMock.Object);
    }
}
