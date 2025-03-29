using PasswordManager.Abstractions.Models;

namespace PasswordManager.Abstractions.Crypto;

/// <summary>
/// Crypto interface
/// </summary>
public interface ICrypto
{
    /// <summary>
    /// Encrypt <paramref name="data"/> with <paramref name="key"/>
    /// </summary>
    EncryptedData Encrypt(byte[] data, byte[] key);

    /// <summary>
    /// Decrypt <paramref name="data"/> with <paramref name="key"/>
    /// </summary>
    byte[] Decrypt(EncryptedData data, byte[] key);

    /// <summary>
    /// Serialize <paramref name="item"/> to JSON and encrypt it with <paramref name="key"/>
    /// </summary>
    EncryptedData EncryptJson<TItem>(TItem item, byte[] key);

    /// <summary>
    /// Decrypt <paramref name="data"/> and deserialize from JSON <paramref name="key"/>
    /// </summary>
    TItem DecryptJson<TItem>(EncryptedData data, byte[] key);
}
