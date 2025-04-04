using System.Threading;
using System.Threading.Tasks;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Repositories;

/// <summary>
/// Repository for <see cref="EncryptedDataDbModel"/>
/// </summary>
public interface ISecureItemsRepository
{
    /// <summary>
    /// Add new account data to repository and return its ID
    /// </summary>
    Task<int> AddAccountAsync(AccountData data, CancellationToken token);

    /// <summary>
    /// Delete secure item from repository
    /// </summary>
    Task DeleteItemAsync(EncryptedDataDbModel item, CancellationToken token);

    /// <summary>
    /// Get item from repository by id
    /// </summary>
    Task<EncryptedDataDbModel> GetItemByIdAsync(int id, CancellationToken token);

    /// <summary>
    /// Get account data from repository by id
    /// </summary>
    Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token);
}
