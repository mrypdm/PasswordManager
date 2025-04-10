using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;

namespace PasswordManager.Abstractions.Repositories;

/// <summary>
/// Repository for key data
/// </summary>
public interface IKeyDataRepository
{
    /// <summary>
    /// Set key data
    /// </summary>
    /// <exception cref="KeyDataExistsException">If key data already exist</exception>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    Task SetKeyDataAsync(byte[] key, CancellationToken token);

    /// <summary>
    /// Change key data and re-encrypt all items repository
    /// </summary>
    /// <exception cref="KeyDataNotExistsException">If key data not exists</exception>
    /// <exception cref="KeyValidationException">If <paramref name="newKey"/> is invalid</exception>
    Task ChangeKeyDataAsync(byte[] newKey, CancellationToken token);

    /// <summary>
    /// Validates <paramref name="key"/> with key data
    /// </summary>
    /// <exception cref="KeyDataNotExistsException">If key data not exists</exception>
    /// <exception cref="KeyValidationException">If <paramref name="key"/> is invalid</exception>
    Task ValidateKeyDataAsync(byte[] key, CancellationToken token);

    /// <summary>
    /// Check if key data exists
    /// </summary>
    Task<bool> IsKeyDataExistAsync(CancellationToken token);

    /// <summary>
    /// Delete key data and all items in repository
    /// </summary>
    Task DeleteKeyDataAsync(CancellationToken token);
}
