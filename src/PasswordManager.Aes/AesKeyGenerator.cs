using System.Security.Cryptography;
using System.Text;
using PasswordManager.Abstractions.Factories;

namespace PasswordManager.Aes;

/// <inheritdoc />
public class AesKeyGenerator(byte[] salt, int iterations) : IKeyGenerator
{
    /// <inheritdoc />
    public byte[] Generate(string masterPassword)
    {
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.ASCII.GetBytes(masterPassword), salt, iterations,
            HashAlgorithmName.SHA256, AesConstants.KeySize);
    }
}
