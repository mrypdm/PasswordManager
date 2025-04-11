using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Data.Contexts;
using PasswordManager.Data.Models;
using PasswordManager.Data.Repositories;

namespace PasswordManager.Data.Tests;

/// <summary>
/// Tests for <see cref="SecureItemsRepository"/>
/// </summary>
public class SecureItemsRepositoryTests : RepositoryTestsBase
{
    [Test]
    public void AddData_NullData_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddDataAsync(null, null, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.AddDataAsync("", null, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.AddDataAsync(" ", null, default));
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddDataAsync("test", null, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.AddDataAsync("test", new EncryptedData { Data = [], Salt = null }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.AddDataAsync("test", new EncryptedData { Data = null, Salt = [] }, default));
    }

    [Test]
    public async Task AddData_CommonWay_ShouldAddToDb()
    {
        // arrange
        var expectedId = 1;
        var expectedVersion = 0;
        var expectedName = "name";
        var key = RandomNumberGenerator.GetBytes(32);

        var data = new EncryptedData { Data = [11], Salt = [12] };

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.AddDataAsync(expectedName, data, default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            var dbData = context.SecureItems.FirstOrDefault();
            Assert.That(dbData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbData.Id, Is.EqualTo(expectedId));
                Assert.That(dbData.Name, Is.EqualTo(expectedName));
                Assert.That(dbData.Version, Is.EqualTo(expectedVersion));
                Assert.That(dbData.Data.SequenceEqual(data.Data), Is.True, "Data in db and local must be same");
                Assert.That(dbData.Salt.SequenceEqual(data.Salt), Is.True, "Salt in db and local must be same");
            });
        }
    }

    [Test]
    public void UpdateData_NullData_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateDataAsync(0, null, null, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateDataAsync(0, "", null, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateDataAsync(0, " ", null, default));
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateDataAsync(0, "test", null, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.UpdateDataAsync(0, "test", new EncryptedData { Data = [], Salt = null }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.UpdateDataAsync(0, "test", new EncryptedData { Data = null, Salt = [] }, default));
    }

    [Test]
    public void UpdateData_InvalidId_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ItemNotExistsException>(
            () => repo.UpdateDataAsync(0, "test", new EncryptedData { Data = [], Salt = [] }, default));
    }

    [Test]
    public async Task UpdateData_CommonWay_ShouldUpdateData()
    {
        // arrange
        var expectedId = 1;
        var oldItem = new SecureItemDbModel { Name = "test", Data = [11], Salt = [12] };
        var expecetedName = "name";
        var expectedData = new EncryptedData { Data = [13], Salt = [14] };

        using (var context = CreateDbContext())
        {
            context.SecureItems.Add(oldItem);
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.UpdateDataAsync(expectedId, expecetedName, expectedData, default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            var dbItem = context.SecureItems.FirstOrDefault();
            Assert.That(dbItem, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbItem.Id, Is.EqualTo(expectedId));
                Assert.That(dbItem.Name, Is.EqualTo(expecetedName));
                Assert.That(dbItem.Data.SequenceEqual(expectedData.Data), Is.True, "Data in db and local must be same");
                Assert.That(dbItem.Salt.SequenceEqual(expectedData.Salt), Is.True, "Salt in db and local must be same");
            });
        }
    }

    [Test]
    public async Task DeleteAccount_AccountNotExists_ShouldNotThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        await repo.DeleteDataAsync(0, default);
    }

    [Test]
    public async Task DeleteAccount_CommonWay_ShouldDeleteData()
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
            await repo.DeleteDataAsync(idToDelete, default);
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

    [Test]
    public void GetDataById_InvaliId_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ItemNotExistsException>(() => repo.GetDataByIdAsync(0, default));
    }

    [Test]
    public async Task GetDataById_CommonWay_ShouldReturnData()
    {
        // arrange
        var expectedId = 3;
        var key = RandomNumberGenerator.GetBytes(32);
        var expectedAccount = new AccountData();
        var item = new SecureItemDbModel
        {
            Name = "needed",
            Data = [132],
            Salt = [123]
        };

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
            var data = await repo.GetDataByIdAsync(expectedId, default);
            Assert.Multiple(() =>
            {
                Assert.That(data.Data.SequenceEqual(item.Data), "Data in DB and in etalon must be same");
                Assert.That(data.Salt.SequenceEqual(item.Salt), "Salt in DB and in etalon must be same");
            });
        }
    }

    [Test]
    public async Task GetNames_ShouldReturnAll()
    {
        // arrange
        var expectedId0 = 1;
        var expectedName0 = "first";
        var expectedId1 = 2;
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
                Assert.That(items[0].Id, Is.EqualTo(expectedId0));
                Assert.That(items[0].Name, Is.EqualTo(expectedName0));
                Assert.That(items[1].Id, Is.EqualTo(expectedId1));
                Assert.That(items[1].Name, Is.EqualTo(expectedName1));
            });
        }
    }

    private static SecureItemsRepository CreateRepository(SecureDbContext context)
    {
        return new SecureItemsRepository(context);
    }
}
