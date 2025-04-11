using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Validators;

namespace PasswordManager.Aes.Validators;

/// <summary>
/// Validator for AES keys
/// </summary>
public sealed class SimpleAesKeyValidator : IKeyValidator
{
    /// <inheritdoc />
    public void Validate(byte[] key)
    {
        if (key is null)
        {
            throw new KeyValidationException("Key is null");
        }

        if (key.Length != AesConstants.KeySize)
        {
            throw new KeyValidationException($"Key size must be {AesConstants.KeySize} but was {key.Length}");
        }
    }
}
