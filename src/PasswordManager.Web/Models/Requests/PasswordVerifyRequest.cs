using System.Diagnostics.CodeAnalysis;

namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request to verify password
/// </summary>
public class PasswordVerifyRequest : IRequest
{
    /// <summary>
    /// Password for verify
    /// </summary>
    public string Password { get; set; }

    /// <inheritdoc />
    public bool Validate([NotNullWhen(false)] out string errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(Password))
        {
            errorMessage = "Password cannot be empty";
        }

        return errorMessage is null;
    }
}
