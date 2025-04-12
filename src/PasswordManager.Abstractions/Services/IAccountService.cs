using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Abstractions.Services;

/// <summary>
/// Service for Accounts
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Add new account
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="account"/> is null</exception>
    Task<Account> AddAccountAsync(Account account, CancellationToken token);

    /// <summary>
    /// Update account
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="account"/> is null</exception>
    /// <exception cref="AccountNotExistsException">If account with <paramref name="id"/> not exists</exception>
    Task UpdateAccountAsync(Account account, CancellationToken token);

    /// <summary>
    /// Delete account by <paramref name="id"/>
    /// </summary>
    Task DeleteAccountAsync(int id, CancellationToken token);

    /// <summary>
    /// Get accoun by id
    /// </summary>
    /// <exception cref="AccountNotExistsException">If account with <paramref name="id"/> not exists</exception>
    Task<Account> GetAccountByIdAsync(int id, CancellationToken token);

    /// <summary>
    /// Get accounts without data
    /// </summary>
    Task<Account[]> GetAccountsWithoutDataAsync(CancellationToken token);
}
