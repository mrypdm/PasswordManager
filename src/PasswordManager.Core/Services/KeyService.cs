using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PasswordManager.Abstractions.Counters;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
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
    ICrypto crypto,
    IKeyStorage keyStorage,
    IKeyGenerator keyGenerator,
    IKeyValidatorFactory keyValidatorFactory,
    ICounter counter,
    IOptions<KeyServiceOptions> options) : IKeyService
{
    /// <inheritdoc />
    public async Task InitKeyAsync(string password, TimeSpan sessionTimeout,
        CancellationToken token)
    {
        keyStorage.ThrowIfBlocked();

        var key = keyGenerator.Generate(password);

        try
        {
            await ValidateKeyAsync(key, token);
        }
        catch (KeyDataNotExistsException)
        {
            await InitKeyDataAsync(key, token);
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
    public async Task ChangeKeySettingsAsync(string oldPassword, string newPassword,
        IKeyGenerator newKeyGenerator, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(newKeyGenerator);
        var key = keyGenerator.Generate(oldPassword);
        await ValidateKeyAsync(key, token);

        // seting up new key data
        var newKey = newKeyGenerator.Generate(newPassword);
        var newKeyData = crypto.Encrypt(newKey, newKey);
        await keyDataRepository.UpdateKeyDataAsync(newKeyData, token);

        // re-encrypting accounts
        var encryptedItems = await secureItemsRepository.GetDataAsync(token);
        foreach (var item in encryptedItems)
        {
            var decryptedData = crypto.DecryptJson<AccountData>(item, key);
            var encryptedData = crypto.EncryptJson(decryptedData, newKey);
            await secureItemsRepository.UpdateDataAsync(item.Id, item.Name, encryptedData, token);
        }

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

    private async Task InitKeyDataAsync(byte[] key, CancellationToken token)
    {
        var encryptedData = crypto.Encrypt(key, key);
        await keyDataRepository.SetKeyDataAsync(encryptedData, token);
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
