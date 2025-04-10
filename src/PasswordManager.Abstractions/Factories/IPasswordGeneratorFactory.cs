using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Generators;

namespace PasswordManager.Abstractions.Factories;

/// <summary>
/// Factory for <see cref="IPasswordGenerator"/>
/// </summary>
public interface IPasswordGeneratorFactory
{
    /// <summary>
    /// Create password generator
    /// </summary>
    IPasswordGenerator Create(IAlphabet alphabet);
}
