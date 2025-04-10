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
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Storages;

namespace PasswordManager.SecureData.Repositories;

/// <inheritdoc />
public class KeyDataRepository(
    SecureDbContext context,
    ICrypto crypto,
    IKeyStorage keyStorage,
    IKeyValidator keyValidator) : IKeyDataRepository
{
    /// <inheritdoc />
    public async Task SetKeyDataAsync(byte[] key, CancellationToken token)
    {
        keyValidator.Validate(key);

        if (await GetKeyDataInternalAsync(token) is not null)
        {
            throw new KeyDataExistsException();
        }

        var encryptedData = crypto.Encrypt(key, key);
        var keyData = new KeyDataDbModel
        {
            Salt = encryptedData.Salt,
            Data = encryptedData.Data
        };

        await context.KeyData.AddAsync(keyData, token);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task ChangeKeyDataAsync(byte[] newKey, CancellationToken token)
    {
        keyValidator.Validate(newKey);

        // seting up new key data
        var keyData = await GetKeyDataInternalAsync(token)
            ?? throw new KeyDataNotExistsException();
        var newKeyData = crypto.Encrypt(newKey, newKey);
        keyData.Salt = newKeyData.Salt;
        keyData.Data = newKeyData.Data;
        context.KeyData.Update(keyData);

        // re-encrypting accounts
        var items = await context.SecureItems.ToArrayAsync(token);
        foreach (var item in items)
        {
            var decrypted = crypto.DecryptJson<AccountData>(item, keyStorage.Key);
            var encrypted = crypto.EncryptJson(decrypted, newKey);
            item.Salt = encrypted.Salt;
            item.Data = encrypted.Data;
            context.SecureItems.Update(item);
        }

        await context.SaveChangesAsync(token);
        keyStorage.ClearKey();
    }

    /// <inheritdoc />
    public async Task ValidateKeyDataAsync(byte[] key, CancellationToken token)
    {
        keyValidator.Validate(key);
        var keyData = await GetKeyDataInternalAsync(token)
            ?? throw new KeyDataNotExistsException();

        try
        {
            var decryptedData = crypto.Decrypt(keyData, key);
            if (key.SequenceEqual(decryptedData))
            {
                return;
            }
        }
        catch (CryptographicException)
        {
            // NOP
        }

        throw new InvalidKeyException();
    }

    /// <inheritdoc />
    public async Task<bool> IsKeyDataExistAsync(CancellationToken token)
    {
        return await GetKeyDataInternalAsync(token) is not null;
    }

    /// <inheritdoc />
    public async Task DeleteKeyDataAsync(CancellationToken token)
    {
        await context.Database.EnsureDeletedAsync(token);
        await context.Database.MigrateAsync(token);
    }

    private Task<KeyDataDbModel> GetKeyDataInternalAsync(CancellationToken token)
    {
        return context.KeyData.SingleOrDefaultAsync(token);
    }
}
