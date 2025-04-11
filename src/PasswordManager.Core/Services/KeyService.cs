using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PasswordManager.Abstractions.Contexts;
using PasswordManager.Abstractions.Counters;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Repositories;
using PasswordManager.Abstractions.Services;
using PasswordManager.Abstractions.Storages;
using PasswordManager.Core.Options;

namespace PasswordManager.Core.Services;

/// <inheritdoc />
public sealed class KeyService(
    IKeyDataRepository keyDataRepository,
    ISecureItemsRepository secureItemsRepository,
    IDataContext dataContext,
    ICrypto crypto,
    IKeyStorage keyStorage,
    IKeyValidatorFactory keyValidatorFactory,
    ICounter counter,
    IOptions<KeyServiceOptions> options) : IKeyService
{
    /// <inheritdoc />
    public async Task InitKeyAsync(byte[] key, TimeSpan sessionTimeout,
        CancellationToken token)
    {
        keyStorage.ThrowIfBlocked();

        try
        {
            await ValidateKeyAsync(key, token);
        }
        catch (KeyDataNotExistsException)
        {
            await SetKeyDataAsync(key, update: false, token);
            await dataContext.SaveChangesAsync(token);
        }
        catch (KeyValidationException)
        {
            CountInvalidAttempt();
            throw;
        }

        counter.Clear();
        keyStorage.InitStorage(key, sessionTimeout);
    }

    /// <inheritdoc />
    public async Task ChangeKeySettingsAsync(byte[] oldKey, byte[] newKey, CancellationToken token)
    {
        keyStorage.ThrowIfBlocked();

        await ValidateKeyAsync(oldKey, token);
        await SetKeyDataAsync(newKey, update: true, token);

        var encryptedItems = await secureItemsRepository.GetDataAsync(token);
        foreach (var item in encryptedItems)
        {
            var decryptedData = crypto.DecryptJson<AccountData>(item, oldKey);
            var encryptedData = crypto.EncryptJson(decryptedData, newKey);
            await secureItemsRepository.UpdateDataAsync(item.Id, item.Name, encryptedData, token);
        }

        await dataContext.SaveChangesAsync(token);
        keyStorage.ClearKey();
    }

    /// <inheritdoc />
    public async Task<bool> IsKeyDataExistAsync(CancellationToken token)
    {
        return await keyDataRepository.IsKeyDataExistAsync(token);
    }

    /// <inheritdoc />
    public Task ChangeKeyTimeoutAsync(TimeSpan sessionTimeout, CancellationToken token)
    {
        keyStorage.ChangeTimeout(sessionTimeout);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearKeyAsync(CancellationToken token)
    {
        keyStorage.ClearKey();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ClearKeyDataAsync(CancellationToken token)
    {
        await ClearKeyAsync(token);
        await keyDataRepository.DeleteKeyDataAsync(token);
    }

    private async Task ValidateKeyAsync(byte[] key, CancellationToken token)
    {
        var keyData = await keyDataRepository.GetKeyDataAsync(token);
        var validator = keyValidatorFactory.Create(keyData);
        validator.Validate(key);
    }

    private async Task SetKeyDataAsync(byte[] key, bool update, CancellationToken token)
    {
        var validator = keyValidatorFactory.Create(null);
        validator.Validate(key);

        var encryptedData = crypto.Encrypt(key, key);

        if (update)
        {
            await keyDataRepository.UpdateKeyDataAsync(encryptedData, token);
        }
        else
        {
            await keyDataRepository.SetKeyDataAsync(encryptedData, token);
        }
    }

    private void CountInvalidAttempt()
    {
        counter.Increment();
        if (counter.Count == options.Value.MaxAttemptCounts)
        {
            counter.Clear();
            keyStorage.Block(options.Value.BlockTimeout);
        }
    }
}
