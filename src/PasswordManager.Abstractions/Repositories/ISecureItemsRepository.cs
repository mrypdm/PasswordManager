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
    /// Add <paramref name="data"/> with <paramref name="name"/> to repository and return its ID
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is whitespace</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    Task<int> AddDataAsync(string name, EncryptedData data, CancellationToken token);

    /// <summary>
    /// Update data with <paramref name="id"/> with
    /// new <paramref name="data"/> and new <paramref name="name"/>
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is whitespace</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    /// <exception cref="ItemNotExistsException">If item with <paramref name="id"/> not exists</exception>
    Task UpdateDataAsync(int id, string name, EncryptedData data, CancellationToken token);

    /// <summary>
    /// Delete data by <paramref name="id"/>
    /// </summary>
    Task DeleteDataAsync(int id, CancellationToken token);

    /// <summary>
    /// Get data by <paramref name="id"/>
    /// </summary>
    /// <exception cref="ItemNotExistsException">If item with <paramref name="id"/> not exists</exception>
    Task<EncryptedData> GetDataByIdAsync(int id, CancellationToken token);

    /// <summary>
    /// Get all items
    /// </summary>
    Task<IItem[]> GetItemsAsync(CancellationToken token);

    /// <summary>
    /// Get all data
    /// </summary>
    Task<EncryptedItem[]> GetDataAsync(CancellationToken token);
}
