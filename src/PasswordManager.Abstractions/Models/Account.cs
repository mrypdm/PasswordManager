namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Account
/// </summary>
public class Account : AccountHeader
{
    /// <summary>
    /// Data of account
    /// </summary>
    public AccountData Data { get; set; }
}
