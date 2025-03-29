using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.External.Checkers;

/// <summary>
/// Password checker by Zxcvbn method
/// </summary>
public class ZxcvbnPasswordChecker : IPasswordChecker
{
    /// <inheritdoc/>
    public Task<PasswordCheckStatus> CheckPasswordAsync(string password, CancellationToken token)
    {
        var strength = Zxcvbn.Core.EvaluatePassword(password).Score;
        return Task.FromResult(new PasswordCheckStatus(PasswordCompromisation.Unknown, (PasswordStrength)strength, strength));
    }
}
