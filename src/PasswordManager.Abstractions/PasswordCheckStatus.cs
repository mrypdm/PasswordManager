namespace PasswordManager.Abstractions;

/// <summary>
/// Status of password
/// </summary>
public class PasswordCheckStatus(PasswordCompromisation isCompromised, PasswordStrength strength)
{
    /// <summary>
    /// If password has been compromised
    /// </summary>
    public PasswordCompromisation IsCompromised { get; private set; } = isCompromised;

    /// <summary>
    /// Strength of password
    /// </summary>
    public PasswordStrength Strength { get; private set; } = strength;
}
