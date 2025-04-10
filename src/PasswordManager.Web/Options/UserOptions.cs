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
    /// Salt for key generation
    /// </summary>
    public string Salt { get; set; } = RandomNumberGenerator.GetHexString(AesConstants.BlockSize * 2);

    /// <summary>
    /// Salt for key generation in bytes
    /// </summary>
    [JsonIgnore]
    public byte[] SaltBytes => Convert.FromHexString(Salt);

    /// <summary>
    /// Count of iterations for key generation
    /// </summary>
    public int Iterations { get; set; } = 100;

    /// <summary>
    /// Timeout of session
    /// </summary>
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(15);
}
