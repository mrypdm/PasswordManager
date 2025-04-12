namespace PasswordManager.Abstractions.Storages;

/// <summary>
/// Read only storage for key
/// </summary>
public interface IReadOnlyKeyStorage
{
    /// <summary>
    /// Current key
    /// </summary>
    /// <exception cref="StorageNotInitializedException">If storage not initialzied</exception>
    /// <exception cref="StorageBlockedException">If storage is blocked</exception>
    byte[] Key { get; }

    /// <summary>
    /// Check if storage is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Throws if blocked
    /// </summary>
    /// <exception cref="StorageBlockedException">If storage is blocked</exception>
    void ThrowIfBlocked();
}
