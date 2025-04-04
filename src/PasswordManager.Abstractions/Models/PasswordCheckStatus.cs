namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Status of password
/// </summary>
public class PasswordCheckStatus(PasswordCompromisation isCompromised, PasswordStrength strength, double score)
{
    /// <summary>
    /// If password has been compromised
    /// </summary>
    public PasswordCompromisation IsCompromised { get; private set; } = isCompromised;

    /// <summary>
    /// Strength of password
    /// </summary>
    public PasswordStrength Strength { get; private set; } = strength;

    /// <summary>
    /// Score of strength of password 
    /// </summary>
    public double Score { get; private set; } = score;

    /// <summary>
    /// Calculates minimal check status based of <paramref name="left"/> and <paramref name="right"/>
    /// </summary>
    public static PasswordCheckStatus MinOf(PasswordCheckStatus left, PasswordCheckStatus right)
    {
        return new PasswordCheckStatus(
            left.IsCompromised > right.IsCompromised ? right.IsCompromised : left.IsCompromised,
            left.Strength > right.Strength ? right.Strength : left.Strength,
            left.Strength > right.Strength ? right.Score : left.Score
        );
    }
}
