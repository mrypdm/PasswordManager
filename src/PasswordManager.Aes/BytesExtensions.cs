using PasswordManager.Abstractions.Extensions;

using AES = System.Security.Cryptography.Aes;

namespace PasswordManager.Aes;

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
        using var aes = AES.Create();
        using var encryptor = aes.CreateEncryptor(key, salt);
        return encryptor.DoCrypt(data);
    }

    /// <summary>
    /// Decrypt <paramref name="data"/> with AES
    /// </summary>
    public static byte[] DecryptAes(this byte[] data, byte[] key, byte[] salt)
    {
        using var aes = AES.Create();
        using var encryptor = aes.CreateDecryptor(key, salt);
        return encryptor.DoCrypt(data);
    }
}
