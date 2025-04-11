using System.Threading;
using System.Threading.Tasks;
using PasswordManager.Abstractions.Contexts;

namespace PasswordManager.Data.Contexts;

/// <inheritdoc />
public class DataContext(SecureDbContext dbContext) : IDataContext
{
    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken token)
    {
        await dbContext.SaveChangesAsync(token);
    }
}
