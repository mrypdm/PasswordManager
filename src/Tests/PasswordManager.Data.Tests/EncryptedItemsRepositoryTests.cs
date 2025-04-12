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
/// Tests for <see cref="EncryptedItemsRepository"/>
/// </summary>
public class EncryptedItemsRepositoryTests : RepositoryTestsBase
{
    [Test]
    public void AddData_NullData_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddItemAsync(null, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.AddItemAsync(new() { Name = "" }, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.AddItemAsync(new() { Name = " " }, default));
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddItemAsync(new() { Name = "test" }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.AddItemAsync(new() { Name = "test", Data = new() { Data = [] } }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.AddItemAsync(new() { Name = "test", Data = new() { Salt = [] } }, default));
    }

    [Test]
    public async Task AddData_CommonWay_ShouldAddToDb()
    {
        // arrange
        var expectedId = 1;
        var expectedVersion = 0;
        var key = RandomNumberGenerator.GetBytes(32);

        var data = new EncryptedItem { Name = "name", Data = new() { Data = [11], Salt = [12] } };

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.AddItemAsync(data, default);
            context.SaveChanges();
        }

        // assert
        using (var context = CreateDbContext())
        {
            var dbData = context.EncryptedItems.FirstOrDefault();
            Assert.That(dbData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbData.Id, Is.EqualTo(expectedId));
                Assert.That(dbData.Name, Is.EqualTo(data.Name));
                Assert.That(dbData.Version, Is.EqualTo(expectedVersion));
                Assert.That(dbData.Data.SequenceEqual(data.Data.Data), Is.True,
                    "Data in db and local must be same");
                Assert.That(dbData.Salt.SequenceEqual(data.Data.Salt), Is.True,
                    "Salt in db and local must be same");
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
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateItemAsync(null, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateItemAsync(new() { Name = "" }, default));
        Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateItemAsync(new() { Name = " " }, default));
        Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateItemAsync(new() { Name = "test" }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.UpdateItemAsync(new() { Name = "test", Data = new() { Data = [] } }, default));
        Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.UpdateItemAsync(new() { Name = "test", Data = new() { Salt = [] } }, default));
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
            () => repo.UpdateItemAsync(
                new() { Id = 0, Name = "test", Data = new() { Data = [], Salt = [] } },
                default));
    }

    [Test]
    public async Task UpdateData_CommonWay_ShouldUpdateData()
    {
        // arrange
        var oldItem = new EncryptedItemDbModel { Name = "test", Data = [11], Salt = [12] };
        var expectedItem = new EncryptedItem
        {
            Id = 1,
            Name = "name",
            Data = new()
            {
                Data = [13],
                Salt = [14]
            }
        };

        using (var context = CreateDbContext())
        {
            context.EncryptedItems.Add(oldItem);
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.UpdateItemAsync(expectedItem, default);
            context.SaveChanges();
        }

        // assert
        using (var context = CreateDbContext())
        {
            var dbItem = context.EncryptedItems.FirstOrDefault();
            Assert.That(dbItem, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dbItem.Id, Is.EqualTo(expectedItem.Id));
                Assert.That(dbItem.Name, Is.EqualTo(expectedItem.Name));
                Assert.That(dbItem.Data.SequenceEqual(expectedItem.Data.Data), Is.True,
                    "Data in db and local must be same");
                Assert.That(dbItem.Salt.SequenceEqual(expectedItem.Data.Salt), Is.True,
                    "Salt in db and local must be same");
            });
        }
    }

    [Test]
    public async Task DeleteItem_AccountNotExists_ShouldNotThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        await repo.DeleteItemAsync(0, default);
    }

    [Test]
    public async Task DeleteItem_CommonWay_ShouldDeleteData()
    {
        // arrange
        var idToDelete = 1;
        var expectedId = 2;
        var expectedLength = 1;

        using (var context = CreateDbContext())
        {
            context.EncryptedItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.EncryptedItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.SaveChanges();
        }

        // act
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            await repo.DeleteItemAsync(idToDelete, default);
        }

        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var items = context.EncryptedItems.ToArray();
            Assert.That(items, Has.Length.EqualTo(expectedLength));
            Assert.That(items[0].Id, Is.EqualTo(expectedId));
        }
    }

    [Test]
    public void GetItemById_InvaliId_ShouldThrow()
    {
        // arrange
        using var context = CreateDbContext();
        var repo = CreateRepository(context);

        // act
        // assert
        Assert.ThrowsAsync<ItemNotExistsException>(() => repo.GetItemByIdAsync(0, default));
    }

    [Test]
    public async Task GetItemById_CommonWay_ShouldReturnData()
    {
        // arrange
        var expectedId = 3;
        var key = RandomNumberGenerator.GetBytes(32);
        var item = new EncryptedItemDbModel
        {
            Name = "needed",
            Data = [132],
            Salt = [123]
        };

        using (var context = CreateDbContext())
        {
            context.EncryptedItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.EncryptedItems.Add(new() { Name = "", Data = [], Salt = [] });
            context.EncryptedItems.Add(item);
            context.SaveChanges();
        }

        // act
        // assert
        using (var context = CreateDbContext())
        {
            var repo = CreateRepository(context);
            var data = await repo.GetItemByIdAsync(expectedId, default);
            Assert.Multiple(() =>
            {
                Assert.That(data.Data.Data.SequenceEqual(item.Data), "Data in DB and in etalon must be same");
                Assert.That(data.Data.Salt.SequenceEqual(item.Salt), "Salt in DB and in etalon must be same");
            });
        }
    }

    [Test]
    public async Task GetItems_ShouldReturnAll()
    {
        // arrange
        var expectedId0 = 1;
        var expectedName0 = "first";
        var expectedId1 = 2;
        var expectedName1 = "second";
        var expectedLength = 2;

        using (var context = CreateDbContext())
        {
            context.EncryptedItems.Add(new() { Name = expectedName0, Data = [], Salt = [] });
            context.EncryptedItems.Add(new() { Name = expectedName1, Data = [], Salt = [] });
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

    private static EncryptedItemsRepository CreateRepository(SecureDbContext context)
    {
        return new EncryptedItemsRepository(context);
    }
}
