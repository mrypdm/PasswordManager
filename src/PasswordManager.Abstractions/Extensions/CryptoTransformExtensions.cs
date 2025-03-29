using System;
using System.IO;
using System.Security.Cryptography;

namespace PasswordManager.Abstractions.Extensions;

/// <summary>
/// Extensions for <see cref="ICryptoTransform"/>
/// </summary>
public static class CryptoTransformExtensions
{
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
