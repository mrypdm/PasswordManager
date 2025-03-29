using PasswordManager.Abstractions.Exceptions;

namespace PasswordManager.Abstractions.Validators;

/// <summary>
/// Validator of encryption keys
/// </summary>
public interface IKeyValidator
{
    /// <summary>
    /// Validate key
    /// </summary>
    /// <exception cref="KeyValidationException">If key is invalid</exception>
    void Validate(byte[] key);
}
