using System.Threading;
using System.Threading.Tasks;

using PasswordManager.Abstractions;

namespace PasswordManager.External.Checkers;

/// <summary>
/// Password checker by Zxcvbn method
/// </summary>
public class ZxcvbnPasswordChecker : IPasswordChecker
{
    /// <inheritdoc/>
    public Task<PasswordCheckStatus> CheckPasswordAsync(string password, CancellationToken token)
    {
        var strength = (PasswordStrength)Zxcvbn.Core.EvaluatePassword(password).Score;
        return Task.FromResult(new PasswordCheckStatus(PasswordCompromisation.Unknown, strength));
    }
}
