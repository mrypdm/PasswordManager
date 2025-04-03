using PasswordManager.Abstractions;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Core.Generators;

namespace PasswordManager.Core.Factories;

/// <summary>
/// Factory for <see cref="PasswordGenerator"/>
/// </summary>
public class PasswordGeneratorFactory : IPasswordGeneratorFactory
{
    /// <inheritdoc />
    public IPasswordGenerator Create(IAlphabet alphabet)
    {
        return new PasswordGenerator(alphabet);
    }
}
