namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Account data
/// </summary>
public sealed class AccountData : AccountHeader
{
    /// <summary>
    /// User login
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// User password
    /// </summary>
    public string Password { get; set; }
}
