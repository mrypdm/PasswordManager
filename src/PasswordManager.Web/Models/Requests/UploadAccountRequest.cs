using System.Diagnostics.CodeAnalysis;

namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request for add/update account
/// </summary>
public class UploadAccountRequest : IRequest
{
    /// <summary>
    /// Name of account
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// User login
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// User password
    /// </summary>
    public string Password { get; set; }

    /// <inheritdoc />
    public bool Validate([NotNullWhen(false)] out string errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(Name))
        {
            errorMessage = "Name cannot be empty";
        }
        if (string.IsNullOrWhiteSpace(Login))
        {
            errorMessage = "Login cannot be empty";
        }
        if (string.IsNullOrWhiteSpace(Password))
        {
            errorMessage = "Password cannot be empty";
        }

        return errorMessage is null;
    }
}
