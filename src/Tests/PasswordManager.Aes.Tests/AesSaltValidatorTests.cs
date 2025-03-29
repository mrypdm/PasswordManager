using System;
using PasswordManager.Abstractions.Exceptions;

namespace PasswordManager.Aes.Tests;

/// <summary>
/// Tests for <see cref="AesSaltValidator"/>
/// </summary>
public class AesSaltValidatorTests
{
    [Test]
    public void Validate_SuccessWay()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.BlockSize);
        var validator = new AesSaltValidator();

        // act
        // assert
        validator.Validate(key);
    }

    [Test]
    public void Validate_Null_Throw()
    {
        // arrange
        var validator = new AesSaltValidator();

        // act
        // assert
        Assert.Throws<SaltValidationException>(() => validator.Validate(null));
    }

    [Test]
    public void Validate_WrongSize_Throw()
    {
        // arrange
        var key = (byte[])Array.CreateInstance(typeof(byte), AesConstants.KeySize - 1);
        var validator = new AesSaltValidator();

        // act
        // assert
        Assert.Throws<SaltValidationException>(() => validator.Validate(key));
    }
}
