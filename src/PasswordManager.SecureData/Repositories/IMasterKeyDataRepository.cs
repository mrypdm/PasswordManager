using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.SecureData.Exceptions;

namespace PasswordManager.SecureData.Repositories;

/// <summary>
/// Repository for master key data
/// </summary>
public interface IMasterKeyDataRepository
{
    /// <summary>
    /// Set master key data
    /// </summary>
    /// <exception cref="MasterKeyDataExistsException">If master key data already exist</exception>
    /// <exception cref="KeyValidationException">If <paramref name="masterKey"/> is invalid</exception>
    Task SetMasterKeyDataAsync(byte[] masterKey, CancellationToken token);

    /// <summary>
    /// Changes master key data and re-encrypt all items repository
    /// </summary>
    /// <exception cref="MasterKeyDataNotExistsException">If master key data not exists</exception>
    /// <exception cref="KeyValidationException">If <paramref name="newMasterKey"/> is invalid</exception>
    Task ChangeMasterKeyDataAsync(byte[] newMasterKey, CancellationToken token);

    /// <summary>
    /// Validates <paramref name="masterKey"/> with master key data
    /// </summary>
    /// <exception cref="MasterKeyDataNotExistsException">If master key data not exists</exception>
    /// <exception cref="KeyValidationException">If <paramref name="masterKey"/> is invalid</exception>
    /// <exception cref="InvalidMasterKeyException">If master key is invalid</exception>
    Task ValidateMasterKeyDataAsync(byte[] masterKey, CancellationToken token);

    /// <summary>
    /// Check if master key data exists
    /// </summary>
    Task<bool> IsMasterKeyDataExistsAsync(CancellationToken token);

    /// <summary>
    /// Delete master key data and all items in repository
    /// </summary>
    Task DeleteMasterKeyDataAsync(CancellationToken token);
}
