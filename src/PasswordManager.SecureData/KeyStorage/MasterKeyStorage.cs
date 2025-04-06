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
    private const string CacheKey = "MasterKey";

    private readonly MemoryCache _cache = new(nameof(MasterKeyStorage));

    /// <inheritdoc />
    public byte[] MasterKey => _cache.Get(CacheKey) as byte[];

    /// <inheritdoc />
    public bool IsInitialized => MasterKey is not null;

    /// <inheritdoc />
    public void InitStorage(byte[] masterKey, TimeSpan timeout)
    {
        keyValidator.Validate(masterKey);
        ValidateTimeout(timeout);

        ClearKey();
        _cache.Set(CacheKey, masterKey, new CacheItemPolicy { SlidingExpiration = timeout });
    }

    /// <inheritdoc />
    public void ChangeTimeout(TimeSpan timeout)
    {
        ValidateTimeout(timeout);
        ThrowIfNotInitialized();

        _cache.Set(CacheKey, MasterKey, new CacheItemPolicy { SlidingExpiration = timeout });
    }

    /// <inheritdoc />
    public void ClearKey()
    {
        _cache.Remove(CacheKey);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cache.Dispose();
    }

    private void ThrowIfNotInitialized()
    {
        if (MasterKey is null)
        {
            throw new StorageIsNotInitializedException($"Storage is empty. Call {nameof(InitStorage)} first");
        }
    }

    private void ValidateTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentException("Timeout cannot be zero or negative");
        }
    }
}
