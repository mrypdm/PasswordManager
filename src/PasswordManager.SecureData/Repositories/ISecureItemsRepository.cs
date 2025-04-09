using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Repositories;

/// <summary>
/// Repository for <see cref="SecureItemDbModel"/>
/// </summary>
public interface ISecureItemsRepository
{
    /// <summary>
    /// Add new account data to repository and return its ID
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    Task<int> AddAccountAsync(AccountData data, CancellationToken token);

    /// <summary>
    /// Updates item with <paramref name="id"/> with new <paramref name="data"/>
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    Task UpdateAccountAsync(int id, AccountData data, CancellationToken token);

    /// <summary>
    /// Deletes account by <paramref name="id"/>
    /// </summary>
    Task DeleteAccountAsync(int id, CancellationToken token);

    /// <summary>
    /// Get account data from repository by id
    /// </summary>
    Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token);

    /// <summary>
    /// Get all items in repository
    /// </summary>
    Task<SecureItemDbModel[]> GetItemsAsync(CancellationToken token);
}
