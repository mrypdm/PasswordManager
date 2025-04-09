using PasswordManager.Abstractions;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.External.Checkers;

namespace PasswordManager.External.Factories;

/// <summary>
/// Factory for <see cref="PwnedPasswordChecker"/>
/// </summary>
public class PwnedPasswordCheckerFactory : IPasswordCheckerFactory
{
    /// <inheritdoc />
    public IPasswordChecker Create(IAlphabet alphabet)
    {
        return new PwnedPasswordChecker();
    }
}
