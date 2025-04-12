using System;
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
    private readonly Mock<ISecureItemsRepository> _repositoryMock = new();
    private readonly Mock<IDataContext> _dataContextMock = new();
    private readonly Mock<ICrypto> _cryptoMock = new();
    private readonly Mock<IKeyStorage> _storageMock = new();

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
        var account = new AccountData() { Name = "name" };
        var data = new EncryptedData() { Data = [], Salt = [] };
        var expectedId = 123;
        var item = new Mock<IItem>();
        item
            .Setup(m => m.Id)
            .Returns(expectedId);

        _storageMock
            .Setup(m => m.Key)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.EncryptJson(account, key))
            .Returns(data);
        _repositoryMock
            .Setup(m => m.AddDataAsync(account.Name, data, default))
            .ReturnsAsync(item.Object);
        var service = CreateService();

        // act
        var id = await service.AddAccountAsync(account, default);

        // assert
        Assert.That(id, Is.EqualTo(expectedId));
        _storageMock.Verify(m => m.Key, Times.Once);
        _cryptoMock.Verify(m => m.EncryptJson(account, key), Times.Once);
        _repositoryMock.Verify(m => m.AddDataAsync(account.Name, data, default), Times.Once);
        _dataContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Test]
    public void UpdateAccount_NullData_ShouldThrow()
    {
        // arrange
        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateAccountAsync(0, null, default));
    }

    [Test]
    public void UpdateAccount_ItemNotExists_ShouldThrow()
    {
        // arrange
        _repositoryMock
            .Setup(m => m.UpdateDataAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<EncryptedData>(),
                default))
            .ThrowsAsync(new ItemNotExistsException(string.Empty));
        var service = CreateService();

        // act
        // assert
        Assert.ThrowsAsync<AccountNotExistsException>(() => service.UpdateAccountAsync(0, new(), default));
    }

    [Test]
    public async Task UpdateAccount_CommonWay_ShouldEncryptAccountAndUpdateInRepo()
    {
        // arrange
        var key = Array.Empty<byte>();
        var account = new AccountData() { Name = "name" };
        var data = new EncryptedData() { Data = [], Salt = [] };
        var expectedId = 123;

        _storageMock
            .Setup(m => m.Key)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.EncryptJson(account, key))
            .Returns(data);
        var service = CreateService();

        // act
        await service.UpdateAccountAsync(expectedId, account, default);

        // assert
        _storageMock.Verify(m => m.Key, Times.Once);
        _cryptoMock.Verify(m => m.EncryptJson(account, key), Times.Once);
        _repositoryMock.Verify(m => m.UpdateDataAsync(expectedId, account.Name, data, default), Times.Once);
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
        _repositoryMock.Verify(m => m.DeleteDataAsync(expectedId, default), Times.Once);
    }

    [Test]
    public void GetAccountById_ItemNotExists_ShouldThrow()
    {
        // arrange
        _repositoryMock
            .Setup(m => m.GetDataByIdAsync(It.IsAny<int>(), default))
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
        var expectedAccount = new AccountData() { Name = "name" };
        var data = new EncryptedData() { Data = [], Salt = [] };
        var expectedId = 123;

        _storageMock
            .Setup(m => m.Key)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.DecryptJson<AccountData>(data, key))
            .Returns(expectedAccount);
        _repositoryMock
            .Setup(m => m.GetDataByIdAsync(expectedId, default))
            .ReturnsAsync(data);
        var service = CreateService();

        // act
        var account = await service.GetAccountByIdAsync(expectedId, default);

        // assert
        Assert.That(account, Is.EqualTo(expectedAccount));
        _storageMock.Verify(m => m.Key, Times.Once);
        _cryptoMock.Verify(m => m.DecryptJson<AccountData>(data, key), Times.Once);
        _repositoryMock.Verify(m => m.GetDataByIdAsync(expectedId, default), Times.Once);
    }

    [Test]
    public async Task GetNames_CommonWay_ShouldGetNamesInRepo()
    {
        // arrange
        var expectedId0 = 0;
        var expectedId1 = 1;

        var itemMock = new Mock<IItem>();
        itemMock
            .SetupSequence(m => m.Id)
            .Returns(expectedId0)
            .Returns(expectedId1);
        itemMock
            .SetupSequence(m => m.Name)
            .Returns($"{expectedId0}")
            .Returns($"{expectedId1}");
        _repositoryMock
            .Setup(m => m.GetItemsAsync(default))
            .ReturnsAsync([itemMock.Object, itemMock.Object]);

        var service = CreateService();

        // act
        var items = await service.GetAccountHeadersAsync(default);

        // assert
        _repositoryMock.Verify(m => m.GetItemsAsync(default), Times.Once);
        Assert.That(items, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(expectedId0));
            Assert.That(items[0].Name, Is.EqualTo($"{expectedId0}"));
            Assert.That(items[1].Id, Is.EqualTo(expectedId1));
            Assert.That(items[1].Name, Is.EqualTo($"{expectedId1}"));
        });
    }

    private AccountService CreateService()
    {
        return new AccountService(_repositoryMock.Object, _dataContextMock.Object, _cryptoMock.Object,
            _storageMock.Object);
    }
}
