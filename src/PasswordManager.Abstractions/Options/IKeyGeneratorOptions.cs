namespace PasswordManager.Abstractions.Options;

/// <summary>
/// Options for <see cref="IKeyGenerator"/>
/// </summary>
public interface IKeyGeneratorOptions
{
    /// <summary>
    /// Salt for generation
    /// </summary>
    byte[] Salt { get; }

    /// <summary>
    /// Count of iterations
    /// </summary>
    int Iterations { get; }
}
