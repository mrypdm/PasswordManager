using System.Security.Cryptography;
using PasswordManager.Abstractions.Generators;

namespace PasswordManager.Aes.Generators;

/// <inheritdoc />
public sealed class AesSaltGenerator : ISaltGenerator
{
    /// <inheritdoc />
    public byte[] Generate()
    {
        return RandomNumberGenerator.GetBytes(AesConstants.BlockSize);
    }
}
