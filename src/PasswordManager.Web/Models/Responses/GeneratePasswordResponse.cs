namespace PasswordManager.Web.Models.Responses;

/// <summary>
/// Response for generating password
/// </summary>
public class GeneratePasswordResponse
{
    /// <summary>
    /// Generated password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Check status of password
    /// </summary>
    public VerifyPasswordReponse CheckStatus { get; set; }
}
