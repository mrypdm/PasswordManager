namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Encrypted item
/// </summary>
public sealed class EncryptedItem : IItem
{
    /// <summary>
    /// Id of encrypted item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of encrypted item
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Encrypted data
    /// </summary>
    public EncryptedData EncryptedData { get; set; }
}
