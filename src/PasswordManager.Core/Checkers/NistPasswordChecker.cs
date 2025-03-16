using System;
using System.Threading;
using System.Threading.Tasks;

using PasswordManager.Abstractions;
using PasswordManager.Core.Internal;

namespace PasswordManager.Core.Checkers;

/// <summary>
/// Password checker by NIST method
/// </summary>
public class NistPasswordChecker : IPasswordChecker
{
    /// <inheritdoc/>
    public Task<PasswordCheckStatus> CheckPasswordAsync(string password, CancellationToken token)
    {
        if (password.Length == 0)
        {
            return Task.FromResult(new PasswordCheckStatus(PasswordCompromisation.Unknown, PasswordStrength.Low));
        }

        var entropy = 4.0;
        var oneBitSymbols = Math.Max(password.Length - 20, 0);
        var oneAndHalfBitSymbols = Math.Max(password.Length - oneBitSymbols - 8, 0);
        var twoBitSymbols = Math.Max(password.Length - oneBitSymbols - oneAndHalfBitSymbols - 1, 0);

        entropy += (1 * oneBitSymbols) + (1.5 * oneAndHalfBitSymbols) + (2 * twoBitSymbols);

        if (HasLowerLetters(password) && HasUpperLetters(password) && (HasNumbers(password) || HasCharacters(password)))
        {
            entropy += 6;
        }

        var strength = entropy switch
        {
            < 16 => PasswordStrength.VeryLow,
            < 32 => PasswordStrength.Low,
            < 48 => PasswordStrength.Medium,
            < 64 => PasswordStrength.High,
            _ => PasswordStrength.VeryHigh,
        };

        return Task.FromResult(new PasswordCheckStatus(PasswordCompromisation.Unknown, strength));
    }

    private static bool HasLowerLetters(string password)
    {
        return Patterns.LowerLettersPattern().IsMatch(password);
    }

    private static bool HasUpperLetters(string password)
    {
        return Patterns.UpperLettersPattern().IsMatch(password);
    }

    private static bool HasNumbers(string password)
    {
        return Patterns.NumbersPattern().IsMatch(password);
    }

    private static bool HasCharacters(string password)
    {
        return Patterns.CharactersPattern().IsMatch(password);
    }
}
