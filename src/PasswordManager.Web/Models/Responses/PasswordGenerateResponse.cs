namespace PasswordManager.Web.Models.Responses;

/// <summary>
/// Response for generating password
/// </summary>
public class PasswordGenerateResponse
{
    /// <summary>
    /// Generated password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Check status of password
    /// </summary>
    public PasswordVerifyReponse CheckStatus { get; set; }
}
