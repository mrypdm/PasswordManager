namespace PasswordManager.Web.Models;

/// <summary>
/// Request to verify password
/// </summary>
public class PasswordVerifyRequest
{
    /// <summary>
    /// Password for verify
    /// </summary>
    public string Password { get; set; }
}
