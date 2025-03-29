using PasswordManager.Abstractions.Models;

namespace PasswordManager.SecureData.Models;

/// <summary>
/// Encrypted item
/// </summary>
public class EncryptedDataDbModel : EncryptedData
{
    /// <summary>
    /// ID of item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of item
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Version of item
    /// </summary>
    public long Version { get; set; }
}
