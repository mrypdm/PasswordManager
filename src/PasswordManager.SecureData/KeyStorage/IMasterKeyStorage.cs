using System;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.SecureData.Exceptions;

namespace PasswordManager.SecureData.KeyStorage;

/// <summary>
/// Initializer for Master Key storage
/// </summary>
public interface IMasterKeyStorage
{
    /// <summary>
    /// Current master key
    /// </summary>
    /// <exception cref="StorageNotInitializedException">If storage not initialzied</exception>
    byte[] MasterKey { get; }

    /// <summary>
    /// Check if storage is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Init storage with master key and Key life timeout
    /// </summary>
    /// <exception cref="KeyValidationException">If <paramref name="masterKey"/> is invalid</exception>
    /// <exception cref="ArgumentException">If <paramref name="timeout"/> is invalid</exception>
    void InitStorage(byte[] masterKey, TimeSpan timeout);

    /// <summary>
    /// Change current timeout of master key
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
