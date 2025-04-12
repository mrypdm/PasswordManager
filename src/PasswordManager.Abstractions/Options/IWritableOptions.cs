using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PasswordManager.Abstractions.Options;

/// <summary>
/// Writable options
/// </summary>
/// <typeparam name="TOptions">Type of options</typeparam>
public interface IWritableOptions<TOptions> : IOptions<TOptions>
    where TOptions : class
{
    /// <summary>
    /// Update options
    /// </summary>
    Task UpdateAsync(Action<TOptions> updateAction, CancellationToken token);
}
