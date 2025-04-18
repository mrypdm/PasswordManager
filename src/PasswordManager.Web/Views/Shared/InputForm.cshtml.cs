namespace PasswordManager.Web.Views.Shared;

/// <summary>
/// Form for user input
/// </summary>
internal class InputFormModel(string id, string type, string name, string value = null)
{
    /// <summary>
    /// Id of form
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Type of form
    /// </summary>
    public string Type { get; } = type;

    /// <summary>
    /// Name of form
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Value of form
    /// </summary>
    public string Value { get; } = value;

    /// <summary>
    /// Add 'copy' button for form
    /// </summary>
    public bool NeedCopy { get; init; }

    /// <summary>
    /// Add checkbox for showing hidden text in form
    /// </summary>
    public bool NeedShow { get; init; }

    /// <summary>
    /// Add 'verify' button for form
    /// </summary>
    public bool NeedVerify { get; init; }
}
