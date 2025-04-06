using System.Diagnostics.CodeAnalysis;

namespace PasswordManager.Web.Models.Requests;

/// <summary>
/// Request for API
/// </summary>
public interface IRequest
{
    /// <summary>
    /// Validates request and return error message
    /// </summary>
    bool Validate([NotNullWhen(false)] out string errorMessage);
}
