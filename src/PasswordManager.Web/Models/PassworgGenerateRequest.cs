namespace PasswordManager.Web.Models;

/// <summary>
/// Request for generating password
/// </summary>
public class PassworgGenerateRequest
{
    /// <summary>
    /// Length of generating password
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Use lower letters for genearating password
    /// </summary>
    public bool UseLowerLetters { get; set; }

    /// <summary>
    /// Use upper letters for genearating password
    /// </summary>
    public bool UseUpperLetters { get; set; }

    /// <summary>
    /// Use numbers for genearating password
    /// </summary>
    public bool UseNumbers { get; set; }

    /// <summary>
    /// Allowed specific characters for generating password
    /// </summary>
    public string SpecialSymbols { get; set; } = "";
}
