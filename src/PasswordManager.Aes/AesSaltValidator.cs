using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Validators;

namespace PasswordManager.Aes;

/// <inheritdoc />
public sealed class AesSaltValidator : ISaltValidator
{
    /// <inheritdoc />
    public void Validate(byte[] salt)
    {
        if (salt is null)
        {
            throw new SaltValidationException("Salt is null");
        }

        if (salt.Length != AesConstants.BlockSize)
        {
            throw new SaltValidationException($"Salt size must be {AesConstants.BlockSize} but was {salt.Length}");
        }
    }
}
