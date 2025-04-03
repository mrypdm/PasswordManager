namespace PasswordManager.Web.Options;

/// <summary>
/// User options
/// </summary>
public class UserOptions
{
    /// <summary>
    /// Salt for master key generation
    /// </summary>
    public string MasterKeySalt { get; set; }

    /// <summary>
    /// Count of iterations for master key generation
    /// </summary>
    public int MasterKeyIterations { get; set; } = 100;

    /// <summary>
    /// Timeout of session
    /// </summary>
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(15);
}
