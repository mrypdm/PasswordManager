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

namespace PasswordManager.SecureData.Repositories;

public class MasterKeyDataRepository(
    SecureDbContext context,
    ICrypto crypto,
    IMasterKeyStorage masterKeyStorage,
    IKeyValidator keyValidator) : IMasterKeyDataRepository
{
    /// <inheritdoc />
    public async Task SetMasterKeyDataAsync(byte[] masterKey, CancellationToken token)
    {
        keyValidator.Validate(masterKey);

        if (await GetMasterKeyDataInternalAsync(token) is not null)
        {
            throw new MasterKeyDataExistsException();
        }

        var encryptedData = crypto.Encrypt(masterKey, masterKey);
        var masterKeyData = new MasterKeyDataDbModel
        {
            Salt = encryptedData.Salt,
            Data = encryptedData.Data
        };

        await context.MasterKeyData.AddAsync(masterKeyData, token);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task ChangeMasterKeyDataAsync(byte[] newMasterKey, CancellationToken token)
    {
        keyValidator.Validate(newMasterKey);

        // seting up new master key data
        var masterKeyData = await GetMasterKeyDataInternalAsync(token)
            ?? throw new MasterKeyDataNotExistsException();
        var newMasterKeyData = crypto.Encrypt(newMasterKey, newMasterKey);
        masterKeyData.Salt = newMasterKeyData.Salt;
        masterKeyData.Data = newMasterKeyData.Data;
        context.MasterKeyData.Update(masterKeyData);

        // re-encrypting accounts
        var items = await context.SecureItems.ToArrayAsync(token);
        foreach (var item in items)
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
    public async Task DeleteMasterKeyDataAsync(CancellationToken token)
    {
        await context.Database.EnsureDeletedAsync(token);
        await context.Database.MigrateAsync(token);
    }

    private Task<MasterKeyDataDbModel> GetMasterKeyDataInternalAsync(CancellationToken token)
    {
        return context.MasterKeyData.SingleOrDefaultAsync(token);
    }
}
