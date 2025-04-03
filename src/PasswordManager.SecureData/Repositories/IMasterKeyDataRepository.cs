using System.Threading;
using System.Threading.Tasks;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Repositories;

/// <summary>
/// Repository for master key data
/// </summary>
public interface IMasterKeyDataRepository
{
    /// <summary>
    /// Set master key data.
    /// </summary>
    /// <exception cref="MasterKeyDataExistsException">If master key data already exist</exception>
    Task SetMasterKeyDataAsync(EncryptedDataDbModel data, CancellationToken token);

    /// <summary>
    /// Get master key data. Can be <see langword="null"/> if it does not exist
    /// </summary>
    /// <exception cref="MasterKeyDataNotExistsException">If master key data does not exist</exception>
    Task<EncryptedDataDbModel> GetMasterKeyDataAsync(CancellationToken token);

    /// <summary>
    /// Delete master key data and all items in repository
    /// </summary>
    Task DeleteMasterKeyData(CancellationToken token);
}
