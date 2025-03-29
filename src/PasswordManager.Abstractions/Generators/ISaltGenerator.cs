namespace PasswordManager.Abstractions.Generators;

/// <summary>
/// Salt generator
/// </summary>
public interface ISaltGenerator
{
    /// <summary>
    /// Generate salt
    /// </summary>
    byte[] Generate();
}
