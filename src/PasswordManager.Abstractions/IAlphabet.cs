using System.Collections.Generic;

namespace PasswordManager.Abstractions;

/// <summary>
/// Alphabet
/// </summary>
public interface IAlphabet
{
    /// <summary>
    /// Get characters of alphabet
    /// </summary>
    HashSet<char> GetCharacters();
}
