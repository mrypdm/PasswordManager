using PasswordManager.Abstractions.Models;

namespace PasswordManager.Data.Models;

/// <summary>
/// Model for encrypted item
/// </summary>
public sealed class EncryptedItemDbModel : EncryptedDataDbModel, IItem
{
    /// <summary>
    /// Name of item
    /// </summary>
    public string Name { get; set; }
}
