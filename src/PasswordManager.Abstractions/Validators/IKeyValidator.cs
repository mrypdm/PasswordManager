namespace PasswordManager.Abstractions.Validators;

/// <summary>
/// Validator of encryption keys
/// </summary>
public interface IKeyValidator
{
    /// <summary>
    /// Validate key
    /// </summary>
    void Validate(byte[] key);
}
