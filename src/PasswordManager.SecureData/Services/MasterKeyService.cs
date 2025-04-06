using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Repositories;

namespace PasswordManager.SecureData.Services;

/// <inheritdoc />
public sealed class MasterKeyService(
    IMasterKeyDataRepository masterKeyDataRepository,
    IMasterKeyStorage masterKeyStorage,
    IKeyGenerator keyGenerator,
    IKeyValidator keyValidator) : IMasterKeyService
{
    /// <inheritdoc />
    public async Task InitMasterKeyAsync(string masterPassword, TimeSpan sessionTimeout,
        CancellationToken token)
    {
        var masterKey = keyGenerator.Generate(masterPassword);

        try
        {
            await ValidateKeyAsync(masterKey, token);
        }
        catch (MasterKeyDataNotExistsException)
        {
            await masterKeyDataRepository.SetMasterKeyDataAsync(masterKey, token);
        }

        masterKeyStorage.InitStorage(masterKey, sessionTimeout);
    }

    /// <inheritdoc />
    public async Task ChangeMasterKeySettingsAsync(string oldMasterPassword, string newMasterPassword,
        IKeyGenerator newKeyGenerator, CancellationToken token)
    {
        var masterKey = keyGenerator.Generate(oldMasterPassword);
        await ValidateKeyAsync(masterKey, token);

        var newMasterKey = newKeyGenerator.Generate(newMasterPassword);
        await masterKeyDataRepository.ChangeMasterKeyDataAsync(newMasterKey, token);
        masterKeyStorage.ClearKey();
    }

    /// <inheritdoc />
    public Task ChangeLifetimeAsync(TimeSpan lifetime, CancellationToken token)
    {
        masterKeyStorage.ChangeLifetime(lifetime);
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
        await masterKeyDataRepository.DeleteMasterKeyData(token);
    }

    private async Task ValidateKeyAsync(byte[] masterKey, CancellationToken token)
    {
        keyValidator.Validate(masterKey);
        await masterKeyDataRepository.ValidateMasterKeyDataAsync(masterKey, token);
    }
}
