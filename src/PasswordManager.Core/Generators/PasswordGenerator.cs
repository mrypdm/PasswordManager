using System.Linq;
using System.Security.Cryptography;
using PasswordManager.Abstractions;
using PasswordManager.Abstractions.Generators;

namespace PasswordManager.Core.Generators;

/// <inheritdoc />
public sealed class PasswordGenerator(IAlphabet alphabet) : IPasswordGenerator
{
    /// <inheritdoc />
    public string GeneratePassword(int length)
    {
        return new(RandomNumberGenerator.GetItems<char>(alphabet.GetCharacters().ToArray(), length));
    }
}
