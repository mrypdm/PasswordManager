using System;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.SecureData.Exceptions;

namespace PasswordManager.SecureData.Storages;

/// <summary>
/// Storage for key
/// </summary>
public interface IKeyStorage
{
    /// <summary>
    /// Current key
    /// </summary>
    /// <exception cref="StorageNotInitializedException">If storage not initialzied</exception>
    byte[] Key { get; }

    /// <summary>
    /// Check if storage is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Init storage with key and key timeout
    /// </summary>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    /// <exception cref="ArgumentException">If <paramref name="timeout"/> is invalid</exception>
    void InitStorage(byte[] key, TimeSpan timeout);

    /// <summary>
    /// Change current timeout of key
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="timeout"/> is invalid</exception>
    /// <exception cref="StorageNotInitializedException">If storage is not initialized</exception>
    void ChangeTimeout(TimeSpan timeout);

    /// <summary>
    /// Clear key from storage
    /// </summary>
    void ClearKey();

    /// <summary>
    /// Blocks storage for <paramref name="timeout"/>
    /// </summary>
    void Block(TimeSpan timeout);

    /// <summary>
    /// Throws if blocked
    /// </summary>
    /// <exception cref="StorageBlockedException">If storage is blocked</exception>
    void ThrowIfBlocked();
}
