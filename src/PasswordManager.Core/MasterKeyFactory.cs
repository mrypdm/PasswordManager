using System.Security.Cryptography;
using System.Text;

using PasswordManager.Abstractions;

namespace PasswordManager.Core;

/// <inheritdoc />
public class MasterKeyFactory(byte[] salt, int iterations) : IMasterKeyFactory
{
    public const int AesKeyLength = 256 / 8;

    /// <inheritdoc />
    public byte[] CreateMasterKey(string masterPassword)
    {
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.ASCII.GetBytes(masterPassword), salt, iterations, HashAlgorithmName.SHA256, AesKeyLength);
    }
}
