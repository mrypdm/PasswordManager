using System;
using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;

namespace PasswordManager.Abstractions.Repositories;

/// <summary>
/// Repository for <see cref="EncryptedItem"/>
/// </summary>
public interface IEncryptedItemsRepository
{
    /// <summary>
    /// Add item to repository
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is whitespace</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="name"/> is null</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
    Task<IItem> AddItemAsync(EncryptedItem item, CancellationToken token);

    /// <summary>
    /// Update item
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="item"/>.Name is whitespace</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="item"/> is null</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="item"/>.Data is null</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="item"/>.Salt is null</exception>
    /// <exception cref="ItemNotExistsException">If item with <paramref name="item"/>.Id not exists</exception>
    Task UpdateItemAsync(EncryptedItem item, CancellationToken token);

    /// <summary>
    /// Delete item by <paramref name="id"/>
    /// </summary>
    Task DeleteItemAsync(int id, CancellationToken token);

    /// <summary>
    /// Get data by <paramref name="id"/>
    /// </summary>
    /// <exception cref="ItemNotExistsException">If item with <paramref name="id"/> not exists</exception>
    Task<EncryptedItem> GetItemByIdAsync(int id, CancellationToken token);

    /// <summary>
    /// Get all data
    /// </summary>
    Task<EncryptedItem[]> GetItemsAsync(CancellationToken token);
}
