using PasswordManager.Abstractions.Exceptions;

namespace PasswordManager.Abstractions.Validators;

/// <summary>
/// Validator of encryption salts
/// </summary>
public interface ISaltValidator
{
    /// <summary>
    /// Validate salt
    /// </summary>
    /// <exception cref="SaltValidationException">If salt is invalid</exception>
    void Validate(byte[] salt);
}
