namespace PasswordManager.Abstractions.Factories;

/// <summary>
/// Factory for <see cref="IKeyGenerator"/>
/// </summary>
public interface IKeyGeneratorFactory
{
    /// <summary>
    /// Create key generator
    /// </summary>
    IKeyGenerator Create(byte[] salt, int iterations);
}
