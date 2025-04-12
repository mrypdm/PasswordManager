using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using PasswordManager.Abstractions.Contexts;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Repositories;
using PasswordManager.Abstractions.Storages;
using PasswordManager.Core.Services;

namespace PasswordManager.Core.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IEncryptedItemsRepository> _repositoryMock = new();
    private readonly Mock<IDataContext> _dataContextMock = new();
    private readonly Mock<ICrypto> _cryptoMock = new();
    private readonly Mock<IReadOnlyKeyStorage> _storageMock = new();

    [SetUp]
    public void SetUp()
    {
        _repositoryMock.Reset();
        _dataContextMock.Reset();
        _cryptoMock.Reset();
        _storageMock.Reset();
    }

    [Test]
    public void AddAccount_NullData_ShouldThrow()
    {
        // arrange
        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAccountAsync(null, default));
    }

    [Test]
    public async Task AddAccount_CommonWay_ShouldEncryptAccountAndAddToRepo()
    {
        // arrange
        var key = Array.Empty<byte>();
        var account = new Account() { Name = "name", Data = new AccountData() };
        var encryptedData = new EncryptedItem() { Name = account.Name, Data = [], Salt = [] };
        var expectedId = 123;
        var item = new Mock<IItem>();
        item
            .Setup(m => m.Id)
            .Returns(expectedId);
        _storageMock
            .Setup(m => m.Key)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.EncryptJson(account.Data, key))
            .Returns(encryptedData);
        _repositoryMock
            .Setup(m => m.AddItemAsync(It.Is<EncryptedItem>(m => CheckItem(m, encryptedData)), default))
            .ReturnsAsync(item.Object);
        var service = CreateService();

        // act
        var id = await service.AddAccountAsync(account, default);

        // assert
        Assert.That(id, Is.EqualTo(expectedId));
        _storageMock.Verify(m => m.Key, Times.Once);
        _cryptoMock.Verify(m => m.EncryptJson(account.Data, key), Times.Once);
        _repositoryMock.Verify(
            m => m.AddItemAsync(It.Is<EncryptedItem>(m => CheckItem(m, encryptedData)), default),
            Times.Once);
        _dataContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Test]
    public void UpdateAccount_NullData_ShouldThrow()
    {
        // arrange
        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAccountAsync(null, default));
    }

    [Test]
    public void UpdateAccount_ItemNotExists_ShouldThrow()
    {
        // arrange
        _repositoryMock
            .Setup(m => m.UpdateItemAsync(It.IsAny<EncryptedItem>(), default))
            .ThrowsAsync(new ItemNotExistsException(string.Empty));
        _cryptoMock
            .Setup(m => m.EncryptJson(It.IsAny<AccountData>(), It.IsAny<byte[]>()))
            .Returns(new EncryptedData());

        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<AccountNotExistsException>(() => service.UpdateAccountAsync(new(), default));
    }

    [Test]
    public async Task UpdateAccount_CommonWay_ShouldEncryptAccountAndUpdateInRepo()
    {
        // arrange
        var key = Array.Empty<byte>();
        var account = new Account() { Id = 123, Name = "name", Data = new AccountData() };
        var encryptedData = new EncryptedData() { Data = [], Salt = [] };
        var expectedItem = new EncryptedItem
        {
            Id = account.Id,
            Name = account.Name,
            Data = encryptedData.Data,
            Salt = encryptedData.Salt
        };

        _storageMock
            .Setup(m => m.Key)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.EncryptJson(account.Data, key))
            .Returns(encryptedData);
        var service = CreateService();

        // act
        await service.UpdateAccountAsync(account, default);

        // assert
        _storageMock.Verify(m => m.Key, Times.Once);
        _cryptoMock.Verify(m => m.EncryptJson(account.Data, key), Times.Once);
        _repositoryMock.Verify(
            m => m.UpdateItemAsync(It.Is<EncryptedItem>(m => CheckItem(m, expectedItem)), default),
            Times.Once);
        _dataContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Test]
    public async Task DeleteAccount_CommonWay_ShouldDeleteDataInRepo()
    {
        // arrange
        var expectedId = 123;
        var service = CreateService();

        // act
        await service.DeleteAccountAsync(expectedId, default);

        // assert
        _repositoryMock.Verify(m => m.DeleteItemAsync(expectedId, default), Times.Once);
    }

    [Test]
    public void GetAccountById_ItemNotExists_ShouldThrow()
    {
        // arrange
        _repositoryMock
            .Setup(m => m.GetItemByIdAsync(It.IsAny<int>(), default))
            .ThrowsAsync(new ItemNotExistsException(string.Empty));
        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<AccountNotExistsException>(() => service.GetAccountByIdAsync(0, default));
    }

    [Test]
    public async Task GetAccountById_CommonWay_ShouldGetDataFromRepoAndDecrypt()
    {
        // arrange
        var key = Array.Empty<byte>();
        var expectedData = new AccountData();
        var item = new EncryptedItem() { Id = 123, Name = "name", Data = [], Salt = [] };

        _repositoryMock
            .Setup(m => m.GetItemByIdAsync(item.Id, default))
            .ReturnsAsync(item);
        _storageMock
            .Setup(m => m.Key)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.DecryptJson<AccountData>(item, key))
            .Returns(expectedData);
        var service = CreateService();

        // act
        var account = await service.GetAccountByIdAsync(item.Id, default);

        // assert
        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.EqualTo(item.Id));
            Assert.That(account.Name, Is.EqualTo(item.Name));
            Assert.That(account.Data, Is.EqualTo(expectedData));
        });
        _storageMock.Verify(m => m.Key, Times.Once);
        _cryptoMock.Verify(m => m.DecryptJson<AccountData>(item, key), Times.Once);
        _repositoryMock.Verify(m => m.GetItemByIdAsync(item.Id, default), Times.Once);
    }

    [Test]
    public async Task GetNames_CommonWay_ShouldGetNamesInRepo()
    {
        // arrange
        var item0 = new EncryptedItem { Id = 0, Name = "0" };
        var item1 = new EncryptedItem { Id = 1, Name = "1" };
        _repositoryMock
            .Setup(m => m.GetItemsAsync(default))
            .ReturnsAsync([item0, item1]);

        var service = CreateService();

        // act
        var items = await service.GetAccountHeadersAsync(default);

        // assert
        _repositoryMock.Verify(m => m.GetItemsAsync(default), Times.Once);
        Assert.That(items, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(item0.Id));
            Assert.That(items[0].Name, Is.EqualTo($"{item0.Name}"));
            Assert.That(items[1].Id, Is.EqualTo(item1.Id));
            Assert.That(items[1].Name, Is.EqualTo($"{item1.Name}"));
        });
    }

    private AccountService CreateService()
    {
        return new AccountService(_repositoryMock.Object, _dataContextMock.Object, _cryptoMock.Object,
            _storageMock.Object);
    }

    private static bool CheckItem(EncryptedItem actual, EncryptedItem expected)
    {
        return actual.Id == expected.Id
            && actual.Name == expected.Name
            && actual.Data.SequenceEqual(expected.Data)
            && actual.Salt.SequenceEqual(expected.Salt);
    }
}
