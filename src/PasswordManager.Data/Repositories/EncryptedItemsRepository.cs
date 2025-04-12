using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Repositories;
using PasswordManager.Data.Contexts;
using PasswordManager.Data.Models;

namespace PasswordManager.Data.Repositories;

/// <inheritdoc />
public sealed class EncryptedItemsRepository(SecureDbContext context) : IEncryptedItemsRepository
{
    /// <inheritdoc />
    public async Task<IItem> AddItemAsync(EncryptedItem item, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Name);
        ArgumentNullException.ThrowIfNull(item.Data);
        ArgumentNullException.ThrowIfNull(item.Data.Data);
        ArgumentNullException.ThrowIfNull(item.Data.Salt);

        var itemDbModel = new EncryptedItemDbModel
        {
            Name = item.Name,
            Salt = item.Data.Salt,
            Data = item.Data.Data,
        };

        await context.EncryptedItems.AddAsync(itemDbModel, token);
        return itemDbModel;
    }

    /// <inheritdoc />
    public async Task UpdateItemAsync(EncryptedItem item, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Name);
        ArgumentNullException.ThrowIfNull(item.Data);
        ArgumentNullException.ThrowIfNull(item.Data.Data);
        ArgumentNullException.ThrowIfNull(item.Data.Salt);

        var itemDbModel = await context.EncryptedItems.SingleOrDefaultAsync(m => m.Id == item.Id, token)
            ?? throw new ItemNotExistsException($"Item with id={item.Id} not exists");

        itemDbModel.Name = item.Name;
        itemDbModel.Salt = item.Data.Salt;
        itemDbModel.Data = item.Data.Data;

        context.Update(itemDbModel);
    }

    /// <inheritdoc />
    public async Task DeleteItemAsync(int id, CancellationToken token)
    {
        await context.EncryptedItems.Where(m => m.Id == id).ExecuteDeleteAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedItem> GetItemByIdAsync(int id, CancellationToken token)
    {
        var item = await context.EncryptedItems.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id, token)
            ?? throw new ItemNotExistsException($"Item with id={id} not exists");

        return new EncryptedItem
        {
            Id = item.Id,
            Name = item.Name,
            Data = new EncryptedData
            {
                Data = item.Data,
                Salt = item.Salt
            }
        };
    }

    /// <inheritdoc />
    public async Task<EncryptedItem[]> GetItemsAsync(CancellationToken token)
    {
        return await context.EncryptedItems.AsNoTracking()
            .Select(item => new EncryptedItem
            {
                Id = item.Id,
                Name = item.Name,
                Data = new EncryptedData
                {
                    Data = item.Data,
                    Salt = item.Salt
                }
            })
            .ToArrayAsync(token);
    }
}
