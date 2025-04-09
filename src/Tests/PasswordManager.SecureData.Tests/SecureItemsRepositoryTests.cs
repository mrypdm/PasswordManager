using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Moq;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Models;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Repositories;

namespace PasswordManager.SecureData.Tests;

/// <summary>
/// Tests for <see cref="SecureItemsRepository"/>
/// </summary>
public class SecureItemsRepositoryTests : RepositoryTestsBase
{
    private readonly Mock<ICrypto> _cryptoMock = new();
    private readonly Mock<IMasterKeyStorage> _storageMock = new();

    [SetUp]
    public void SetUp()
    {
        _cryptoMock.Reset();
        _storageMock.Reset();
    }

    [Test]
    public void AddAccount_NullData_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAccountAsync(null, default));
    }

    [Test]
    public async Task AddAccount_CommonWay_ShouldEncryptAndAddToDb()
    {
        // arrange
        var expectedId = 1;
        var expectedVersion = 0;
        var key = RandomNumberGenerator.GetBytes(32);

        var data = new AccountData { Name = "", Login = "", Password = "" };
        var encrypted = new EncryptedData { Data = [11], Salt = [12] };

        _storageMock
            .Setup(m => m.MasterKey)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.EncryptJson(data, key))
            .Returns(encrypted);

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.AddAccountAsync(data, default);
        }

        // assert
        _storageMock.Verify(m => m.MasterKey, Times.Once);
        _cryptoMock.Verify(m => m.EncryptJson(data, key), Times.Once);
        using (var context = CreateDbContext())
        {
            var dbData = context.SecureItems.FirstOrDefault();
            Assert.That(dbData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbData.Data.SequenceEqual(encrypted.Data), Is.True, "Data in db and local must be same");
                Assert.That(dbData.Salt.SequenceEqual(encrypted.Salt), Is.True, "Salt in db and local must be same");
                Assert.That(dbData.Id, Is.EqualTo(expectedId));
                Assert.That(dbData.Version, Is.EqualTo(expectedVersion));
            });
        }
    }

    [Test]
    public void GetAccountById_InvaliId_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ItemNotExistsException>(() => repo.GetAccountByIdAsync(0, default));
    }

    [Test]
    public async Task GetAccountById_CommonWay_ShouldReturnAccount()
    {
        // arrange
        var expectedId = 3;
        var key = RandomNumberGenerator.GetBytes(32);
        var expectedAccount = new AccountData();
        var item = new SecureItemDbModel
        {
            Name = "needed",
            Data = [],
            Salt = []
        };

        _storageMock
            .Setup(m => m.MasterKey)
            .Returns(key);
        _cryptoMock
            .Setup(m => m.DecryptJson<AccountData>(It.Is<SecureItemDbModel>(m => m.Name == item.Name), key))
            .Returns(expectedAccount);

        using (var context = CreateDbContext())
        {
            context.SecureItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.SecureItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.SecureItems.Add(item);
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var account = await repo.GetAccountByIdAsync(expectedId, default);
            Assert.That(account, Is.EqualTo(expectedAccount));
        }

        _storageMock.Verify(m => m.MasterKey, Times.Once);
        _cryptoMock.Verify(
            m => m.DecryptJson<AccountData>(It.Is<SecureItemDbModel>(m => m.Name == item.Name), key),
            Times.Once);
    }

    [Test]
    public async Task GetItems_ShouldReturnAll()
    {
        // arrange
        var expectedName0 = "first";
        var expectedName1 = "second";
        var expectedLength = 2;

        using (var context = CreateDbContext())
        {
            context.SecureItems.Add(new() { Name = expectedName0, Data = [], Salt = [] });
            context.SecureItems.Add(new() { Name = expectedName1, Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var items = await repo.GetItemsAsync(default);

            Assert.That(items, Has.Length.EqualTo(expectedLength));
            Assert.Multiple(() =>
            {
                Assert.That(items[0].Name, Is.EqualTo(expectedName0));
                Assert.That(items[1].Name, Is.EqualTo(expectedName1));
            });
        }
    }

    [Test]
    public async Task DeleteAccount_AccountNotExists_ShouldNotThrow()
    {
        // arrange
        var idToDelete = 1;
        var expectedId = 2;
        var expectedLength = 1;

        using (var context = CreateDbContext())
        {
            context.SecureItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.SecureItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.DeleteAccountAsync(idToDelete, default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var items = context.SecureItems.ToArray();
            Assert.That(items, Has.Length.EqualTo(expectedLength));
            Assert.That(items[0].Id, Is.EqualTo(expectedId));
        }
    }

    private SecureItemsRepository CreateRepository(SecureDbContext context)
    {
        return new SecureItemsRepository(context, _cryptoMock.Object, _storageMock.Object);
    }
}
