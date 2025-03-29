using System;
using PasswordManager.Abstractions.Validators;

namespace PasswordManager.Aes;

/// <summary>
/// Validator for AES keys
/// </summary>
public class AesKeyValidator : IKeyValidator
{
    /// <summary>
    /// Validate AES key
    /// </summary>
    public void Validate(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != AesConstants.KeySize)
        {
            throw new ArgumentException($"Key size is not valid. Must be {AesConstants.KeySize} but was {key.Length}");
        }
    }
}
