namespace PasswordManager.Abstractions;

/// <summary>
/// Factory for master key
/// </summary>
public interface IMasterKeyFactory
{
    /// <summary>
    /// Create master key by master password
    /// </summary>
    /// <remarks>
    /// The output key is 256 bits in size.
    /// </remarks>
    byte[] CreateMasterKey(string masterPassword);
}
