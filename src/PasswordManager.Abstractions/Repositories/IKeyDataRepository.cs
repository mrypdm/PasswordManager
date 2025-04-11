using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Abstractions.Repositories;

/// <summary>
/// Repository for key data
/// </summary>
public interface IKeyDataRepository
{
    /// <summary>
    /// Set key data
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    /// <exception cref="KeyDataExistsException">If key data already exist</exception>
    Task SetKeyDataAsync(EncryptedData data, CancellationToken token);

    /// <summary>
    /// Get key data
    /// </summary>
    /// <exception cref="KeyDataNotExistsException">If key data not exists</exception>
    Task<EncryptedData> GetKeyDataAsync(CancellationToken token);

    /// <summary>
    /// Update key data
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    /// <exception cref="KeyDataNotExistsException">If key data not exists</exception>
    Task UpdateKeyDataAsync(EncryptedData data, CancellationToken token);

    /// <summary>
    /// Check if key data exists
    /// </summary>
    Task<bool> IsKeyDataExistAsync(CancellationToken token);

    /// <summary>
    /// Delete key data and all items in repository
    /// </summary>
    Task DeleteKeyDataAsync(CancellationToken token);
}
