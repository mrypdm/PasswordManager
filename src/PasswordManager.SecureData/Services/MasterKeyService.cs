using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Repositories;

namespace PasswordManager.SecureData.Services;

/// <inheritdoc />
public sealed class MasterKeyService(
    IMasterKeyDataRepository masterKeyDataRepository,
    IMasterKeyStorage masterKeyStorage,
    IKeyGenerator masterKeyGenerator,
    IKeyValidator keyValidator,
    ICrypto crypto) : IMasterKeyService
{
    /// <inheritdoc />
    public async Task InitMasterKeyAsync(string masterPassword, TimeSpan sessionTimeout,
        CancellationToken token)
    {
        var masterKey = masterKeyGenerator.Generate(masterPassword);

        try
        {
            await ValidateKeyAsync(masterKey, token);
        }
        catch (MasterKeyDataNotExistsException)
        {
            await InitMasterKeyData(masterKey, token);
        }

        masterKeyStorage.InitStorage(masterKey, sessionTimeout);
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

    private async Task InitMasterKeyData(byte[] masterKey, CancellationToken token)
    {
        var encryptedData = crypto.Encrypt(masterKey, masterKey);
        var masterKeyData = new EncryptedDataDbModel
        {
            Name = nameof(MasterKeyService),
            Salt = encryptedData.Salt,
            Data = encryptedData.Data
        };
        await masterKeyDataRepository.SetMasterKeyDataAsync(masterKeyData, token);
    }

    private async Task ValidateKeyAsync(byte[] masterKey, CancellationToken token)
    {
        keyValidator.Validate(masterKey);
        var masterKeyData = await masterKeyDataRepository.GetMasterKeyDataAsync(token);
        var decryptedData = crypto.Decrypt(masterKeyData, masterKey);
        if (!masterKey.SequenceEqual(decryptedData))
        {
            throw new InvalidMasterKeyException();
        }
    }

}
