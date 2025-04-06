using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Moq;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.KeyStorage;

namespace PasswordManager.SecureData.Tests;

/// <summary>
/// Tests for <see cref="MasterKeyStorage"/>
/// </summary>
public class MasterKeyStorageServiceTests
{
    private readonly Mock<IKeyValidator> _validatorMock = new();

    [SetUp]
    public void SetUp()
    {
        _validatorMock.Reset();
    }

    [Test]
    public void MasterKey_NotInitialized_ShouldThrow()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.Throws<StorageNotInitializedException>(() => { storage.MasterKey.Any(); });
    }

    [Test]
    public void IsInitialized_NotInitialized()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.That(storage.IsInitialized, Is.False);
    }

    [Test]
    public void ChangeTimeout_ShouldThrow()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.Throws<StorageNotInitializedException>(() => storage.ChangeTimeout(TimeSpan.MaxValue));
    }

    [Test]
    public void ChangeTimeout_ShouldThrow_InvalidTimeout_ShouldThrow()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.Throws<ArgumentException>(() => storage.InitStorage(null, TimeSpan.Zero));
    }

    [Test]
    public void Initialize_NotInitialized_ShoudValidateKeyAndInitStorage()
    {
        // arrange
        var key = RandomNumberGenerator.GetBytes(32);
        using var storage = CreateStorage();

        // act
        storage.InitStorage(key, TimeSpan.FromSeconds(10));

        // assert
        Assert.That(storage.IsInitialized, Is.True);
        _validatorMock.Verify(m => m.Validate(key), Times.Once);
    }

    [Test]
    public void Initialize_InvalidTimeout_ShouldThrow()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.Throws<ArgumentException>(() => storage.InitStorage(null, TimeSpan.Zero));
    }

    [Test]
    public async Task Initialize_ShoudBeClearedAfterTimeout()
    {
        // arrange
        var key = RandomNumberGenerator.GetBytes(32);
        using var storage = CreateStorage();

        // act
        storage.InitStorage(key, TimeSpan.FromSeconds(2));
        await Task.Delay(TimeSpan.FromSeconds(3));

        // assert
        Assert.That(storage.IsInitialized, Is.False);
    }

    [Test]
    public void ClearKey_ShouldClearKey()
    {
        // arrange
        var key = RandomNumberGenerator.GetBytes(32);
        using var storage = CreateStorage();

        // act
        storage.InitStorage(key, TimeSpan.FromSeconds(3));
        storage.ClearKey();

        // assert
        Assert.That(storage.IsInitialized, Is.False);
    }

    private MasterKeyStorage CreateStorage()
    {
        return new MasterKeyStorage(_validatorMock.Object);
    }
}
