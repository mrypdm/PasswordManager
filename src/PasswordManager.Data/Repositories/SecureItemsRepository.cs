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
public sealed class SecureItemsRepository(SecureDbContext context) : ISecureItemsRepository
{
    /// <inheritdoc />
    public async Task<IItem> AddDataAsync(string name, EncryptedData data, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Data);
        ArgumentNullException.ThrowIfNull(data.Salt);

        var secureItem = new SecureItemDbModel
        {
            Name = name,
            Salt = data.Salt,
            Data = data.Data,
        };

        await context.SecureItems.AddAsync(secureItem, token);
        return secureItem;
    }

    /// <inheritdoc />
    public async Task UpdateDataAsync(int id, string name, EncryptedData data, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Data);
        ArgumentNullException.ThrowIfNull(data.Salt);

        var secureItem = await context.SecureItems.SingleOrDefaultAsync(m => m.Id == id, token)
            ?? throw new ItemNotExistsException($"Item with id={id} not exists");

        secureItem.Name = name;
        secureItem.Salt = data.Salt;
        secureItem.Data = data.Data;

        context.Update(secureItem);
    }

    /// <inheritdoc />
    public async Task DeleteDataAsync(int id, CancellationToken token)
    {
        await context.SecureItems.Where(m => m.Id == id).ExecuteDeleteAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedData> GetDataByIdAsync(int id, CancellationToken token)
    {
        var item = await context.SecureItems.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id, token)
            ?? throw new ItemNotExistsException($"Item with id={id} not exists");

        return new EncryptedData
        {
            Data = item.Data,
            Salt = item.Salt
        };
    }

    /// <inheritdoc />
    public async Task<IItem[]> GetItemsAsync(CancellationToken token)
    {
        return await context.SecureItems.AsNoTracking().ToArrayAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedItem[]> GetDataAsync(CancellationToken token)
    {
        return await context.SecureItems.AsNoTracking()
            .Select(m => new EncryptedItem { Id = m.Id, Name = m.Name, Data = m.Data, Salt = m.Salt })
            .ToArrayAsync(token);
    }
}
