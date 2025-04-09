using System.Diagnostics.CodeAnalysis;

namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request for login
/// </summary>
public class LoginRequest : IRequest
{
    /// <summary>
    /// Master password
    /// </summary>
    public string MasterPassword { get; set; }

    /// <inheritdoc />
    public bool Validate([NotNullWhen(false)] out string errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(MasterPassword))
        {
            errorMessage = "Master password cannot be empty";
        }

        return errorMessage is null;
    }
}
