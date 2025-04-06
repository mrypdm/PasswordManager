using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
/// Tests for <see cref="MasterKeyDataRepository"/>
/// </summary>
public class MasterKeyDataRepositoryTests : RepositoryTestsBase
{
    private readonly Mock<ICrypto> _cryptoMock = new();
    private readonly Mock<IMasterKeyStorage> _storageMock = new();
    private readonly Mock<IKeyValidator> _validatorMock = new();

    [SetUp]
    public void SetUp()
    {
        _cryptoMock.Reset();
        _storageMock.Reset();
        _validatorMock.Reset();
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

    private MasterKeyDataRepository CreateRepository(SecureDbContext context)
    {
        return new MasterKeyDataRepository(context, _cryptoMock.Object, _storageMock.Object, _validatorMock.Object);
    }
}
