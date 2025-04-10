using System;

namespace PasswordManager.Abstractions.Generators;

/// <summary>
/// Factory for key
/// </summary>
public interface IKeyGenerator
{
    /// <summary>
    /// Create key by password
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="password"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="password"/> is whitespace</exception>
    byte[] Generate(string password);
}
