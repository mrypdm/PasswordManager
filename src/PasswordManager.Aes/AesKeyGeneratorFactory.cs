using PasswordManager.Abstractions.Factories;

namespace PasswordManager.Aes;

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
