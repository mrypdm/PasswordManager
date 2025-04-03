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
    /// Get secure item from repository by name
    /// </summary>
    Task<EncryptedDataDbModel[]> GetItemsByNameAsync(string accountName, CancellationToken token);

    /// <summary>
    /// Get secure item and decrypt it
    /// </summary>
    Task<AccountData[]> GetAccountsByNameAsync(string accountName, CancellationToken token);
}
