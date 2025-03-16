using System;
using System.Threading;
using System.Threading.Tasks;

using PasswordManager.Abstractions;
using PasswordManager.Core.Internal;

namespace PasswordManager.Core.Checkers;

/// <summary>
/// Password checker by SeaMonkey method
/// </summary>
public class SeaMonkeyPasswordChecker : IPasswordChecker
{
    /// <inheritdoc/>
    public Task<PasswordCheckStatus> CheckPasswordAsync(string password, CancellationToken token)
    {
        var length = Math.Min(password.Length, 5);
        var uppers = Math.Min(CountOfUppers(password), 3);
        var numbers = Math.Min(CountOfNumbers(password), 3);
        var chars = Math.Min(CountOfCharacters(password), 3);
        var rating = (length * 10) - 20 + (numbers * 10) + (chars * 15) + (uppers * 10);
        rating = Math.Max(Math.Min(rating, 100), 0);

        var strength = rating switch
        {
            < 20 => PasswordStrength.VeryLow,
            < 40 => PasswordStrength.Low,
            < 60 => PasswordStrength.Medium,
            < 80 => PasswordStrength.High,
            _ => PasswordStrength.VeryHigh
        };

        return Task.FromResult(new PasswordCheckStatus(PasswordCompromisation.Unknown, strength));
    }

    private static int CountOfUppers(string password)
    {
        return Patterns.UpperLettersPattern().Count(password);
    }

    private static int CountOfNumbers(string password)
    {
        return Patterns.NumbersPattern().Count(password);
    }

    private static int CountOfCharacters(string password)
    {
        return Patterns.CharactersPattern().Count(password);
    }
}
