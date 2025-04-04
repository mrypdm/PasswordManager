using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Repositories;

/// <inheritdoc />
public sealed class MasterKeyDataRepository(SecureDbContext context) : IMasterKeyDataRepository
{
    /// <inheritdoc />
    public async Task SetMasterKeyDataAsync(EncryptedDataDbModel data, CancellationToken token)
    {
        if (await GetMasterKeyDataInternalAsync(token) is not null)
        {
            throw new MasterKeyDataExistsException();
        }

        await context.SecureItems.AddAsync(data, token);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedDataDbModel> GetMasterKeyDataAsync(CancellationToken token)
    {
        return await GetMasterKeyDataInternalAsync(token)
            ?? throw new MasterKeyDataNotExistsException();
    }

    /// <inheritdoc />
    public async Task DeleteMasterKeyData(CancellationToken token)
    {
        await context.SecureItems.ExecuteDeleteAsync(token);
    }

    private Task<EncryptedDataDbModel> GetMasterKeyDataInternalAsync(CancellationToken token)
    {
        return context.SecureItems.SingleOrDefaultAsync(m => m.Id == 1, token);
    }
}
