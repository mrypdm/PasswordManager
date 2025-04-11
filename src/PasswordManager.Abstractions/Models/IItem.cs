namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Item model
/// </summary>
public interface IItem
{
    /// <summary>
    /// Item ID
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Item name
    /// </summary>
    string Name { get; set; }
}
