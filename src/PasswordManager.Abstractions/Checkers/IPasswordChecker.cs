using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Abstractions.Checkers;

/// <summary>
/// Checker for password strength
/// </summary>
public interface IPasswordChecker
{
    /// <summary>
    /// Check strength of password
    /// </summary>
    Task<PasswordCheckStatus> CheckPasswordAsync(string password, CancellationToken token);
}
