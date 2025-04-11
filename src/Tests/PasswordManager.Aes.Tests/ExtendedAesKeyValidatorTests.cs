using System;
using System.Security.Cryptography;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Aes.Crypto;
using PasswordManager.Aes.Validators;

namespace PasswordManager.Aes.Tests;

/// <summary>
/// Tests for <see cref="ExtendedAesKeyValidatorTests"/>
/// </summary>
public class ExtendedAesKeyValidatorTests
{
    [Test]
    public void Validate_CommonWay_ShouldNotThrow()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.KeySize);
        var data = new AesCrypto().Encrypt(key, key);
        var validator = new ExtendedAesKeyValidator(data);

        // act
        // assert
        validator.Validate(key);
    }

    [Test]
    public void Validate_CanDecryptButWrongData_ShouldThrow()
    {
        // arrange
        var key = RandomNumberGenerator.GetBytes(AesConstants.KeySize);
        var wrongKey = MakeBadKey(key);
        var data = new AesCrypto().Encrypt(wrongKey, key);
        var validator = new ExtendedAesKeyValidator(data);

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(key));
    }

    [Test]
    public void Validate_CannotDecrypt_ShouldThrow()
    {
        // arrange
        var key = RandomNumberGenerator.GetBytes(AesConstants.KeySize);
        var wrongKey = MakeBadKey(key);
        var data = new AesCrypto().Encrypt(key, key);
        var validator = new ExtendedAesKeyValidator(data);

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(wrongKey));
    }

    [Test]
    public void Ctor_NullData_ShouldThrow()
    {
        // arrange
        // act
        // assert
        Assert.Throws<ArgumentNullException>(() => new ExtendedAesKeyValidator(null));
        Assert.Throws<ArgumentNullException>(
            () => new ExtendedAesKeyValidator(new EncryptedData { Data = null, Salt = [] }));
        Assert.Throws<ArgumentNullException>(
            () => new ExtendedAesKeyValidator(new EncryptedData { Data = [], Salt = null }));
    }

    [Test]
    public void Validate_Null_ShouldThrow()
    {
        // arrange
        var validator = new ExtendedAesKeyValidator(new EncryptedData { Data = [], Salt = [] });

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(null));
    }

    [Test]
    public void Validate_WrongSize_ShouldThrow()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.KeySize - 1);
        var validator = new ExtendedAesKeyValidator(new EncryptedData { Data = [], Salt = [] });

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(key));
    }

    private static byte[] MakeBadKey(byte[] key)
    {
        return [key[1], key[0], .. key[2..]];
    }
}
