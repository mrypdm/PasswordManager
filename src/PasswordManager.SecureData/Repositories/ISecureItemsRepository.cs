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
    /// Updates item with <paramref name="id"/> with new <paramref name="data"/>
    /// </summary>
    Task UpdateAccountAsync(int id, AccountData data, CancellationToken token);

    /// <summary>
    /// Get account data from repository by id
    /// </summary>
    Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token);
}
