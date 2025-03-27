using System;

namespace PasswordManager.SecureData;

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
    /// Init storage with master key and Key lifetime
    /// </summary>
    void InitStorage(byte[] masterKey, TimeSpan keyLifeTime);

    /// <summary>
    /// Change current lifetime of master key
    /// </summary>
    void ChangeLifetime(TimeSpan keyLifeTime);
}
