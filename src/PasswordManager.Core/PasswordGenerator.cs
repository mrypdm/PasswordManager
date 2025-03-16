using System.Linq;
using System.Security.Cryptography;

using PasswordManager.Abstractions;

namespace PasswordManager.Core;

/// <inheritdoc />
public class PasswordGenerator(IAlphabet alphabet) : IPasswordGenerator
{
    /// <inheritdoc />
    public string GeneratePassword(int length)
    {
        return new(RandomNumberGenerator.GetItems<char>(alphabet.GetCharacters().ToArray(), length));
    }
}
