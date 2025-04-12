using System.Collections.Generic;
using System.Linq;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Checkers;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Core.Checkers;

namespace PasswordManager.Core.Factories;

/// <summary>
/// Factory for <see cref="CombinedPasswordChecker"/>
/// </summary>
public class CombinedPasswordCheckerFactory(IEnumerable<IPasswordCheckerFactory> childFactories)
    : IPasswordCheckerFactory
{
    /// <inheritdoc />
    public IPasswordChecker Create(IAlphabet alphabet)
    {
        return new CombinedPasswordChecker(childFactories.Select(m => m.Create(alphabet)));
    }
}
