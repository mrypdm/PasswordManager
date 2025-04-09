using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Models;
using PasswordManager.Core.Internal;

namespace PasswordManager.Core.Checkers;

/// <summary>
/// Password checker by SeaMonkey method
/// </summary>
public sealed class SeaMonkeyPasswordChecker : IPasswordChecker
{
    /// <inheritdoc/>
    public Task<PasswordCheckStatus> CheckAsync(string password, CancellationToken token)
    {
        var length = Math.Min(password.Length, 5);
        var uppers = Math.Min(CountOfUppers(password), 3);
        var numbers = Math.Min(CountOfNumbers(password), 3);
        var chars = Math.Min(CountOfCharacters(password), 3);
        var rating = 10 * length - 20 + 10 * numbers + 15 * chars + 10 * uppers;

        var strength = rating switch
        {
            < 50 => PasswordStrength.VeryLow,
            < 70 => PasswordStrength.Low,
            < 100 => PasswordStrength.Medium,
            < 130 => PasswordStrength.High,
            _ => PasswordStrength.VeryHigh
        };

        return Task.FromResult(new PasswordCheckStatus(PasswordCompromisation.Unknown, strength, rating));
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
