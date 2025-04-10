using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Generators;

namespace PasswordManager.Abstractions.Services;

/// <summary>
/// Service for key
/// </summary>
public interface IKeyService
{
    /// <summary>
    /// Init key from <paramref name="password"/> and validates it with current key data.
    /// If not key data exists, then creates key data with new key
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="password"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="password"/> is whitespace</exception>
    /// <exception cref="KeyValidationException">If <paramref name="password"/> is invalid</exception>
    /// <exception cref="ArgumentException">If <paramref name="sessionTimeout"/> is invalid</exception>
    Task InitKeyAsync(string password, TimeSpan sessionTimeout, CancellationToken token);

    /// <summary>
    /// Change key settings
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="oldPassword"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="oldPassword"/> is whitespace</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="newPassword"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="newPassword"/> is whitespace</exception>
    /// <exception cref="KeyValidationException">If <paramref name="oldPassword"/> is invalid</exception>
    /// <exception cref="KeyValidationException">If <paramref name="newPassword"/> is invalid</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="newKeyGenerator"/> is null</exception>
    Task ChangeKeySettingsAsync(string oldPassword, string newPassword,
        IKeyGenerator newKeyGenerator, CancellationToken token);

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
