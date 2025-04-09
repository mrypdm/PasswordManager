using PasswordManager.Abstractions;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Factories;

/// <summary>
/// Factory for <see cref="SeaMonkeyPasswordChecker"/>
/// </summary>
public class SeaMonkeyPasswordCheckerFactory : IPasswordCheckerFactory
{
    /// <inheritdoc />
    public IPasswordChecker Create(IAlphabet alphabet)
    {
        return new SeaMonkeyPasswordChecker();
    }
}
