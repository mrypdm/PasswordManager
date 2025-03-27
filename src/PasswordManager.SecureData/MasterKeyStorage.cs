using System;
using System.Runtime.Caching;
using System.Security.Cryptography;

namespace PasswordManager.SecureData;

/// <summary>
/// Storage for master key
/// </summary>
public sealed class MasterKeyStorage : IMasterKeyStorage, ICryptoProvider, IDisposable
{
    private const string CacheKey = "MasterKey";
    private const int AesKeySize = 256 / 8;

    private readonly MemoryCache _cache = new(nameof(MasterKeyStorage));
    private readonly Aes _aes = Aes.Create();

    /// <inheritdoc />
    public byte[] MasterKey => _cache.Get(CacheKey) as byte[];

    /// <inheritdoc />
    public void InitStorage(byte[] masterKey, TimeSpan keyLifeTime)
    {
        ThrowIfInvalid(masterKey);
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
    public ICryptoTransform CreateEncryptor(byte[] salt)
    {
        ThrowIfNotInitialized();
        return _aes.CreateEncryptor(MasterKey, salt);
    }

    /// <inheritdoc />
    public ICryptoTransform CreateDecryptor(byte[] salt)
    {
        ThrowIfNotInitialized();
        return _aes.CreateDecryptor(MasterKey, salt);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _aes.Dispose();
        _cache.Dispose();
    }

    private void ClearKey()
    {
        var key = _cache.Remove(CacheKey) as byte[];
        if (key is not null)
        {
            Array.Fill<byte>(key, 0);
        }
    }

    private void ThrowIfNotInitialized()
    {
        if (MasterKey is null)
        {
            throw new InvalidOperationException($"Storage is empty. Call {nameof(InitStorage)} first");
        }
    }

    private void ThrowIfInvalid(byte[] masterKey)
    {
        ArgumentNullException.ThrowIfNull(masterKey);
        if (masterKey.Length != AesKeySize)
        {
            throw new ArgumentException($"Key has wrong size. It must be {AesKeySize}, but was {masterKey.Length}", nameof(masterKey));
        }
    }
}
