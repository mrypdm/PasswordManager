using System.Text.RegularExpressions;

namespace PasswordManager.Core.Internal;

/// <summary>
/// Regex patterns
/// </summary>
internal partial class Patterns
{
    [GeneratedRegex("[a-z]")]
    public static partial Regex LowerLettersPattern();

    [GeneratedRegex("[A-Z]")]
    public static partial Regex UpperLettersPattern();

    [GeneratedRegex("[0-9]")]
    public static partial Regex NumbersPattern();

    [GeneratedRegex("[~!@#$%^&*+-/.,`\\\\{}\\[\\]();:?<>\"'_]")]
    public static partial Regex CharactersPattern();
}
