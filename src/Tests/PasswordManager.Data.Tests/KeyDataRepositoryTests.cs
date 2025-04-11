using System;
using System.Linq;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Data.Contexts;
using PasswordManager.Data.Models;
using PasswordManager.Data.Repositories;

namespace PasswordManager.Data.Tests;

/// <summary>
/// Tests for <see cref="KeyDataRepository"/>
/// </summary>
public class KeyDataRepositoryTests : RepositoryTestsBase
{
    [Test]
    public void SetKeyData_NullData_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.SetKeyDataAsync(null, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SetKeyDataAsync(new() { Data = null, Salt = [] }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SetKeyDataAsync(new() { Data = [], Salt = null }, default));
    }

    [Test]
    public async Task SetKeyData_KeyDataNotExists_ShouldAddDataToDb()
    {
        // arrange
        var expectedId = 1;
        var expectedVersion = 0;
        var keyData = new EncryptedData { Data = [11], Salt = [12] };

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.SetKeyDataAsync(keyData, default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            var dbData = context.KeyData.SingleOrDefault();
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
    public void SetKeyData_KeyDataExists_ShouldThrow()
    {
        // arrange
        using (var context = CreateDbContext())
        {
            context.KeyData.Add(new KeyDataDbModel() { Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            Assert.ThrowsAsync<KeyDataExistsException>(
                () => repo.SetKeyDataAsync(new() { Data = [], Salt = [] }, default));
        }
    }

    [Test]
    public void UpdateKeyData_NullData_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.SetKeyDataAsync(null, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SetKeyDataAsync(new() { Data = null, Salt = [] }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SetKeyDataAsync(new() { Data = [], Salt = null }, default));
    }

    [Test]
    public void UpdateKeyData_KeyDataNotExists_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<KeyDataNotExistsException>(
            () => repo.UpdateKeyDataAsync(new() { Data = [], Salt = [] }, default));
    }

    [Test]
    public async Task UpdateKeyData_CommonWay_ShouldUpdateData()
    {
        // arrange
        var expectedId = 1;
        var expectedVersion = 1;
        var keyData = new EncryptedData { Data = [11], Salt = [12] };
        var newKeyData = new EncryptedData { Data = [13], Salt = [14] };
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.SetKeyDataAsync(keyData, default);
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.UpdateKeyDataAsync(newKeyData, default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            var dbData = context.KeyData.SingleOrDefault();
            Assert.That(dbData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbData.Data.SequenceEqual(newKeyData.Data), Is.True, "Data in db and local must be same");
                Assert.That(dbData.Salt.SequenceEqual(newKeyData.Salt), Is.True, "Salt in db and local must be same");
                Assert.That(dbData.Id, Is.EqualTo(expectedId));
                Assert.That(dbData.Version, Is.EqualTo(expectedVersion));
            });
        }
    }

    [Test]
    public async Task IsKeyDataExistAsync_KeyDataExists_ShouldReturnTrue()
    {
        // arrange
        using (var context = CreateDbContext())
        {
            context.KeyData.Add(new KeyDataDbModel() { Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var res = await repo.IsKeyDataExistAsync(default);
            Assert.That(res, Is.True);
        }
    }

    [Test]
    public async Task IsKeyDataExistAsync_KeyDataNotExists_ShouldReturnFalse()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        var res = await repo.IsKeyDataExistAsync(default);

        // assert
        Assert.That(res, Is.False);
    }

    [Test]
    public async Task DeleteKeyData_ShouldReCreateDatabase()
    {
        // arrange
        using (var context = CreateDbContext())
        {
            context.KeyData.Add(new KeyDataDbModel() { Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.DeleteKeyDataAsync(default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            Assert.That(context.SecureItems.Count(), Is.EqualTo(0));
        }
    }

    [Test]
    public void GetKeyData_KeyDataNotExists_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<KeyDataNotExistsException>(() => repo.GetKeyDataAsync(default));
    }

    [Test]
    public async Task GetKeyData_CommonWay_ShouldReturnData()
    {
        // arrange
        var keyData = new EncryptedData { Data = [11], Salt = [12] };
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.SetKeyDataAsync(keyData, default);
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var data = await repo.GetKeyDataAsync(default);

            // assert
            Assert.That(data, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(data.Data.SequenceEqual(keyData.Data), Is.True, "Data in db and local must be same");
                Assert.That(data.Salt.SequenceEqual(keyData.Salt), Is.True, "Salt in db and local must be same");
            });
        }
    }

    private static KeyDataRepository CreateRepository(SecureDbContext context)
    {
        return new KeyDataRepository(context);
    }
}
