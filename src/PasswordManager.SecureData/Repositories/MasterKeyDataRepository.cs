using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.SecureData.Contexts;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Repositories;

/// <inheritdoc />
public class MasterKeyDataRepository(SecureDbContext context) : IMasterKeyDataRepository
{
    /// <inheritdoc />
    public async Task SetMasterKeyDataAsync(EncryptedDataDbModel data, CancellationToken token)
    {
        if (await GetMasterKeyDataAsync(token) is not null)
        {
            throw new MasterKeyDataExistsException();
        }

        await context.SecureItems.AddAsync(data, token);
        await context.SaveChangesAsync(token);
    }

    /// <inheritdoc />
    public async Task<EncryptedDataDbModel> GetMasterKeyDataAsync(CancellationToken token)
    {
        return await context.SecureItems.SingleOrDefaultAsync(m => m.Id == 1, token)
            ?? throw new MasterKeyDataNotExistsException();
    }
}
