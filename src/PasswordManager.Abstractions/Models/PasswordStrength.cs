namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Strength of password
/// </summary>
public enum PasswordStrength
{
    VeryLow = 0,
    Low,
    Medium,
    High,
    VeryHigh,
    Unknown = 255,
}
