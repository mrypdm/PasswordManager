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
    /// Add new account data to repository
    /// </summary>
    Task AddAccountAsync(AccountData data, CancellationToken token);

    /// <summary>
    /// Delete secure item from repository
    /// </summary>
    Task DeleteItemAsync(EncryptedDataDbModel item, CancellationToken token);

    /// <summary>
    /// Update secure item from <paramref name="oldValue"/> to <paramref name="newValue"/>
    /// </summary>
    Task UpdateAccountAsync(AccountData oldValue, AccountData newValue, CancellationToken token);

    /// <summary>
    /// Get secure item from repository by name
    /// </summary>
    Task<EncryptedDataDbModel> GetItemByNameAsync(string accountName, CancellationToken token);

    /// <summary>
    /// Get secure item and decrypt it
    /// </summary>
    Task<AccountData> GetAccountByNameAsync(string accountName, CancellationToken token);
}
