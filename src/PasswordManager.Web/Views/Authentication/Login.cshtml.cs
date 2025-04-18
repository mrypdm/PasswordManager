namespace PasswordManager.Web.Views.Authentication;

/// <summary>
/// Model for login view
/// </summary>
public class LoginModel(string returnUrl, bool isKeyDataExist)
{
    /// <summary>
    /// URL for redirect after login
    /// </summary>
    public string ReturnUrl { get; } = returnUrl ?? "/";

    /// <summary>
    /// If key data exists
    /// </summary>
    public bool IsKeyDataExist { get; } = isKeyDataExist;
}
