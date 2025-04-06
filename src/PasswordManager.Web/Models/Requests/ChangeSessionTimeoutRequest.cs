using System;
using System.Diagnostics.CodeAnalysis;

namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request for change session timeout
/// </summary>
public class ChangeSessionTimeoutRequest : IRequest
{
    /// <summary>
    /// New value for timeout
    /// </summary>
    public TimeSpan Timeout { get; set; }

    /// <inheritdoc />
    public bool Validate([NotNullWhen(false)] out string errorMessage)
    {
        errorMessage = null;
        if (Timeout <= TimeSpan.Zero)
        {
            errorMessage = "Timeout cannot be zero or negative";
        }

        return errorMessage is null;
    }
}
