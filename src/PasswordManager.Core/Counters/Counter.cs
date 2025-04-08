using PasswordManager.Abstractions.Counters;

namespace PasswordManager.Core.Counters;

/// <inheritdoc />
public class Counter : ICounter
{
    /// <inheritdoc />
    public int Count { get; private set; }

    /// <inheritdoc />
    public void Increment()
    {
        Count++;
    }

    /// <inheritdoc />
    public void Clear()
    {
        Count = 0;
    }
}
