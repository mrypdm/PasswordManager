using System;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Options;

namespace PasswordManager.Abstractions.Factories;

/// <summary>
/// Factory for <see cref="IKeyGenerator"/>
/// </summary>
public interface IKeyGeneratorFactory
{
    /// <summary>
    /// Create key generator
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="options"/> is null</exception>
    IKeyGenerator Create(IKeyGeneratorOptions options);
}
