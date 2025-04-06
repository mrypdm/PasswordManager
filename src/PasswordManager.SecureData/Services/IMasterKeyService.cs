using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Factories;
using PasswordManager.SecureData.Exceptions;

namespace PasswordManager.SecureData.Services;

/// <summary>
/// Service for master key
/// </summary>
public interface IMasterKeyService
{
    /// <summary>
    /// Init master key from <paramref name="masterPassword"/> and validates it with current master key data.
    /// If not master key data exists, then creates master key data with new master key
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="masterPassword"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="masterPassword"/> is whitespace</exception>
    /// <exception cref="KeyValidationException">If <paramref name="masterPassword"/> is invalid</exception>
    /// <exception cref="ArgumentException">If <paramref name="sessionTimeout"/> is invalid</exception>
    /// <exception cref="InvalidMasterKeyException">If master password is invalid</exception>
    Task InitMasterKeyAsync(string masterPassword, TimeSpan sessionTimeout, CancellationToken token);

    /// <summary>
    /// Changes master key settings
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="oldMasterPassword"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="oldMasterPassword"/> is whitespace</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="newMasterPassword"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="newMasterPassword"/> is whitespace</exception>
    /// <exception cref="KeyValidationException">If <paramref name="oldMasterPassword"/> is invalid</exception>
    /// <exception cref="KeyValidationException">If <paramref name="newMasterPassword"/> is invalid</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="newKeyGenerator"/> is null</exception>
    Task ChangeMasterKeySettingsAsync(string oldMasterPassword, string newMasterPassword,
        IKeyGenerator newKeyGenerator, CancellationToken token);

    /// <summary>
    /// Check if master key data exists
    /// </summary>
    Task<bool> IsMasterKeyDataExistsAsync(CancellationToken token);

    /// <summary>
    /// Change timeout of master key
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="sessionTimeout"/> is invalid</exception>
    /// <exception cref="StorageNotInitializedException">If storage is not initialized</exception>
    Task ChangeKeyTimeoutAsync(TimeSpan timeout, CancellationToken token);

    /// <summary>
    /// Clear master key
    /// </summary>
    Task ClearMasterKeyAsync(CancellationToken token);

    /// <summary>
    /// Clear master key data
    /// </summary>
    Task ClearMasterKeyDataAsync(CancellationToken token);
}
