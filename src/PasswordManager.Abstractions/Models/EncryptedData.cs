namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Secured data
/// </summary>
public class EncryptedData
{
    /// <summary>
    /// Salt of encrypted data
    /// </summary>
    public byte[] Salt { get; set; }

    /// <summary>
    /// Encrypted data
    /// </summary>
    public byte[] Data { get; set; }
}
