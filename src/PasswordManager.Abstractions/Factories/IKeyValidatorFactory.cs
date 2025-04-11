using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Validators;

namespace PasswordManager.Abstractions.Factories;

/// <summary>
/// Factory for <see cref="IKeyValidator"/>
/// </summary>
public interface IKeyValidatorFactory
{
    /// <summary>
    /// Create validator with key data
    /// </summary>
    IKeyValidator Create(EncryptedData keyData);
}
