using System;
using System.Linq;
using System.Security.Cryptography;
using PasswordManager.Abstractions.Alphabets;
using PasswordManager.Abstractions.Generators;

namespace PasswordManager.Core.Generators;

/// <inheritdoc />
public sealed class PasswordGenerator : IPasswordGenerator
{
    private readonly IAlphabet _alphabet;

    public PasswordGenerator(IAlphabet alphabet)
    {
        if (alphabet.GetCharacters().Count == 0)
        {
            throw new ArgumentException("Alphabet is empty");
        }

        _alphabet = alphabet;
    }

    /// <inheritdoc />
    public string Generate(int length)
    {
        return new(RandomNumberGenerator.GetItems<char>(_alphabet.GetCharacters().ToArray(), length));
    }
}
