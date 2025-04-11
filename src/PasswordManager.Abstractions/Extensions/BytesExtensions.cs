using System.Security.Cryptography;

namespace PasswordManager.Abstractions.Extensions;

/// <summary>
/// Extensions for <see cref="byte[]"/>
/// </summary>
public static class BytesExtensions
{
    /// <summary>
    /// Encrypt <paramref name="data"/> with AES
    /// </summary>
    public static byte[] EncryptAes(this byte[] data, byte[] key, byte[] salt)
    {
        using var aes = Aes.Create();
        using var encryptor = aes.CreateEncryptor(key, salt);
        return encryptor.DoCrypt(data);
    }

    /// <summary>
    /// Decrypt <paramref name="data"/> with AES
    /// </summary>
    public static byte[] DecryptAes(this byte[] data, byte[] key, byte[] salt)
    {
        using var aes = Aes.Create();
        using var encryptor = aes.CreateDecryptor(key, salt);
        return encryptor.DoCrypt(data);
    }
}
