using System.Collections.Generic;
using System.Linq;
using System.Text;
using PasswordManager.Abstractions.Alphabets;

namespace PasswordManager.Core.Alphabets;

/// <inheritdoc />
public sealed class Alphabet : IAlphabet
{
    /// <summary>
    /// Empty alphabet
    /// </summary>
    public static IAlphabet Empty { get; } = new Alphabet();

    private const string LowerLetters = "qwertyuiopasdfghjklzxcvbnm";

    private const string UpperLetters = "QWERTYUIOPASDFGHJKLZXCVBNM";

    private const string Numbers = "0123456789";

    private const string Characters = "`~!@#$%^&*+-/.,\\{}[]();:?<>\"'_";

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
    public Alphabet WithNumbers()
    {
        _alphabetBuilder.Append(Numbers);
        return this;
    }

    /// <summary>
    /// Append intersection of <paramref name="allowedCharacters"/> and <see cref="Characters"/> to alphabet
    /// </summary>
    public Alphabet WithCharacters(string allowedCharacters = Characters)
    {
        _alphabetBuilder.Append([.. allowedCharacters.Intersect(Characters)]);
        return this;
    }
}
