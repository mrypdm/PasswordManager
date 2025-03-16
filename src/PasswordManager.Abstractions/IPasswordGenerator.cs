namespace PasswordManager.Abstractions;

/// <summary>
/// Password generator
/// </summary>
public interface IPasswordGenerator
{
    /// <summary>
    /// Generate password with <paramref name="length"/> with characters from <paramref name="alphabet"/>
    /// </summary>
    string GeneratePassword(IAlphabet alphabet, int length);
}
