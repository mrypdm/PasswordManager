using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Services;

namespace PasswordManager.SecureData.Repositories;

/// <inheritdoc />
public sealed class SecureItemsRepository(SecureDbContext context, ICrypto crypto, IMasterKeyStorage masterKeyStorage,
    IKeyValidator keyValidator)
    : ISecureItemsRepository, IMasterKeyDataRepository
{
    /// <inheritdoc />
    public async Task<int> AddAccountAsync(AccountData data, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(data);

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
    public async Task UpdateAccountAsync(int id, AccountData data, CancellationToken token)
    {
        var secureItem = await context.SecureItems.SingleOrDefaultAsync(m => m.Id == id, token)
            ?? throw new ItemNotExistsException($"Item with id={id} not exists");

        var encryptedData = crypto.EncryptJson(data, masterKeyStorage.MasterKey);
        secureItem.Name = data.Name;
        secureItem.Salt = encryptedData.Salt;
        secureItem.Data = encryptedData.Data;

        context.Update(secureItem);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token)
    {
        var item = await context.SecureItems.AsNoTracking().SingleOrDefaultAsync(m => m.Id == id, token)
            ?? throw new ItemNotExistsException($"Item with id={id} not exists");
        return crypto.DecryptJson<AccountData>(item, masterKeyStorage.MasterKey);
    }

    /// <inheritdoc />
    public async Task<EncryptedDataDbModel[]> GetItemsAsync(CancellationToken token)
    {
        return await context.SecureItems.Skip(1).AsNoTracking().ToArrayAsync(token);
    }

    /// <inheritdoc />
    public async Task ReEncryptRepositoryAsync(byte[] newMasterKey, CancellationToken token)
    {
        keyValidator.Validate(newMasterKey);

        var items = await context.SecureItems.ToArrayAsync(token);
        if (items.Length == 0)
        {
            throw new MasterKeyDataNotExistsException();
        }

        var masterKeyDataItem = items[0];
        var newMasterKeyData = crypto.Encrypt(newMasterKey, newMasterKey);
        masterKeyDataItem.Salt = newMasterKeyData.Salt;
        masterKeyDataItem.Data = newMasterKeyData.Data;
        context.SecureItems.Update(masterKeyDataItem);

        foreach (var item in items[1..])
        {
            var decrypted = crypto.DecryptJson<AccountData>(item, masterKeyStorage.MasterKey);
            var encrypted = crypto.EncryptJson(decrypted, newMasterKey);
            item.Salt = encrypted.Salt;
            item.Data = encrypted.Data;
            context.SecureItems.Update(item);
        }

        await context.SaveChangesAsync(token);
        masterKeyStorage.ClearKey();
    }

    /// <inheritdoc />
    public async Task SetMasterKeyDataAsync(byte[] masterKey, CancellationToken token)
    {
        keyValidator.Validate(masterKey);

        if (await GetMasterKeyDataInternalAsync(token) is not null)
        {
            throw new MasterKeyDataExistsException();
        }

        var encryptedData = crypto.Encrypt(masterKey, masterKey);
        var masterKeyData = new EncryptedDataDbModel
        {
            Name = nameof(MasterKeyService),
            Salt = encryptedData.Salt,
            Data = encryptedData.Data
        };

        await context.SecureItems.AddAsync(masterKeyData, token);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task ValidateMasterKeyDataAsync(byte[] masterKey, CancellationToken token)
    {
        keyValidator.Validate(masterKey);
        var masterKeyData = await GetMasterKeyDataInternalAsync(token)
            ?? throw new MasterKeyDataNotExistsException();

        try
        {
            var decryptedData = crypto.Decrypt(masterKeyData, masterKey);
            if (masterKey.SequenceEqual(decryptedData))
            {
                return;
            }
        }
        catch (CryptographicException)
        {
            // NOP
        }

        throw new InvalidMasterKeyException();
    }

    /// <inheritdoc />
    public async Task<bool> IsMasterKeyDataExistsAsync(CancellationToken token)
    {
        return await GetMasterKeyDataInternalAsync(token) is not null;
    }

    /// <inheritdoc />
    public async Task DeleteMasterKeyData(CancellationToken token)
    {
        await context.Database.EnsureDeletedAsync(token);
        await context.Database.MigrateAsync(token);
    }

    /// <inheritdoc />
    public async Task ChangeMasterKeyDataAsync(byte[] newMasterPassword, CancellationToken token)
    {
        await ReEncryptRepositoryAsync(newMasterPassword, token);
    }

    private Task<EncryptedDataDbModel> GetMasterKeyDataInternalAsync(CancellationToken token)
    {
        return context.SecureItems.SingleOrDefaultAsync(m => m.Id == 1, token);
    }
}
