using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PasswordManager.Abstractions;

namespace PasswordManager.Core.Checkers;

/// <summary>
/// Password checker by Entropy method
/// </summary>
public class EntropyPasswordChecker(IAlphabet alphabet) : IPasswordChecker
{
    /// <inheritdoc/>
    public Task<PasswordCheckStatus> CheckPasswordAsync(string password, CancellationToken token)
    {
        ValidatePasswordCharacters(password);

        var entropy = Math.Log2(alphabet.GetCharacters().Count) * password.Length;
        var strength = entropy switch
        {
            < 60 => PasswordStrength.VeryLow,
            < 80 => PasswordStrength.Low,
            < 100 => PasswordStrength.Medium,
            < 120 => PasswordStrength.High,
            _ => PasswordStrength.VeryHigh,
        };

        return Task.FromResult(new PasswordCheckStatus(PasswordCompromisation.Unknown, strength, entropy));
    }

    private void ValidatePasswordCharacters(string password)
    {
        if (password.ToHashSet().Except(alphabet.GetCharacters()).Any())
        {
            throw new InvalidOperationException("Password contains characters, which does not exist in alphabet");
        }
    }
}
