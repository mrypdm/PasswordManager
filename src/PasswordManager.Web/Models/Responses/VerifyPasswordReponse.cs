namespace PasswordManager.Web.Models.Responses;

/// <summary>
/// Request to verify password
/// </summary>
public class VerifyPasswordReponse
{
    /// <summary>
    /// Strength of password
    /// </summary>
    public string Strength { get; set; }

    /// <summary>
    /// Password compromisation status
    /// </summary>
    public string Compromisation { get; set; }
}
