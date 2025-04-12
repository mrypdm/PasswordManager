using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using PasswordManager.Abstractions.Contexts;
using PasswordManager.Abstractions.Counters;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Repositories;
using PasswordManager.Abstractions.Storages;
using PasswordManager.Abstractions.Validators;
using PasswordManager.Core.Options;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Tests.Services;

/// <summary>
/// Tests for <see cref="KeyService"/>
/// </summary>
public class KeyServiceTests
{
    private readonly Mock<IKeyDataRepository> _keyRepositoryMock = new();
    private readonly Mock<ISecureItemsRepository> _itemsRepositoryMock = new();
    private readonly Mock<IDataContext> _dataContextMock = new();
    private readonly Mock<ICrypto> _cryptoMock = new();
    private readonly Mock<IKeyStorage> _storageMock = new();
    private readonly Mock<IKeyValidatorFactory> _validatorFactoryMock = new();
    private readonly Mock<IKeyValidator> _validatorMock = new();
    private readonly Mock<ICounter> _counterMock = new();
    private readonly Mock<IOptions<KeyServiceOptions>> _optionsMock = new();

    [SetUp]
    public void SetUp()
    {
        _keyRepositoryMock.Reset();
        _itemsRepositoryMock.Reset();
        _dataContextMock.Reset();
        _cryptoMock.Reset();
        _storageMock.Reset();
        _validatorFactoryMock.Reset();
        _validatorMock.Reset();
        _counterMock.Reset();

        _validatorFactoryMock
            .Setup(m => m.Create(It.IsAny<EncryptedData>()))
            .Returns(_validatorMock.Object);
    }

    [Test]
    public async Task InitKey_KeyDataNotExists_ShouldAddEncryptedKeyToRepoAndInitStorage()
    {
        // arrange
        var timeout = TimeSpan.FromSeconds(10);
        var key = Array.Empty<byte>();
        var keyData = new EncryptedData { Data = [], Salt = [] };

        _cryptoMock
            .Setup(m => m.Encrypt(key, key))
            .Returns(keyData);
        _keyRepositoryMock
            .Setup(m => m.GetKeyDataAsync(default))
            .ThrowsAsync(new KeyDataNotExistsException());

        var service = CreateService();

        // act
        await service.InitKeyAsync(key, timeout, default);

        // assert
        _keyRepositoryMock.Verify(m => m.GetKeyDataAsync(default), Times.Once);
        _cryptoMock.Verify(m => m.Encrypt(key, key), Times.Once);
        _keyRepositoryMock.Verify(m => m.SetKeyDataAsync(keyData, default), Times.Once);
        _storageMock.Verify(m => m.InitStorage(key, timeout), Times.Once);
        _dataContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Test]
    public async Task InitKey_KeyDataExists_ShouldValidateKeyAndInitStorage()
    {
        // arrange
        var timeout = TimeSpan.FromSeconds(10);
        var key = Array.Empty<byte>();
        var keyData = new EncryptedData();

        _keyRepositoryMock
            .Setup(m => m.GetKeyDataAsync(default))
            .ReturnsAsync(keyData);

        var service = CreateService();

        // act
        await service.InitKeyAsync(key, timeout, default);

        // assert
        _keyRepositoryMock.Verify(m => m.GetKeyDataAsync(default), Times.Once);
        _validatorFactoryMock.Verify(m => m.Create(keyData), Times.Once);
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _storageMock.Verify(m => m.InitStorage(key, timeout), Times.Once);
    }

    [Test]
    public void InitKey_InvalidKey_ShouldCountAndThrow()
    {
        // arrange
        var key = Array.Empty<byte>();
        var timeout = TimeSpan.FromSeconds(10);
        var options = new KeyServiceOptions
        {
            MaxAttemptCounts = 5,
            BlockTimeout = TimeSpan.FromSeconds(5)
        };

        _validatorMock
            .Setup(m => m.Validate(key))
            .Throws(new KeyValidationException());
        _optionsMock
            .Setup(m => m.Value)
            .Returns(options);

        var service = CreateService();

        // act
        Assert.ThrowsAsync<KeyValidationException>(() => service.InitKeyAsync(key, timeout, default));

        // assert
        _validatorFactoryMock.Verify(m => m.Create(It.IsAny<EncryptedData>()), Times.Once);
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _counterMock.Verify(m => m.Increment(), Times.Once);
    }

    [Test]
    public void InitKey_InvalidKeyAndMaxAttempts_ShouldBlockStorage()
    {
        // arrange
        var key = Array.Empty<byte>();
        var timeout = TimeSpan.FromSeconds(10);
        var options = new KeyServiceOptions
        {
            MaxAttemptCounts = 5,
            BlockTimeout = TimeSpan.FromSeconds(5)
        };

        _validatorMock
            .Setup(m => m.Validate(It.IsAny<byte[]>()))
            .Throws(new KeyValidationException());
        _counterMock
            .Setup(m => m.Count)
            .Returns(options.MaxAttemptCounts);
        _optionsMock
            .Setup(m => m.Value)
            .Returns(options);

        var service = CreateService();

        // act
        Assert.ThrowsAsync<KeyValidationException>(() => service.InitKeyAsync(key, timeout, default));

        // assert
        _counterMock.Verify(m => m.Increment(), Times.Once);
        _counterMock.Verify(m => m.Count, Times.Once);
        _storageMock.Verify(m => m.Block(options.BlockTimeout), Times.Once);
    }

