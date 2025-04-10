namespace PasswordManager.Data.Models;

/// <summary>
/// Model for secure item
/// </summary>
public sealed class SecureItemDbModel : EncryptedDataDbModel
{
    /// <summary>
    /// Name of item
    /// </summary>
    public string Name { get; set; }
}
