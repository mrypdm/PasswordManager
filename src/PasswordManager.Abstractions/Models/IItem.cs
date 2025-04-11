namespace PasswordManager.Abstractions.Models;

/// <summary>
/// Item model
/// </summary>
public interface IItem
{
    /// <summary>
    /// Id of item
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Name of item
    /// </summary>
    string Name { get; set; }
}
