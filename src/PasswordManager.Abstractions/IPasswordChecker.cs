using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.Abstractions;

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
