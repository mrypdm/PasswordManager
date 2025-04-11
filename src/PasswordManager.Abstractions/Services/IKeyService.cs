using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;

namespace PasswordManager.Abstractions.Services;

/// <summary>
/// Service for key
/// </summary>
public interface IKeyService
{
    /// <summary>
    /// Init key service with <paramref name="key"/>
    /// </summary>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    /// <exception cref="ArgumentException">If <paramref name="sessionTimeout"/> is invalid</exception>
    Task InitKeyAsync(byte[] key, TimeSpan sessionTimeout, CancellationToken token);

    /// <summary>
    /// Change key settings
    /// </summary>
    /// <exception cref="KeyValidationException">If <paramref name="oldKey"/> is invalid</exception>
    /// <exception cref="KeyValidationException">If <paramref name="newKey"/> is invalid</exception>
    Task ChangeKeySettingsAsync(byte[] oldKey, byte[] newKey, CancellationToken token);

    /// <summary>
    /// Check if key data exists
    /// </summary>
    Task<bool> IsKeyDataExistAsync(CancellationToken token);

    /// <summary>
    /// Change timeout of key
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="sessionTimeout"/> is invalid</exception>
    /// <exception cref="StorageNotInitializedException">If storage is not initialized</exception>
    Task ChangeKeyTimeoutAsync(TimeSpan timeout, CancellationToken token);

    /// <summary>
    /// Clear key
    /// </summary>
    Task ClearKeyAsync(CancellationToken token);

    /// <summary>
    /// Clear key data
    /// </summary>
    Task ClearKeyDataAsync(CancellationToken token);
}
