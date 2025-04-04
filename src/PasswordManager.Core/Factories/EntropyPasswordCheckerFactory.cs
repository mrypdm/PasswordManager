using PasswordManager.Abstractions;
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
        return new EntropyPasswordChecker(alphabet);
    }
}
