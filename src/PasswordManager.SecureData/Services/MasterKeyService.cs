using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PasswordManager.Abstractions.Counters;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Options;
using PasswordManager.SecureData.Repositories;

namespace PasswordManager.SecureData.Services;

/// <inheritdoc />
public sealed class MasterKeyService(
    IMasterKeyDataRepository masterKeyDataRepository,
    IMasterKeyStorage masterKeyStorage,
    IKeyGenerator keyGenerator,
    IKeyValidator keyValidator,
    ICounter counter,
    IOptions<MasterKeyServiceOptions> options) : IMasterKeyService
{
    /// <inheritdoc />
    public async Task InitMasterKeyAsync(string masterPassword, TimeSpan sessionTimeout,
        CancellationToken token)
    {
        masterKeyStorage.ThrowIfBlocked();

        var masterKey = keyGenerator.Generate(masterPassword);

        try
        {
            await ValidateKeyAsync(masterKey, token);
        }
        catch (MasterKeyDataNotExistsException)
        {
            await masterKeyDataRepository.SetMasterKeyDataAsync(masterKey, token);
        }
        catch (InvalidMasterKeyException)
        {
            CountInvalidAttempt();
            throw;
        }

        counter.Clear();
        masterKeyStorage.InitStorage(masterKey, sessionTimeout);
    }

    /// <inheritdoc />
    public async Task ChangeMasterKeySettingsAsync(string oldMasterPassword, string newMasterPassword,
        IKeyGenerator newKeyGenerator, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(newKeyGenerator);

        var masterKey = keyGenerator.Generate(oldMasterPassword);
        await ValidateKeyAsync(masterKey, token);

        var newMasterKey = newKeyGenerator.Generate(newMasterPassword);
        await masterKeyDataRepository.ChangeMasterKeyDataAsync(newMasterKey, token);
        masterKeyStorage.ClearKey();
    }

    /// <inheritdoc />
    public async Task<bool> IsMasterKeyDataExistsAsync(CancellationToken token)
    {
        return await masterKeyDataRepository.IsMasterKeyDataExistsAsync(token);
    }

    /// <inheritdoc />
    public Task ChangeKeyTimeoutAsync(TimeSpan sessionTimeout, CancellationToken token)
    {
        masterKeyStorage.ChangeTimeout(sessionTimeout);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearMasterKeyAsync(CancellationToken token)
    {
        masterKeyStorage.ClearKey();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ClearMasterKeyDataAsync(CancellationToken token)
    {
        await ClearMasterKeyAsync(token);
        await masterKeyDataRepository.DeleteMasterKeyDataAsync(token);
    }

    private async Task ValidateKeyAsync(byte[] masterKey, CancellationToken token)
    {
        keyValidator.Validate(masterKey);
        await masterKeyDataRepository.ValidateMasterKeyDataAsync(masterKey, token);
    }

    private void CountInvalidAttempt()
    {
        counter.Increment();
        if (counter.Count == options.Value.MaxAttemptCounts)
        {
            counter.Clear();
            masterKeyStorage.Block(options.Value.BlockTimeout);
        }
    }
}
