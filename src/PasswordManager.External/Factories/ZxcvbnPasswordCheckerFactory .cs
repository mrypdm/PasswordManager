using PasswordManager.Abstractions;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.External.Checkers;

namespace PasswordManager.External.Factories;

/// <summary>
/// Factory for <see cref="SeaMonkeyPasswordChecker"/>
/// </summary>
public class ZxcvbnPasswordCheckerFactory : IPasswordCheckerFactory
{
    /// <inheritdoc />
    public IPasswordChecker Create(IAlphabet alphabet)
    {
        return new ZxcvbnPasswordChecker();
    }
}
