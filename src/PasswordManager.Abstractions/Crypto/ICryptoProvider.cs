using System.Security.Cryptography;

namespace PasswordManager.Abstractions.Crypto;

/// <summary>
/// Provider for crypto transforms
/// </summary>
public interface ICryptoProvider
{
    /// <summary>
    /// Creates encryptor with salt
    /// </summary>
    ICryptoTransform CreateEncryptor(byte[] salt);

    /// <summary>
    /// Creates decryptor with salt
    /// </summary>
    ICryptoTransform CreateDecryptor(byte[] salt);
}
