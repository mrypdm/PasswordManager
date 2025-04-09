using System;
using PasswordManager.Abstractions.Exceptions;

namespace PasswordManager.Aes.Tests;

/// <summary>
/// Tests for <see cref="AesKeyValidator"/>
/// </summary>
public class AesKeyValidatorTests
{
    [Test]
    public void Validate_CommonWay_ShouldNotThrow()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.KeySize);
        var validator = new AesKeyValidator();

        // act
        // assert
        validator.Validate(key);
    }

    [Test]
    public void Validate_Null_ShouldThrow()
    {
        // arrange
        var validator = new AesKeyValidator();

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(null));
    }

    [Test]
    public void Validate_WrongSize_ShouldThrow()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.KeySize - 1);
        var validator = new AesKeyValidator();

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(key));
    }
}
