namespace PasswordManager.Abstractions.Factories;

/// <summary>
/// Factory for master key
/// </summary>
public interface IKeyGenerator
{
    /// <summary>
    /// Create master key by master password
    /// </summary>
    byte[] Generate(string masterPassword);
}
