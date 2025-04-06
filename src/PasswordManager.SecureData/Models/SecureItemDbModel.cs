namespace PasswordManager.SecureData.Models;

/// <summary>
/// Model for master key data
/// </summary>
public sealed class SecureItemDbModel : EncryptedDataDbModel
{
    /// <summary>
    /// Name of item
    /// </summary>
    public string Name { get; set; }
}
