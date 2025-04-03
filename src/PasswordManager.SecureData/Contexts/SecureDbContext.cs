using System;
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
    public DbSet<EncryptedDataDbModel> SecureItems { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SecureDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Runtime design factory for EF Core. Using for migrations creation and testing
    /// </summary>
    public class SecureDbContextFactory : IDesignTimeDbContextFactory<SecureDbContext>
    {
        public SecureDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite("Filename=FakeFile.db")
                .LogTo(Console.WriteLine)
                .EnableSensitiveDataLogging()
                .Options;

            return new SecureDbContext(options);
        }
    }
}
