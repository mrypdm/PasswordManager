using System;
using System.Linq;
using System.Security.Cryptography;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Validators;
using PasswordManager.Aes.Crypto;

namespace PasswordManager.Aes.Validators;

/// <summary>
/// Extended validator for AES key
/// </summary>
public class ExtendedAesKeyValidator : IKeyValidator
{
    private readonly AesCrypto _crypto = new();
    private readonly SimpleAesKeyValidator _simpleValidator = new();
    private readonly EncryptedData _keyData;

    public ExtendedAesKeyValidator(EncryptedData keyData)
    {
        ArgumentNullException.ThrowIfNull(keyData);
        ArgumentNullException.ThrowIfNull(keyData.Data);
        ArgumentNullException.ThrowIfNull(keyData.Salt);
        _keyData = keyData;
    }

    public void Validate(byte[] key)
    {
        _simpleValidator.Validate(key);
        try
        {
            var decryptedData = _crypto.Decrypt(_keyData, key);
            if (!key.SequenceEqual(decryptedData))
            {
                throw new KeyValidationException();
            }
        }
        catch (CryptographicException)
        {
            throw new KeyValidationException();
        }
    }
}
