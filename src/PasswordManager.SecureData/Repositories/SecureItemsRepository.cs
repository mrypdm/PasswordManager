using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Repositories;

/// <inheritdoc />
public sealed class SecureItemsRepository(SecureDbContext context, ICrypto crypto, IMasterKeyStorage masterKeyStorage)
    : ISecureItemsRepository
{
    /// <inheritdoc />
    public async Task AddAccountAsync(AccountData data, CancellationToken token)
    {
        var encryptedData = crypto.EncryptJson(data, masterKeyStorage.MasterKey);

        var secureItem = new EncryptedDataDbModel
        {
            Name = data.Name,
            Salt = encryptedData.Salt,
            Data = encryptedData.Data,
        };

        await context.SecureItems.AddAsync(secureItem, token);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task DeleteItemAsync(EncryptedDataDbModel item, CancellationToken token)
    {
        await context.SecureItems.Where(m => m.Id == item.Id).ExecuteDeleteAsync(token);
    }

    /// <inheritdoc />
    public async Task UpdateAccountAsync(AccountData oldValue, AccountData newValue, CancellationToken token)
    {
        var secureData = await GetItemInternalAsync(oldValue.Name, withTrack: true, token);

        var encryptedData = crypto.EncryptJson(newValue, masterKeyStorage.MasterKey);

        secureData.Name = newValue.Name;
        secureData.Salt = encryptedData.Salt;
        secureData.Data = encryptedData.Data;

        context.SecureItems.Update(secureData);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedDataDbModel> GetItemByNameAsync(string accountName, CancellationToken token)
    {
        return await GetItemInternalAsync(accountName, withTrack: false, token);
    }

    /// <inheritdoc />
    public async Task<AccountData> GetAccountByNameAsync(string accountName, CancellationToken token)
    {
        var secureData = await GetItemInternalAsync(accountName, withTrack: false, token);
        return crypto.DecryptJson<AccountData>(secureData, masterKeyStorage.MasterKey);
    }

    private async Task<EncryptedDataDbModel> GetItemInternalAsync(string accountName, bool withTrack, CancellationToken token)
    {
        var query = context.SecureItems as IQueryable<EncryptedDataDbModel>;

        if (!withTrack)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(m => m.Name == accountName, token)
            ?? throw new KeyNotFoundException($"Cannot find data with name '{accountName}'");
    }
}
