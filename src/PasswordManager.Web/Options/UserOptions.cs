using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using PasswordManager.Aes;

namespace PasswordManager.Web.Options;

/// <summary>
/// User options
/// </summary>
public class UserOptions
{
    /// <summary>
    /// Salt for master key generation
    /// </summary>
    public string MasterKeySalt { get; set; } = RandomNumberGenerator.GetHexString(AesConstants.KeySize * 2);

    /// <summary>
    /// Salt for master key generation in bytes
    /// </summary>
    [JsonIgnore]
    public byte[] MasterKeySaltBytes => Convert.FromHexString(MasterKeySalt);

    /// <summary>
    /// Count of iterations for master key generation
    /// </summary>
    public int MasterKeyIterations { get; set; } = 100;

    /// <summary>
    /// Timeout of session
    /// </summary>
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(15);
}
