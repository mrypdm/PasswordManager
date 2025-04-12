using System;
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
    void Update(Action<TOptions> updateAction);
}
