namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Status of passowrd compromisation
/// </summary>
public enum PasswordCompromisation
{
    Compromised = 0,
    NotCompromised = 1,
    Unknown = 255,
}
