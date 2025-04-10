using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Abstractions.Repositories;

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
    /// <exception cref="ItemNotExistsException">If item with <paramref name="id"/> not exists</exception>
    Task UpdateAccountAsync(int id, AccountData data, CancellationToken token);

    /// <summary>
    /// Deletes account by <paramref name="id"/>
    /// </summary>
    Task DeleteAccountAsync(int id, CancellationToken token);

    /// <summary>
    /// Get account data from repository by id
    /// </summary>
    /// <exception cref="ItemNotExistsException">If item with <paramref name="id"/> not exists</exception>
    Task<AccountData> GetAccountByIdAsync(int id, CancellationToken token);

    /// <summary>
    /// Get items headers from repository
    /// </summary>
    Task<ItemHeader[]> GetItemHeadersAsync(CancellationToken token);
}
