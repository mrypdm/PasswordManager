namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Account header
/// </summary>
public class AccountHeader : IItem
{
    /// <summary>
    /// Account ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Account name
    /// </summary>
    public string Name { get; set; }
}
