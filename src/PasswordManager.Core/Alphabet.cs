using System.Collections.Generic;
using System.Linq;
using System.Text;

using PasswordManager.Abstractions;

namespace PasswordManager.Core;

/// <inheritdoc />
public class Alphabet : IAlphabet
{
    public const string LowerLetters = "qwertyuiopasdfghjklzxcvbnm";

    public const string UpperLetters = "QWERTYUIOPASDFGHJKLZXCVBNM";

    public const string Numbers = "0123456789";

    public const string Charecters = "`~!@#$%^&*+-/.,\\{}[]();:?<>\"'_";

    private readonly StringBuilder _alphabetBuilder = new();

    /// <inheritdoc />
    public HashSet<char> GetCharacters()
    {
        return [.. _alphabetBuilder.ToString().ToCharArray()];
    }

    /// <summary>
    /// Append <see cref="LowerLetters"/> to alphabet
    /// </summary>
    public Alphabet WithLowerLetters()
    {
        _alphabetBuilder.Append(LowerLetters);
        return this;
    }

    /// <summary>
    /// Append <see cref="UpperLetters"/> to alphabet
    /// </summary>
    public Alphabet WithUpperLetters()
    {
        _alphabetBuilder.Append(UpperLetters);
        return this;
    }

    /// <summary>
    /// Append <see cref="Numbers"/> to alphabet
    /// </summary>
    public Alphabet WithNumbersLetters()
    {
        _alphabetBuilder.Append(Numbers);
        return this;
    }

    /// <summary>
    /// Append intersection of <paramref name="allowedCharacters"/> and <see cref="Charecters"/> to alphabet
    /// </summary>
    public Alphabet WithCharacters(string allowedCharacters = Charecters)
    {
        _alphabetBuilder.Append([.. allowedCharacters.Intersect(Charecters)]);
        return this;
    }
}
