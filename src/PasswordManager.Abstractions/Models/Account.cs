namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Account
/// </summary>
public class Account : IItem
{
    /// <summary>
    /// Account ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Account name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Data of account
    /// </summary>
    public AccountData Data { get; set; }
}
