using System;
using System.Runtime.Caching;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Storages;
using PasswordManager.Abstractions.Validators;

namespace PasswordManager.Core.Storages;

/// <inheritdoc cref="IKeyStorage"/>
public sealed class KeyStorage(IKeyValidator keyValidator) : IKeyStorage, IDisposable
{
    private const string KeyCacheKey = nameof(KeyCacheKey);
    private const string BlockStorageCacheKey = nameof(BlockStorageCacheKey);

    private readonly MemoryCache _cache = new(nameof(KeyStorage));

    /// <inheritdoc />
    public byte[] Key
    {
        get
        {
            ThrowIfBlocked();
            ThrowIfNotInitialized();
            return _cache.Get(KeyCacheKey) as byte[];
        }
    }

    /// <inheritdoc />
    public bool IsInitialized => _cache.Get(KeyCacheKey) is not null;

    /// <inheritdoc />
    public void InitStorage(byte[] key, TimeSpan timeout)
    {
        ThrowIfBlocked();
        keyValidator.Validate(key);
        ValidateTimeout(timeout);

        ClearKey();
        _cache.Set(KeyCacheKey, key, new CacheItemPolicy { SlidingExpiration = timeout });
    }

    /// <inheritdoc />
    public void ChangeTimeout(TimeSpan timeout)
    {
        ValidateTimeout(timeout);
        ThrowIfBlocked();
        ThrowIfNotInitialized();

        _cache.Set(KeyCacheKey, Key, new CacheItemPolicy { SlidingExpiration = timeout });
    }

    /// <inheritdoc />
    public void ClearKey()
    {
        ThrowIfBlocked();
        _cache.Remove(KeyCacheKey);
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
