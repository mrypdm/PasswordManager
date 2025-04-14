namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Status of password
/// </summary>
public class PasswordCheckStatus(PasswordCompromisation isCompromised, PasswordStrength strength)
{
    /// <summary>
    /// Password compromisation status
    /// </summary>
    public PasswordCompromisation Compomisation { get; private set; } = isCompromised;

    /// <summary>
    /// Strength of password
    /// </summary>
    public PasswordStrength Strength { get; private set; } = strength;

    /// <summary>
    /// Calculates minimal check status based of <paramref name="left"/> and <paramref name="right"/>
    /// </summary>
    public static PasswordCheckStatus MinOf(PasswordCheckStatus left, PasswordCheckStatus right)
    {
        return new PasswordCheckStatus(
            left.Compomisation > right.Compomisation ? right.Compomisation : left.Compomisation,
            left.Strength > right.Strength ? right.Strength : left.Strength
        );
    }
}
