using System;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.UserSettings;

/// <summary>
/// Writable options
/// </summary>
/// <typeparam name="TOptions">Type of options</typeparam>
public interface IWritableOptions<TOptions> where TOptions : class, new()
{
    /// <summary>
    /// Value of options
    /// </summary>
    TOptions Value { get; }

    /// <summary>
    /// Update options
    /// </summary>
    Task UpdateAsync(Action<TOptions> updateAction, CancellationToken token);
}
