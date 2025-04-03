using System;
using System.Runtime.Caching;

namespace PasswordManager.SecureData.KeyStorage;

/// <summary>
/// Storage for master key
/// </summary>
public sealed class MasterKeyStorage : IMasterKeyStorage, IDisposable
{
    private const string CacheKey = "MasterKey";

    private readonly MemoryCache _cache = new(nameof(MasterKeyStorage));

    /// <inheritdoc />
    public byte[] MasterKey => _cache.Get(CacheKey) as byte[];

    /// <inheritdoc />
    public void InitStorage(byte[] masterKey, TimeSpan keyLifeTime)
    {
        ClearKey();
        _cache.Set(CacheKey, masterKey, new CacheItemPolicy { SlidingExpiration = keyLifeTime });
    }

    /// <inheritdoc />
    public void ChangeLifetime(TimeSpan keyLifeTime)
    {
        ThrowIfNotInitialized();
        _cache.Set(CacheKey, MasterKey, new CacheItemPolicy { SlidingExpiration = keyLifeTime });
    }

    /// <inheritdoc />
    public void ClearKey()
    {
        _cache.Remove(CacheKey);
    }

    /// <inheritdoc />
    public bool IsInitialized()
    {
        return MasterKey is not null;
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
            throw new InvalidOperationException($"Storage is empty. Call {nameof(InitStorage)} first");
        }
    }
}
