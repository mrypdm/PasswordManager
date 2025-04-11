using System;
using System.Linq;
using System.Security.Cryptography;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Aes.Crypto;

namespace PasswordManager.Aes.Tests;

/// <summary>
/// Tests for <see cref="AesCrypto"/>
/// </summary>
public class AesCryptoTests
{
    [Test]
    public void EncryptWithDecrypt_ShouldBeEqualToEtalon()
    {
        // arrange
        var data = RandomNumberGenerator.GetBytes(100);
        var key = RandomNumberGenerator.GetBytes(AesConstants.KeySize);
        var crypto = new AesCrypto();

        // act
        var encrypted1 = crypto.Encrypt(data, key);
        var encrypted2 = crypto.Encrypt(data, key);

        var decrypted1 = crypto.Decrypt(encrypted1, key);
        var decrypted2 = crypto.Decrypt(encrypted2, key);

        // assert
        CollectionAssert.AreEqual(data, decrypted1, "Decrypted value should be equal to etalon");
        Assert.That(decrypted1.SequenceEqual(decrypted1), Is.True, "Decrypted values should be equal");
    }

    [Test]
    public void EncryptWithDecrypt_Json_ShouldBeEqualToEtalon()
    {
        // arrange
        var data = "very cool string";
        var key = RandomNumberGenerator.GetBytes(AesConstants.KeySize);
        var crypto = new AesCrypto();

        // act
        var encrypted1 = crypto.EncryptJson(data, key);
        var encrypted2 = crypto.EncryptJson(data, key);

        var decrypted1 = crypto.DecryptJson<string>(encrypted1, key);
        var decrypted2 = crypto.DecryptJson<string>(encrypted2, key);

        // assert
        Assert.That(decrypted1, Is.EqualTo(data));
        Assert.That(decrypted2, Is.EqualTo(data));
    }

    [Test]
    public void Encrypt_ShouldBeDifferentEncryptedValues()
    {
        // arrange
        var data = RandomNumberGenerator.GetBytes(100);
        var key = RandomNumberGenerator.GetBytes(AesConstants.KeySize);
        var crypto = new AesCrypto();

        // act
        var encrypted1 = crypto.Encrypt(data, key);
        var encrypted2 = crypto.Encrypt(data, key);

        // assert
        Assert.That(encrypted1.Data.SequenceEqual(encrypted2.Data), Is.False, "Encrypted values should be different");
    }

    [Test]
    public void Decrypt_WrongSalt_ShouldBeTrash()
    {
        // arrange
        var data = RandomNumberGenerator.GetBytes(100);
        var key = RandomNumberGenerator.GetBytes(AesConstants.KeySize);
        var crypto = new AesCrypto();

        // act
        var encrypted = crypto.Encrypt(data, key);
        encrypted.Salt = RandomNumberGenerator.GetBytes(AesConstants.BlockSize);
        var decrypted = crypto.Decrypt(encrypted, key);

        // assert
        CollectionAssert.AreNotEqual(data, decrypted, "Decrypted with wrong salt should be not equal to etalon");
    }

    [Test]
    public void InvalidKey_ShouldThrow()
    {
        // arrange
        var crypto = new AesCrypto();
        var data = new EncryptedData
        {
            Data = [],
            Salt = []
        };

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => crypto.Encrypt([], null));
        Assert.Throws<KeyValidationException>(() => crypto.Decrypt(data, null));
        Assert.Throws<KeyValidationException>(() => crypto.EncryptJson<byte[]>([], null));
        Assert.Throws<KeyValidationException>(() => crypto.DecryptJson<byte[]>(data, null));
        Assert.Throws<KeyValidationException>(() => crypto.Encrypt([], [1]));
        Assert.Throws<KeyValidationException>(() => crypto.Decrypt(data, [1]));
        Assert.Throws<KeyValidationException>(() => crypto.EncryptJson<byte[]>([], [1]));
        Assert.Throws<KeyValidationException>(() => crypto.DecryptJson<byte[]>(data, [1]));
    }

    [Test]
    public void NullSalt_ShouldThrow()
    {
        // arrange
        var crypto = new AesCrypto();
        var badData = new EncryptedData
        {
            Data = [],
            Salt = null
        };

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => crypto.Decrypt(badData, null));
        Assert.Throws<KeyValidationException>(() => crypto.DecryptJson<byte[]>(badData, null));
    }

    [Test]
    public void WrongSizeSalt_ShouldThrow()
    {
        // arrange
        var crypto = new AesCrypto();
        var badData = new EncryptedData
        {
            Data = [],
            Salt = [1]
        };

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => crypto.Decrypt(badData, null));
        Assert.Throws<KeyValidationException>(() => crypto.DecryptJson<byte[]>(badData, null));
    }

    [Test]
    public void NullData_ShouldThrow()
    {
        // arrange
        var crypto = new AesCrypto();
        var badData = new EncryptedData
        {
            Data = null,
            Salt = null
        };

        // act
        // assert
        Assert.Throws<ArgumentNullException>(() => crypto.Encrypt(null, null));
        Assert.Throws<ArgumentNullException>(() => crypto.EncryptJson<byte[]>(null, null));
        Assert.Throws<ArgumentNullException>(() => crypto.Decrypt(null, null));
        Assert.Throws<ArgumentNullException>(() => crypto.DecryptJson<byte[]>(null, null));
        Assert.Throws<ArgumentNullException>(() => crypto.Decrypt(badData, null));
        Assert.Throws<ArgumentNullException>(() => crypto.DecryptJson<byte[]>(badData, null));
    }
}
