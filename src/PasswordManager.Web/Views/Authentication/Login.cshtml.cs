namespace PasswordManager.Web.Views.Authentication;

/// <summary>
/// Model for login view
/// </summary>
public class LoginModel(string returnUrl, bool isMasterKeyDataExist)
{
    /// <summary>
    /// URL for redirect after login
    /// </summary>
    public string ReturnUrl { get; } = returnUrl ?? "/";

    /// <summary>
    /// If master key data exists
    /// </summary>
    public bool IsMasterKeyDataExist { get; } = isMasterKeyDataExist;
}
