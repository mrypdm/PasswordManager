using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Moq;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Validators;
using PasswordManager.Core.Storages;

namespace PasswordManager.Core.Tests.Storages;

/// <summary>
/// Tests for <see cref="KeyStorage"/>
/// </summary>
public class KeyStorageServiceTests
{
    private readonly Mock<IKeyValidator> _validatorMock = new();

    [SetUp]
    public void SetUp()
    {
        _validatorMock.Reset();
    }

    [Test]
    public void Key_NotInitialized_ShouldThrow()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.Throws<StorageNotInitializedException>(() => { storage.Key.ToArray(); });
    }

    [Test]
    public void IsInitialized_NotInitialized_ShouldReturnFalse()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.That(storage.IsInitialized, Is.False);
    }

    [Test]
    public void ChangeTimeout_NotInitialized_ShouldThrow()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.Throws<StorageNotInitializedException>(() => storage.ChangeTimeout(TimeSpan.MaxValue));
    }

    [Test]
    public void ChangeTimeout_InvalidTimeout_ShouldThrow()
    {
        // arrange
        using var storage = CreateStorage();

        // act
        // assert
        Assert.Throws<ArgumentException>(() => storage.InitStorage(null, TimeSpan.Zero));
    }

    [Test]
    public void Initialize_NotInitialized_ShouldValidateKeyAndInitStorage()
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
    public async Task Initialize_ShouldBeClearedAfterTimeout()
    {
        // arrange
        var key = RandomNumberGenerator.GetBytes(32);
        using var storage = CreateStorage();

        // act
        storage.InitStorage(key, TimeSpan.FromMilliseconds(500));
        await Task.Delay(TimeSpan.FromSeconds(1));

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

    [Test]
    public void Block_ShouldBlock()
    {
        // arrange
        var seconds = 10;
        using var storage = CreateStorage();

        // act
        storage.Block(TimeSpan.FromSeconds(seconds));

        // assert
        Assert.Throws<StorageBlockedException>(storage.ThrowIfBlocked);
    }

    [Test]
    public async Task Block_ShouldUnblockAfterTimeout()
    {
        // arrange
        var seconds = 2;
        using var storage = CreateStorage();

        // act
        storage.Block(TimeSpan.FromSeconds(seconds));
        await Task.Delay(TimeSpan.FromSeconds(seconds + 1));

        // assert
        storage.ThrowIfBlocked();
    }

    private KeyStorage CreateStorage()
    {
        return new KeyStorage(_validatorMock.Object);
    }
}
