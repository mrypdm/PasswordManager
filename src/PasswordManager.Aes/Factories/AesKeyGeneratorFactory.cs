using System;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Options;
using PasswordManager.Aes.Generators;

namespace PasswordManager.Aes.Factories;

/// <summary>
/// Factory for <see cref="AesKeyGenerator"/>
/// </summary>
public class AesKeyGeneratorFactory : IKeyGeneratorFactory
{
    /// <inheritdoc />
    public IKeyGenerator Create(IKeyGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new AesKeyGenerator(options.Salt, options.Iterations);
    }
}
