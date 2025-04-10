using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PasswordManager.Abstractions.Counters;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Options;
using PasswordManager.SecureData.Repositories;
using PasswordManager.SecureData.Storages;

namespace PasswordManager.SecureData.Services;

/// <inheritdoc />
public sealed class KeyService(
    IKeyDataRepository keyDataRepository,
    IKeyStorage keyStorage,
    IKeyGenerator keyGenerator,
    IKeyValidator keyValidator,
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
            await keyDataRepository.SetKeyDataAsync(key, token);
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

        var newKey = newKeyGenerator.Generate(newPassword);
        await keyDataRepository.ChangeKeyDataAsync(newKey, token);
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
        keyValidator.Validate(key);
        await keyDataRepository.ValidateKeyDataAsync(key, token);
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