    [Test]
    public async Task InitKey_ShouldCheckForBlockedStorage()
    {
        // arrange
        var key = Array.Empty<byte>();
        var timeout = TimeSpan.FromSeconds(10);
        var service = CreateService();

        // act
        await service.InitKeyAsync(key, timeout, default);

        // assert
        _storageMock.Verify(m => m.ThrowIfBlocked(), Times.Once);
    }

    [Test]
    public async Task ChangeKeySettings_CommonWay_ShouldValidateKeysAndUpdateKeyDataAndReEncryptItems()
    {
        // arrange
        var oldKey = new byte[] { 1 };
        var oldKeyData = new EncryptedData();
        var newKey = new byte[] { 2 };
        var newKeyData = new EncryptedData();
        var itemData = new EncryptedItem() { Id = 12, Name = "name" };
        var account = new AccountData();
        var encryptedData = new EncryptedData();

        _cryptoMock
            .Setup(m => m.Encrypt(newKey, newKey))
            .Returns(newKeyData);
        _keyRepositoryMock
            .Setup(m => m.GetKeyDataAsync(default))
            .ReturnsAsync(oldKeyData);
        _itemsRepositoryMock
            .Setup(m => m.GetDataAsync(default))
            .ReturnsAsync([itemData]);
        _cryptoMock
            .Setup(m => m.DecryptJson<AccountData>(itemData, oldKey))
            .Returns(account);
        _cryptoMock
            .Setup(m => m.EncryptJson(account, newKey))
            .Returns(encryptedData);

        var service = CreateService();

        // act
        await service.ChangeKeySettingsAsync(oldKey, newKey, default);

        // assert
        _storageMock.Verify(m => m.ThrowIfBlocked(), Times.Once);
        _keyRepositoryMock.Verify(m => m.GetKeyDataAsync(default), Times.Once);
        _validatorFactoryMock.Verify(m => m.Create(oldKeyData), Times.Once);
        _validatorFactoryMock.Verify(m => m.Create(null), Times.Once);
        _validatorMock.Verify(m => m.Validate(oldKey), Times.Once);
        _validatorMock.Verify(m => m.Validate(newKey), Times.Once);
        _cryptoMock.Verify(m => m.Encrypt(newKey, newKey), Times.Once);
        _keyRepositoryMock.Verify(m => m.UpdateKeyDataAsync(newKeyData, default), Times.Once);
        _itemsRepositoryMock.Verify(m => m.GetDataAsync(default), Times.Once);
        _cryptoMock.Verify(m => m.DecryptJson<AccountData>(itemData, oldKey), Times.Once);
        _cryptoMock.Verify(m => m.EncryptJson(account, newKey), Times.Once);
        _itemsRepositoryMock.Verify(
            m => m.UpdateDataAsync(itemData.Id, itemData.Name, encryptedData, default),
            Times.Once);
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
        _dataContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Test]
    public void ChangeKeySettings_InvalidOldKey_ShouldThrow()
    {
        // arrange
        var oldKey = new byte[] { 1 };
        var oldKeyData = new EncryptedData();

        _keyRepositoryMock
            .Setup(m => m.GetKeyDataAsync(default))
            .ReturnsAsync(oldKeyData);
        _validatorMock
            .Setup(m => m.Validate(oldKey))
            .Throws(new KeyValidationException());

        var service = CreateService();

        // act
        Assert.ThrowsAsync<KeyValidationException>(() => service.ChangeKeySettingsAsync(oldKey, [], default));

        // assert
        _keyRepositoryMock.Verify(m => m.GetKeyDataAsync(default), Times.Once);
        _validatorFactoryMock.Verify(m => m.Create(oldKeyData), Times.Once);
        _validatorMock.Verify(m => m.Validate(oldKey), Times.Once);
    }

    [Test]
    public void ChangeKeySettings_InvalidNewKey_ShouldThrow()
    {
        // arrange
        var newKey = new byte[] { 1 };
        _keyRepositoryMock
            .Setup(m => m.GetKeyDataAsync(default))
            .ReturnsAsync(new EncryptedData());
        _validatorMock
            .Setup(m => m.Validate(newKey))
            .Throws(new KeyValidationException());

        var service = CreateService();

        // act
        Assert.ThrowsAsync<KeyValidationException>(() => service.ChangeKeySettingsAsync([], newKey, default));

        // assert
        _keyRepositoryMock.Verify(m => m.GetKeyDataAsync(default), Times.Once);
        _validatorFactoryMock.Verify(m => m.Create(null), Times.Once);
        _validatorMock.Verify(m => m.Validate(newKey), Times.Once);
    }

    [Test]
    public async Task IsKeyDataExists_ShouldCheckInRepository()
    {
        // arrange
        var service = CreateService();

        // act
        await service.IsKeyDataExistAsync(default);

        // assert
        _keyRepositoryMock.Verify(m => m.IsKeyDataExistAsync(default), Times.Once);
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
    public async Task ClearKey_ShouldClearStorage()
    {
        // arrange
        var service = CreateService();

        // act
        await service.ClearKeyAsync(default);

        // assert
        _storageMock.Verify(m => m.ClearKey(), Times.Once);
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
        _keyRepositoryMock.Verify(m => m.DeleteKeyDataAsync(default), Times.Once);
    }

    private KeyService CreateService()
    {
        return new KeyService(
            _keyRepositoryMock.Object, _itemsRepositoryMock.Object, _dataContextMock.Object, _cryptoMock.Object,
            _storageMock.Object, _validatorFactoryMock.Object, _counterMock.Object, _optionsMock.Object);
    }
}
