using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Repositories;
using PasswordManager.SecureData.Services;

namespace PasswordManager.SecureData.Tests;

/// <summary>
/// Tests for <see cref="MasterKeyServiceService"/>
/// </summary>
public class MasterKeyServiceServiceTests
{
    private readonly Mock<IMasterKeyDataRepository> _repositoryMock = new();
    private readonly Mock<IMasterKeyStorage> _storageMock = new();
    private readonly Mock<IKeyGenerator> _generatorMock = new();
    private readonly Mock<IKeyValidator> _validatorMock = new();

    [SetUp]
    public void SetUp()
    {
        _repositoryMock.Reset();
        _storageMock.Reset();
        _generatorMock.Reset();
        _validatorMock.Reset();
    }

    [Test]
    public async Task InitMasterKey_MasterKeyDataNotExists_ShouldInitMasterKeyData()
    {
        // arrange
        var password = "password";
        var timeout = TimeSpan.FromSeconds(10);
        var key = RandomNumberGenerator.GetBytes(32);

        _generatorMock.Setup(m => m.Generate(password)).Returns(key);
        _repositoryMock
            .Setup(m => m.ValidateMasterKeyDataAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MasterKeyDataNotExistsException());

        var service = CreateService();

        // act
        await service.InitMasterKeyAsync(password, timeout, default);

        // assert

        _generatorMock.Verify(m => m.Generate(password), Times.Once);
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _repositoryMock.Verify(m => m.ValidateMasterKeyDataAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(m => m.SetMasterKeyDataAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        _storageMock.Verify(m => m.InitStorage(key, timeout), Times.Once);
    }

    [Test]
    public async Task InitMasterKey_MasterKeyDataExists_ShouldValidateMasterKeyData()
    {
        // arrange
        var password = "password";
        var timeout = TimeSpan.FromSeconds(10);
        var key = RandomNumberGenerator.GetBytes(32);

        _generatorMock.Setup(m => m.Generate(password)).Returns(key);

        var service = CreateService();

        // act
        await service.InitMasterKeyAsync(password, timeout, default);

        // assert

        _generatorMock.Verify(m => m.Generate(password), Times.Once);
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _repositoryMock.Verify(m => m.ValidateMasterKeyDataAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(m => m.SetMasterKeyDataAsync(key, It.IsAny<CancellationToken>()), Times.Never);
        _storageMock.Verify(m => m.InitStorage(key, timeout), Times.Once);
    }

    [Test]
    public async Task ChangeMasterKeySettings_ShouldGenerateKeysAndReEncryptRepository()
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
        await service.ChangeMasterKeySettingsAsync(oldPassword, newPassword, newGeneratorMock.Object, default);

        // assert

        _generatorMock.Verify(m => m.Generate(oldPassword), Times.Once);
        newGeneratorMock.Verify(m => m.Generate(newPassword), Times.Once);
        _validatorMock.Verify(m => m.Validate(oldKey), Times.Once);
        _repositoryMock.Verify(m => m.ValidateMasterKeyDataAsync(oldKey, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(m => m.ChangeMasterKeyDataAsync(newKey, It.IsAny<CancellationToken>()), Times.Once);
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
    }

    [Test]
    public void ChangeMasterKeySettings_NullGenerator_Throw()
    {
        // arrange
        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ChangeMasterKeySettingsAsync(null, null, null, default));
    }

    [Test]
    public async Task IsMasterKeyDataExists_ShouldCheckInRepository()
    {
        // arrange
        var service = CreateService();

        // act
        await service.IsMasterKeyDataExistsAsync(default);

        // assert
        _repositoryMock.Verify(m => m.IsMasterKeyDataExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ChangeKeyTimeout_ShouldChangeLifeTimeInStorage()
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
    public async Task ClearMasterKeyData_ShouldClearStorageAndClearRepository()
    {
        // arrange
        var service = CreateService();

        // act
        await service.ClearMasterKeyDataAsync(default);

        // assert
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
        _repositoryMock.Verify(m => m.DeleteMasterKeyDataAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ClearMasterKey_ShouldClearStorage()
    {
        // arrange
        var service = CreateService();

        // act
        await service.ClearMasterKeyAsync(default);

        // assert
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
    }

    private MasterKeyService CreateService()
    {
        return new MasterKeyService(_repositoryMock.Object, _storageMock.Object,
            _generatorMock.Object, _validatorMock.Object);
    }
}
