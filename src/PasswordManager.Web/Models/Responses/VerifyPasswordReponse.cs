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
    /// Is password compomised
    /// </summary>
    public bool IsCompomised { get; set; }
}
