using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using PasswordManager.Abstractions.Options;
using PasswordManager.Core.Converters;

namespace PasswordManager.Core.Options;

/// <summary>
/// User options
/// </summary>
public class UserOptions : IKeyGeneratorOptions
{
    /// <summary>
    /// Salt for key generation in bytes
    /// </summary>
    [JsonConverter(typeof(JsonStringBytesConverter))]
    public byte[] Salt { get; set; } = RandomNumberGenerator.GetBytes(16);

    /// <summary>
    /// Count of iterations for key generation
    /// </summary>
    public int Iterations { get; set; } = 100;

    /// <summary>
    /// Timeout of session
    /// </summary>
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(15);
}
