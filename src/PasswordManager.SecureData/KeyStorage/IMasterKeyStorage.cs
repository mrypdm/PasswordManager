using System;

namespace PasswordManager.SecureData.KeyStorage;

/// <summary>
/// Initializer for Master Key storage
/// </summary>
public interface IMasterKeyStorage
{
    /// <summary>
    /// Current master key
    /// </summary>
    /// <remarks>Can be <see langword="null"/> if storage has not been initialized or key has been expired</remarks>
    byte[] MasterKey { get; }

    /// <summary>
    /// Init storage with master key and Key life timeout
    /// </summary>
    void InitStorage(byte[] masterKey, TimeSpan timeout);

    /// <summary>
    /// Change current timeout of master key
    /// </summary>
    void ChangeTimeout(TimeSpan timeout);

    /// <summary>
    /// Clear key from storage
    /// </summary>
    void ClearKey();

    /// <summary>
    /// Check if storage is initialized
    /// </summary>
    bool IsInitialized();
}
