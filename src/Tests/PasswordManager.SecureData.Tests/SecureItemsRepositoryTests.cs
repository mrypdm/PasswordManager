using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Repositories;

namespace PasswordManager.SecureData.Tests;

/// <summary>
/// Tests for <see cref="SecureItemsRepository"/>
/// </summary>
public class SecureItemsRepositoryTests
{
    private readonly Mock<ICrypto> _cryptoMock = new();
    private readonly Mock<IMasterKeyStorage> _storageMock = new();
    private readonly Mock<IKeyValidator> _validatorMock = new();

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
    }

    [SetUp]
    public void SetUp()
    {
        _cryptoMock.Reset();
        _storageMock.Reset();
        _validatorMock.Reset();

        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
    }

    [Test]
    public async Task SetMasterKeyData_MasterKeyDataNotExists_ShouldEncryptMasterKeyWithItself()
    {
        // arrange
        var expectedId = 1;
        var expectedVersion = 0;
        var key = RandomNumberGenerator.GetBytes(32);
        var keyData = new EncryptedData { Data = [11], Salt = [12] };

        _cryptoMock.Setup(m => m.Encrypt(key, key)).Returns(keyData);

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.SetMasterKeyDataAsync(key, default);
        }

        // assert
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _cryptoMock.Verify(m => m.Encrypt(key, key), Times.Once);

        using (var context = CreateDbContext())
        {
            var dbData = context.MasterKeyData.SingleOrDefault();
            Assert.That(dbData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbData.Data.SequenceEqual(keyData.Data), Is.True, "Data in db and local must be same");
                Assert.That(dbData.Salt.SequenceEqual(keyData.Salt), Is.True, "Salt in db and local must be same");
                Assert.That(dbData.Id, Is.EqualTo(expectedId));
                Assert.That(dbData.Version, Is.EqualTo(expectedVersion));
            });
        }
    }

    [Test]
    public void SetMasterKeyData_MasterKeyDataExists_ShouldThrow()
    {
        // arrange
        using (var context = CreateDbContext())
        {
            context.MasterKeyData.Add(new MasterKeyDataDbModel() { Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            Assert.ThrowsAsync<MasterKeyDataExistsException>(() => repo.SetMasterKeyDataAsync(null, default));
        }
    }

    [Test]
    public void ChangeMasterKeyData_MasterKeyDataNotExists_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<MasterKeyDataNotExistsException>(() => repo.ChangeMasterKeyDataAsync(null, default));
    }

    [Test]
    public async Task ChangeMasterKeyData_CommonWay_ShouldReEncryptItems()
    {
        // arrange
        var expectedId = 1;
        var expectedVersion = 1;
        var expectedName = "encryptedOldItem";

        var oldKey = RandomNumberGenerator.GetBytes(32);
        var newKey = RandomNumberGenerator.GetBytes(32);

        _storageMock.Setup(m => m.MasterKey).Returns(oldKey);

        var encryptedNewKey = new EncryptedData() { Data = [10], Salt = [11] };
        _cryptoMock
            .Setup(m => m.Encrypt(newKey, newKey))
            .Returns(encryptedNewKey);

        var encryptedOldItem = new SecureItemDbModel() { Name = expectedName, Data = [], Salt = [] };
        var decrypted = new AccountData();
        var encryptedNewItem = new EncryptedData() { Data = [14], Salt = [15] };
        _cryptoMock
            .Setup(m => m.DecryptJson<AccountData>(
                It.Is<SecureItemDbModel>(t => t.Name == expectedName),
                oldKey))
            .Returns(decrypted);
        _cryptoMock
            .Setup(m => m.EncryptJson(decrypted, newKey))
            .Returns(encryptedNewItem);

        using (var context = CreateDbContext())
        {
            context.MasterKeyData.Add(new MasterKeyDataDbModel() { Data = [], Salt = [] });
            context.SecureItems.Add(encryptedOldItem);
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.ChangeMasterKeyDataAsync(newKey, default);
        }

        // assert
        _validatorMock.Verify(m => m.Validate(newKey), Times.Once);
        _cryptoMock.Verify(m => m.Encrypt(newKey, newKey), Times.Once);
        _storageMock.Verify(m => m.MasterKey, Times.Once);
        _cryptoMock.Verify(m =>
            m.DecryptJson<AccountData>(
                It.Is<SecureItemDbModel>(t => t.Name == expectedName),
                oldKey),
            Times.Once);
        _cryptoMock.Verify(m => m.EncryptJson(decrypted, newKey), Times.Once);

        using (var context = CreateDbContext())
        {
            var dbKey = context.MasterKeyData.SingleOrDefault();
            Assert.That(dbKey, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbKey.Data.SequenceEqual(encryptedNewKey.Data), Is.True, "Data in db and local must be same");
                Assert.That(dbKey.Salt.SequenceEqual(encryptedNewKey.Salt), Is.True, "Salt in db and local must be same");
                Assert.That(dbKey.Id, Is.EqualTo(expectedId));
                Assert.That(dbKey.Version, Is.EqualTo(expectedVersion));
            });

            var dbItem = context.SecureItems.SingleOrDefault();
            Assert.That(dbItem, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbItem.Data.SequenceEqual(encryptedNewItem.Data), Is.True, "Data in db and local must be same");
                Assert.That(dbItem.Salt.SequenceEqual(encryptedNewItem.Salt), Is.True, "Salt in db and local must be same");
                Assert.That(dbItem.Id, Is.EqualTo(expectedId));
                Assert.That(dbItem.Version, Is.EqualTo(expectedVersion));
            });
        }
    }

    [Test]
    public void ValidateMasterKeyData_MasterKeyDataNotExists_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<MasterKeyDataNotExistsException>(() => repo.ValidateMasterKeyDataAsync(null, default));
    }

    [Test]
    public async Task ValidateMasterKeyData_MasterKeyDataExists_ShouldValidateKeyAndDecrypData()
    {
        // arrange
        var expectedId = 1;
        var key = RandomNumberGenerator.GetBytes(32);
        var keyData = new MasterKeyDataDbModel() { Data = [], Salt = [] };

        _cryptoMock
            .Setup(m => m.Decrypt(It.Is<MasterKeyDataDbModel>(m => m.Id == expectedId), key))
            .Returns(key);

        using (var context = CreateDbContext())
        {
            context.MasterKeyData.Add(keyData);
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.ValidateMasterKeyDataAsync(key, default);
        }

        // assert
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
        _cryptoMock.Verify(m => m.Decrypt(It.Is<MasterKeyDataDbModel>(m => m.Id == expectedId), key), Times.Once);
    }

    [Test]
    public void ValidateMasterKeyData_CryptoExceptionThrown_ShouldThrow()
    {
        // arrange
        var expectedId = 1;
        var key = RandomNumberGenerator.GetBytes(32);
        var keyData = new MasterKeyDataDbModel() { Data = [], Salt = [] };

        _cryptoMock
            .Setup(m => m.Decrypt(It.Is<MasterKeyDataDbModel>(m => m.Id == expectedId), key))
            .Throws(new CryptographicException());

        using (var context = CreateDbContext())
        {
            context.MasterKeyData.Add(keyData);
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            Assert.ThrowsAsync<InvalidMasterKeyException>(() => repo.ValidateMasterKeyDataAsync(key, default));
        }
    }

    [Test]
    public void ValidateMasterKeyData_DifferentKey_ShouldThrow()
    {
        // arrange
        var expectedId = 1;
        var key = RandomNumberGenerator.GetBytes(32);
        var keyData = new MasterKeyDataDbModel() { Data = [], Salt = [] };

        _cryptoMock
            .Setup(m => m.Decrypt(It.Is<MasterKeyDataDbModel>(m => m.Id == expectedId), key))
            .Returns([]);

        using (var context = CreateDbContext())
        {
            context.MasterKeyData.Add(keyData);
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            Assert.ThrowsAsync<InvalidMasterKeyException>(() => repo.ValidateMasterKeyDataAsync(key, default));
        }
    }

    [Test]
    public async Task IsMasterKeyDataExistsAsync_MasterKeyDataExists()
    {
        // arrange
        using (var context = CreateDbContext())
        {
            context.MasterKeyData.Add(new MasterKeyDataDbModel() { Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var res = await repo.IsMasterKeyDataExistsAsync(default);
            Assert.That(res, Is.True);
        }
    }

    [Test]
    public async Task IsMasterKeyDataExistsAsync_MasterKeyDataNotExists()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        var res = await repo.IsMasterKeyDataExistsAsync(default);

        // assert
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task DeleteMasterKeyData_ShouldReCreateDatabase()
    {
        // arrange
        using (var context = CreateDbContext())
        {
            context.MasterKeyData.Add(new MasterKeyDataDbModel() { Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.DeleteMasterKeyDataAsync(default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            Assert.That(context.SecureItems.Count(), Is.EqualTo(0));
        }
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

    private SecureItemsRepository CreateRepository(SecureDbContext context)
    {
        return new SecureItemsRepository(context, _cryptoMock.Object, _storageMock.Object, _validatorMock.Object);
    }

    private static SecureDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlite("Filename=fake-secure-db.db")
            .LogTo(TestContext.WriteLine)
            .EnableSensitiveDataLogging()
            .Options;
        return new SecureDbContext(options);
    }
}
