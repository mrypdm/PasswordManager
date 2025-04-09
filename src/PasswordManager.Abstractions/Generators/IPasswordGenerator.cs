namespace PasswordManager.Abstractions.Generators;

/// <summary>
/// Password generator
/// </summary>
public interface IPasswordGenerator
{
    /// <summary>
    /// Generate password with <paramref name="length"/>
    /// </summary>
    string Generate(int length);
}
