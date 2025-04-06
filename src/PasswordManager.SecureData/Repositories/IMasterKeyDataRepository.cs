using System.Threading;
using System.Threading.Tasks;
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
    Task SetMasterKeyDataAsync(byte[] masterKey, CancellationToken token);

    /// <summary>
    /// Validates <paramref name="masterKey"/> with master key data
    /// </summary>
    Task ValidateMasterKeyDataAsync(byte[] masterKey, CancellationToken token);

    /// <summary>
    /// Delete master key data and all items in repository
    /// </summary>
    Task DeleteMasterKeyData(CancellationToken token);

    /// <summary>
    /// Changes master key data and re-encrypt all items repository
    /// </summary>
    Task ChangeMasterKeyDataAsync(byte[] newMasterPassword, CancellationToken token);
}
