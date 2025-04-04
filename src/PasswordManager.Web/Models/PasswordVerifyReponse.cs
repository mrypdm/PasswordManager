namespace PasswordManager.Web.Models;

/// <summary>
/// Request to verify password
/// </summary>
public class PasswordVerifyReponse
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
