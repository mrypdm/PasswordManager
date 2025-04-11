using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Repositories;
using PasswordManager.Data.Contexts;
using PasswordManager.Data.Models;

namespace PasswordManager.Data.Repositories;

/// <inheritdoc />
public class KeyDataRepository(SecureDbContext context) : IKeyDataRepository
{
    /// <inheritdoc />
    public async Task SetKeyDataAsync(EncryptedData data, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Data);
        ArgumentNullException.ThrowIfNull(data.Salt);

        if (await IsKeyDataExistAsync(token))
        {
            throw new KeyDataExistsException();
        }

        var keyData = new KeyDataDbModel
        {
            Salt = data.Salt,
            Data = data.Data
        };

        await context.KeyData.AddAsync(keyData, token);
    }

    /// <inheritdoc />
    public async Task UpdateKeyDataAsync(EncryptedData data, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(data.Data);
        ArgumentNullException.ThrowIfNull(data.Salt);

        var keyData = await GetKeyDataInternalAsync(token) ?? throw new KeyDataNotExistsException();

        keyData.Salt = data.Salt;
        keyData.Data = data.Data;

        context.Update(keyData);
    }

    /// <inheritdoc />
    public async Task<bool> IsKeyDataExistAsync(CancellationToken token)
    {
        return await GetKeyDataInternalAsync(token) is not null;
    }

    /// <inheritdoc />
    public async Task DeleteKeyDataAsync(CancellationToken token)
    {
        await context.Database.EnsureDeletedAsync(token);
        await context.Database.MigrateAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedData> GetKeyDataAsync(CancellationToken token)
    {
        var data = await GetKeyDataInternalAsync(token) ?? throw new KeyDataNotExistsException();
        return new EncryptedData { Data = data.Data, Salt = data.Salt };
    }

    private Task<KeyDataDbModel> GetKeyDataInternalAsync(CancellationToken token)
    {
        return context.KeyData.SingleOrDefaultAsync(token);
    }
}
