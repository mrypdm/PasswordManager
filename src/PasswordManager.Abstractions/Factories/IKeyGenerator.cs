using System;

namespace PasswordManager.Abstractions.Factories;

/// <summary>
/// Factory for master key
/// </summary>
public interface IKeyGenerator
{
    /// <summary>
    /// Create master key by master password
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="masterPassword"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="masterPassword"/> is whitespace</exception>
    byte[] Generate(string masterPassword);
}
