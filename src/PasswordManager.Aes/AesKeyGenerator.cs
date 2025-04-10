using System;
using System.Security.Cryptography;
using System.Text;
using PasswordManager.Abstractions.Generators;

namespace PasswordManager.Aes;

/// <inheritdoc />
public sealed class AesKeyGenerator(byte[] salt, int iterations) : IKeyGenerator
{
    private readonly byte[] _salt = salt ?? throw new ArgumentNullException(nameof(salt));

    /// <inheritdoc />
    public byte[] Generate(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.ASCII.GetBytes(password), _salt, iterations,
            HashAlgorithmName.SHA256, AesConstants.KeySize);
    }
}
