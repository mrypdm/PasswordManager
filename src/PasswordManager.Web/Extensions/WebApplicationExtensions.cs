using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PasswordManager.Data.Contexts;

namespace PasswordManager.Web.Extensions;

/// <summary>
/// Extensions for <see cref="WebApplication"/>
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Migrate <see cref="SecureDbContext"/>
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app, CancellationToken token)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetService<SecureDbContext>();
        await context.Database.MigrateAsync(token);
    }
}
