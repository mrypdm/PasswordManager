using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Contexts;

/// <summary>
/// DB context for secure items
/// </summary>
public sealed class SecureDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// Secure items
    /// </summary>
    public DbSet<SecureItemDbModel> SecureItems { get; set; }

    /// <summary>
    /// Master key data
    /// </summary>
    public DbSet<MasterKeyDataDbModel> MasterKeyData { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SecureDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Runtime design factory for migrations creation
    /// </summary>
    public class SecureDbContextFactory : IDesignTimeDbContextFactory<SecureDbContext>
    {
        public SecureDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite("Filename=fake-secure-db.db")
                .Options;

            return new SecureDbContext(options);
        }
    }
}
