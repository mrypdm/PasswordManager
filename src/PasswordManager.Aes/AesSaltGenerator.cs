using System.Security.Cryptography;
using PasswordManager.Abstractions.Generators;

namespace PasswordManager.Aes;

/// <inheritdoc />
public class AesSaltGenerator : ISaltGenerator
{
    /// <inheritdoc />
    public byte[] Generate()
    {
        return RandomNumberGenerator.GetBytes(AesConstants.BlockSize);
    }
}
