namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request for login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Master password
    /// </summary>
    public string MasterPassword { get; set; }
}
