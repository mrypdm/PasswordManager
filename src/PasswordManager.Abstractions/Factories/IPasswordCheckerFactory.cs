using PasswordManager.Abstractions.Checkers;

namespace PasswordManager.Abstractions.Factories;

/// <summary>
/// Factory for <see cref="IPasswordChecker"/>
/// </summary>
public interface IPasswordCheckerFactory
{
    /// <summary>
    /// Create password checker
    /// </summary>
    IPasswordChecker Create(IAlphabet alphabet);
}
