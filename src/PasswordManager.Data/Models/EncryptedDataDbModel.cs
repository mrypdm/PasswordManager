namespace PasswordManager.Data.Models;

/// <summary>
/// Encrypted item
/// </summary>
public abstract class EncryptedDataDbModel
{
    /// <summary>
    /// ID of item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Encrypted data
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    /// Salt for encrypted data
    /// </summary>
    public byte[] Salt { get; set; }

    /// <summary>
    /// Version of item
    /// </summary>
    public long Version { get; set; }
}
