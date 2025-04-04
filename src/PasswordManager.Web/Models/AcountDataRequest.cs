namespace PasswordManager.Web.Models;

/// <summary>
/// Request for add/update account data
/// </summary>
public class AcountDataRequest
{
    /// <summary>
    /// Id of account
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of account
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Login for account
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// Password for account
    /// </summary>
    public string Password { get; set; }
}
