using System;
using System.Runtime.Caching;
using PasswordManager.Abstractions.Validators;
using PasswordManager.SecureData.Exceptions;

namespace PasswordManager.SecureData.KeyStorage;

/// <summary>
/// Storage for master key
/// </summary>
public sealed class MasterKeyStorage(IKeyValidator keyValidator) : IMasterKeyStorage, IDisposable
{
    private const string MasterKeyCacheKey = nameof(MasterKeyCacheKey);
    private const string BlockStorageCacheKey = nameof(BlockStorageCacheKey);

    private readonly MemoryCache _cache = new(nameof(MasterKeyStorage));

    /// <inheritdoc />
    public byte[] MasterKey
    {
        get
        {
            ThrowIfNotInitialized();
            return _cache.Get(MasterKeyCacheKey) as byte[];
        }
    }

    /// <inheritdoc />
    public bool IsInitialized => _cache.Get(MasterKeyCacheKey) is not null;

    /// <inheritdoc />
    public void InitStorage(byte[] masterKey, TimeSpan timeout)
    {
        ThrowIfBlocked();
        keyValidator.Validate(masterKey);
        ValidateTimeout(timeout);

        ClearKey();
        _cache.Set(MasterKeyCacheKey, masterKey, new CacheItemPolicy { SlidingExpiration = timeout });
    }

    /// <inheritdoc />
    public void ChangeTimeout(TimeSpan timeout)
    {
        ValidateTimeout(timeout);
        ThrowIfBlocked();
        ThrowIfNotInitialized();

        _cache.Set(MasterKeyCacheKey, MasterKey, new CacheItemPolicy { SlidingExpiration = timeout });
    }

    /// <inheritdoc />
    public void ClearKey()
    {
        _cache.Remove(MasterKeyCacheKey);
    }

    /// <inheritdoc />
    public void Block(TimeSpan timeout)
    {
        ClearKey();
        _cache.Set(BlockStorageCacheKey, BlockStorageCacheKey, new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.UtcNow.Add(timeout)
        });
    }

    /// <inheritdoc />
    public void ThrowIfBlocked()
    {
        if (_cache.Get(BlockStorageCacheKey) is not null)
        {
            throw new StorageBlockedException();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cache.Dispose();
    }

    private void ThrowIfNotInitialized()
    {
        if (!IsInitialized)
        {
            throw new StorageNotInitializedException($"Storage is empty. Call {nameof(InitStorage)} first");
        }
    }

    private static void ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentException("Timeout cannot be zero or negative");
        }
    }
}
