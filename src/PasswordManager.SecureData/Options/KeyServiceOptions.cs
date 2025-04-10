using System;
using PasswordManager.SecureData.Services;

namespace PasswordManager.SecureData.Options;

/// <summary>
/// Options for <see cref="KeyService"/>
/// </summary>
public class KeyServiceOptions
{
    /// <summary>
    /// Count of attempts to init key
    /// </summary>
    public int MaxAttemptCounts { get; set; }

    /// <summary>
    /// Timeout for block new attempts after <see cref="MaxAttemptCounts"/> is reached
    /// </summary>
    public TimeSpan BlockTimeout { get; set; }
}
