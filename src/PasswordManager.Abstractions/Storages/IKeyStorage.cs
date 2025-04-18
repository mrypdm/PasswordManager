using System;
using PasswordManager.Abstractions.Exceptions;

namespace PasswordManager.Abstractions.Storages;

/// <summary>
/// Storage for key
/// </summary>
public interface IKeyStorage : IReadOnlyKeyStorage
{
    /// <summary>
    /// Init storage with key and key timeout
    /// </summary>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    /// <exception cref="ArgumentException">If <paramref name="timeout"/> is invalid</exception>
    /// <exception cref="StorageBlockedException">If storage is blocked</exception>
    void InitStorage(byte[] key, TimeSpan timeout);

    /// <summary>
    /// Change current timeout of key
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="timeout"/> is invalid</exception>
    /// <exception cref="StorageNotInitializedException">If storage is not initialized</exception>
    /// <exception cref="StorageBlockedException">If storage is blocked</exception>
    void ChangeTimeout(TimeSpan timeout);

    /// <summary>
    /// Clear key from storage
    /// </summary>
    /// <exception cref="StorageBlockedException">If storage is blocked</exception>
    void ClearKey();

    /// <summary>
    /// Blocks storage for <paramref name="timeout"/>
    /// </summary>
    void Block(TimeSpan timeout);
}
