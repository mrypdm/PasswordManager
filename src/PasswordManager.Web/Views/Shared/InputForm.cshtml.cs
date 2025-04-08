namespace PasswordManager.Web.Views.Shared;

/// <summary>
/// Form for user input
/// </summary>
internal class InputFormModel(string id, string type, string name, string value = null)
{
    public string Id { get; } = id;

    public string Type { get; } = type;

    public string Name { get; } = name;

    public string Value { get; } = value;

    public bool NeedCopy { get; init; }

    public bool NeedShow { get; init; }

    public bool NeedVerify { get; init; }
}
