using System;
using System.IO;
using System.Security.Cryptography;

using _Aes = System.Security.Cryptography.Aes;

namespace PasswordManager.Aes.Extensions;

/// <summary>
/// Extensions for <see cref="byte[]"/>
/// </summary>
public static class CryptographicExtensions
{
    /// <summary>
    /// Encrypt <paramref name="data"/> with AES
    /// </summary>
    public static byte[] EncryptAes(this byte[] data, byte[] key, byte[] salt)
    {
        using var aes = _Aes.Create();
        using var encryptor = aes.CreateEncryptor(key, salt);
        return encryptor.DoCrypt(data);
    }

    /// <summary>
    /// Decrypt <paramref name="data"/> with AES
    /// </summary>
    public static byte[] DecryptAes(this byte[] data, byte[] key, byte[] salt)
    {
        using var aes = _Aes.Create();
        using var encryptor = aes.CreateDecryptor(key, salt);
        return encryptor.DoCrypt(data);
    }

    /// <summary>
    /// Applies <paramref name="crypto"/> to <paramref name="data"/>
    /// </summary>
    public static byte[] DoCrypt(this ICryptoTransform crypto, ReadOnlySpan<byte> data)
    {
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, crypto, CryptoStreamMode.Write);
        cryptoStream.Write(data);
        cryptoStream.FlushFinalBlock();
        return memoryStream.ToArray();
    }
}
