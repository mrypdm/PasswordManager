using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Core.Checkers;

/// <summary>
/// Password checker by Entropy method
/// </summary>
public sealed class EntropyPasswordChecker(IAlphabet alphabet) : IPasswordChecker
{
    /// <inheritdoc/>
    public Task<PasswordCheckStatus> CheckAsync(string password, CancellationToken token)
    {
        if (ValidatePasswordCharacters(password))
        {
            return Task.FromResult(
                new PasswordCheckStatus(PasswordCompromisation.Unknown, PasswordStrength.Unknown, -1));
        }

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

    private bool ValidatePasswordCharacters(string password)
    {
        return password.ToHashSet().Except(alphabet.GetCharacters()).Any();
    }
}
