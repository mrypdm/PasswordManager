using System.Linq;
using System.Security.Cryptography;

using PasswordManager.Abstractions;

namespace PasswordManager.Core;

/// <inheritdoc />
public class PasswordGenerator : IPasswordGenerator
{
    /// <inheritdoc />
    public string GeneratePassword(IAlphabet alphabet, int length)
    {
        return new(RandomNumberGenerator.GetItems<char>(alphabet.GetCharacters().ToArray(), length));
    }

    /// <summary>
    /// Generates password
    /// </summary>
    public string GeneratePassword(int length,
        bool useLowerLetters = true,
        bool useUpperLetters = true,
        bool useNubmers = true,
        bool useCharacters = true,
        string allowedCharacters = Alphabet.Charecters)
    {
        var alphabet = new Alphabet();

        if (useLowerLetters)
        {
            alphabet.WithLowerLetters();
        }

        if (useUpperLetters)
        {
            alphabet.WithUpperLetters();
        }

        if (useNubmers)
        {
            alphabet.WithNumbersLetters();
        }

        if (useCharacters)
        {
            alphabet.WithCharacters(allowedCharacters);
        }

        return GeneratePassword(alphabet, length);
    }
}
