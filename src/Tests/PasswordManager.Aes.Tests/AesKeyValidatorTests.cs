using System;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Aes.Validators;

namespace PasswordManager.Aes.Tests;

/// <summary>
/// Tests for <see cref="SimpleAesKeyValidator"/>
/// </summary>
public class AesKeyValidatorTests
{
    [Test]
    public void Validate_CommonWay_ShouldNotThrow()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.KeySize);
        var validator = new SimpleAesKeyValidator();

        // act
        // assert
        validator.Validate(key);
    }

    [Test]
    public void Validate_Null_ShouldThrow()
    {
        // arrange
        var validator = new SimpleAesKeyValidator();

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(null));
    }

    [Test]
    public void Validate_WrongSize_ShouldThrow()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.KeySize - 1);
        var validator = new SimpleAesKeyValidator();

        // act
        // assert
        Assert.Throws<KeyValidationException>(() => validator.Validate(key));
    }
}
