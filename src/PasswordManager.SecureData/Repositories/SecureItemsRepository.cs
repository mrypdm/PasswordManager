using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Storages;

namespace PasswordManager.SecureData.Repositories;

/// <inheritdoc />
public sealed class SecureItemsRepository(
    SecureDbContext context,
    ICrypto crypto,
    IKeyStorage keyStorage)
    : ISecureItemsRepository
{
    /// <inheritdoc />
    public async Task<int> AddAccountAsync(AccountData data, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(data);

        var encryptedData = crypto.EncryptJson(data, keyStorage.Key);

        var secureItem = new SecureItemDbModel
        {
            Name = data.Name,
            Salt = encryptedData.Salt,
            Data = encryptedData.Data,
        };

        await context.SecureItems.AddAsync(secureItem, token);
        await context.SaveChangesAsync(token);

        return secureItem.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAccountAsync(int id, AccountData data, CancellationToken token)
    {
        var secureItem = await context.SecureItems.SingleOrDefaultAsync(m => m.Id == id, token)
            ?? throw new ItemNotExistsException($"Item with id={id} not exists");

        var encryptedData = crypto.EncryptJson(data, keyStorage.Key);
        secureItem.Name = data.Name;
        secureItem.Salt = encryptedData.Salt;
        secureItem.Data = encryptedData.Data;

        context.Update(secureItem);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task DeleteAccountAsync(int id, CancellationToken token)
    {
        await context.SecureItems.Where(m => m.Id == id).ExecuteDeleteAsync(token);
    }

    /// <inheritdoc />
    public async Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token)
    {
        var item = await context.SecureItems.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id, token)
            ?? throw new ItemNotExistsException($"Item with id={id} not exists");
        return crypto.DecryptJson<AccountData>(item, keyStorage.Key);
    }

    /// <inheritdoc />
    public async Task<SecureItemDbModel[]> GetItemsAsync(CancellationToken token)
    {
        return await context.SecureItems.AsNoTracking().ToArrayAsync(token);
    }
}
