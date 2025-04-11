using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Validators;
using PasswordManager.Aes.Validators;

namespace PasswordManager.Aes.Factories;

/// <summary>
/// Factory for <see cref="SimpleAesKeyValidator"/>
/// </summary>
public class AesKeyValidatorFactory : IKeyValidatorFactory
{
    /// <inheritdoc />
    public IKeyValidator Create(EncryptedData keyData)
    {
        if (keyData is null || keyData.Data is null || keyData.Salt is null)
        {
            return new SimpleAesKeyValidator();
        }

        return new ExtendedAesKeyValidator(keyData);
    }
}
