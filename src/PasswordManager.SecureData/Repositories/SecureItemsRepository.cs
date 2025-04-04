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
    public async Task<int> AddAccountAsync(AccountData data, CancellationToken token)
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

        return secureItem.Id;
    }

    /// <inheritdoc />
    public async Task DeleteItemAsync(EncryptedDataDbModel item, CancellationToken token)
    {
        await context.SecureItems.Where(m => m.Id == item.Id).ExecuteDeleteAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedDataDbModel> GetItemByIdAsync(int id, CancellationToken token)
    {
        return await context.SecureItems.SingleOrDefaultAsync(m => m.Id == id, token);
    }

    /// <inheritdoc />
    public async Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token)
    {
        var item = await GetItemByIdAsync(id, token);
        return crypto.DecryptJson<AccountData>(item, masterKeyStorage.MasterKey);
    }

    private async Task<EncryptedDataDbModel[]> GetItemsInternalAsync(string accountName, bool withTrack,
        CancellationToken token)
    {
        var query = context.SecureItems as IQueryable<EncryptedDataDbModel>;

        if (!withTrack)
        {
            query = query.AsNoTracking();
        }

        return await query.Where(m => m.Name == accountName).ToArrayAsync(token)
            ?? throw new KeyNotFoundException($"Cannot find data with name '{accountName}'");
    }
}
