using System;
using System.Text;
using System.Text.Json;
using PasswordManager.Abstractions.Crypto;
using PasswordManager.Abstractions.Models;
using PasswordManager.Aes.Extensions;
using PasswordManager.Aes.Generators;
using PasswordManager.Aes.Validators;

namespace PasswordManager.Aes.Crypto;

/// <summary>
/// Crypt with <see cref="Aes"/>
/// </summary>
public sealed class AesCrypto : ICrypto
{
    private readonly AesSaltGenerator saltGenerator = new();
    private readonly SimpleAesKeyValidator keyValidator = new();
    private readonly AesSaltValidator saltValidator = new();

    /// <inheritdoc />
    public EncryptedData Encrypt(byte[] data, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(data);
        keyValidator.Validate(key);
        var salt = saltGenerator.Generate();

        return new EncryptedData
        {
            Salt = salt,
            Data = data.EncryptAes(key, salt)
        };
    }

    /// <inheritdoc />
    public EncryptedData EncryptJson<TItem>(TItem item, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(item);
        var jsonStr = JsonSerializer.Serialize(item);
        var data = Encoding.UTF8.GetBytes(jsonStr);
        return Encrypt(data, key);
    }

    /// <inheritdoc />
    public byte[] Decrypt(EncryptedData data, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Data);
        keyValidator.Validate(key);
        saltValidator.Validate(data.Salt);
        return data.Data.DecryptAes(key, data.Salt);
    }

    /// <inheritdoc />
    public TItem DecryptJson<TItem>(EncryptedData data, byte[] key)
    {
        var decryptedData = Decrypt(data, key);
        return JsonSerializer.Deserialize<TItem>(decryptedData);
    }
}
