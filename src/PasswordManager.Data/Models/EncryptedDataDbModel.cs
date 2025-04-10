using PasswordManager.Abstractions.Models;

namespace PasswordManager.Data.Models;

/// <summary>
/// Encrypted item
/// </summary>
public abstract class EncryptedDataDbModel : EncryptedData
{
    /// <summary>
    /// ID of item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Version of item
    /// </summary>
    public long Version { get; set; }
}
