using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Aes.Generators;

namespace PasswordManager.Aes.Factories;

/// <summary>
/// Factory for <see cref="AesKeyGenerator"/>
/// </summary>
public class AesKeyGeneratorFactory : IKeyGeneratorFactory
{
    /// <inheritdoc />
    public IKeyGenerator Create(byte[] salt, int iterations)
    {
        return new AesKeyGenerator(salt, iterations);
    }
}
