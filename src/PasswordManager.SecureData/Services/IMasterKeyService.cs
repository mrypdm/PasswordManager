using System;
using System.Threading;
using System.Threading.Tasks;
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
    /// <exception cref="InvalidMasterKeyException">If master password is invalid</exception>
    Task InitMasterKeyAsync(string masterPassword, TimeSpan sessionTimeout, CancellationToken token);

    /// <summary>
    /// Changes master key settings
    /// </summary>
    Task ChangeMasterKeySettingsAsync(string oldMasterPassword, string newMasterPassword,
        IKeyGenerator newKeyGenerator, CancellationToken token);

    /// <summary>
    /// Check if master key data exists
    /// </summary>
    Task<bool> IsMasterKeyDataExists(CancellationToken token);

    /// <summary>
    /// Change lifetime of master key
    /// </summary>
    Task ChangeLifetimeAsync(TimeSpan lifetime, CancellationToken token);

    /// <summary>
    /// Clear master key
    /// </summary>
    Task ClearMasterKeyAsync(CancellationToken token);

    /// <summary>
    /// Clear master key data
    /// </summary>
    Task ClearMasterKeyDataAsync(CancellationToken token);
}
