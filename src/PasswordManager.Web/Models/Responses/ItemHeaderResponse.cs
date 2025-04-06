namespace PasswordManager.Web.Models.Responses;

/// <summary>
/// Get item header response
/// </summary>
public class ItemHeaderResponse(int id, string name)
{
    /// <summary>
    /// Id of item
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Name of item
    /// </summary>
    public string Name { get; } = name;
}
