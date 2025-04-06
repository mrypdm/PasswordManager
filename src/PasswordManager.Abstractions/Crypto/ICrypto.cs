using System;
using PasswordManager.Abstractions.Exceptions;

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
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    EncryptedData Encrypt(byte[] data, byte[] key);

    /// <summary>
    /// Decrypt <paramref name="data"/> with <paramref name="key"/>
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    /// <exception cref="SaltValidationException">If <see cref="EncryptedData.Salt"/> in <paramref name="data"/> is invalid</exception>
    byte[] Decrypt(EncryptedData data, byte[] key);

    /// <summary>
    /// Serialize <paramref name="item"/> to JSON and encrypt it with <paramref name="key"/>
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null</exception>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    EncryptedData EncryptJson<TItem>(TItem item, byte[] key);

    /// <summary>
    /// Decrypt <paramref name="data"/> and deserialize from JSON <paramref name="key"/>
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    /// <exception cref="SaltValidationException">If <see cref="EncryptedData.Salt"/> in <paramref name="data"/> is invalid</exception>
    TItem DecryptJson<TItem>(EncryptedData data, byte[] key);
}
