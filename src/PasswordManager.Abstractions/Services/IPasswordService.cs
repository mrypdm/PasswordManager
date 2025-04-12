using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Abstractions.Services;

/// <summary>
/// Service for password
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Generate password
    string GeneratePassword(int length, IAlphabet alphabet);

    /// <summary>
    /// Check password strength and compomisation
    /// </summary>
    Task<PasswordCheckStatus> CheckPasswordAsync(string password, IAlphabet alphabet, CancellationToken token);
}
