namespace PasswordManager.Abstractions.Counters;

/// <summary>
/// Counter
/// </summary>
public interface ICounter
{
    /// <summary>
    /// Current count
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Increment count
    /// </summary>
    void Increment();

    /// <summary>
    /// Clear count
    /// </summary>
    void Clear();
}
