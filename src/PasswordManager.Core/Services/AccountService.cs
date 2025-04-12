using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Contexts;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Repositories;
using PasswordManager.Abstractions.Services;
using PasswordManager.Abstractions.Storages;

namespace PasswordManager.Core.Services;

/// <inheritdoc />
public class AccountService(
    IEncryptedItemsRepository repository,
    IDataContext dataContext,
    ICrypto crypto,
    IReadOnlyKeyStorage keyStorage)
    : IAccountService
{
    /// <inheritdoc />
    public async Task<int> AddAccountAsync(AccountData data, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(data);

        var encryptedData = crypto.EncryptJson(data, keyStorage.Key);
        var encryptedItem = new EncryptedItem
        {
            Name = data.Name,
            Data = encryptedData.Data,
            Salt = encryptedData.Salt
        };
        var item = await repository.AddItemAsync(encryptedItem, token);
        await dataContext.SaveChangesAsync(token);

        return item.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAccountAsync(int id, AccountData data, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(data);
        var encryptedData = crypto.EncryptJson(data, keyStorage.Key);
        try
        {
            var item = new EncryptedItem
            {
                Id = id,
                Name = data.Name,
                Data = encryptedData.Data,
                Salt = encryptedData.Salt
            };
            await repository.UpdateItemAsync(item, token);
            await dataContext.SaveChangesAsync(token);
        }
        catch (ItemNotExistsException)
        {
            throw new AccountNotExistsException($"Account with id={id} not exists");
        }
    }

    /// <inheritdoc />
    public async Task DeleteAccountAsync(int id, CancellationToken token)
    {
        await repository.DeleteItemAsync(id, token);
    }

    /// <inheritdoc />
    public async Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token)
    {
        try
        {
            var encryptedData = await repository.GetItemByIdAsync(id, token);
            return crypto.DecryptJson<AccountData>(encryptedData, keyStorage.Key);
        }
        catch (ItemNotExistsException)
        {
            throw new AccountNotExistsException($"Account with id={id} not exists");
        }
    }

    /// <inheritdoc />
    public async Task<AccountHeader[]> GetAccountHeadersAsync(CancellationToken token)
    {
        var items = await repository.GetItemsAsync(token);
        return [.. items.Select(m => new AccountHeader { Id = m.Id, Name = m.Name })];
    }
}
