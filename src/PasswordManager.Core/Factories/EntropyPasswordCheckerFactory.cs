using System;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Factories;

/// <summary>
/// Factory for <see cref="EntropyPasswordChecker"/>
/// </summary>
public class EntropyPasswordCheckerFactory : IPasswordCheckerFactory
{
    /// <inheritdoc />
    public IPasswordChecker Create(IAlphabet alphabet)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        return new EntropyPasswordChecker(alphabet);
    }
}
