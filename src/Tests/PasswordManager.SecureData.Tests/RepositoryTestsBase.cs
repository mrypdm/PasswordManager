using Microsoft.EntityFrameworkCore;
using PasswordManager.SecureData.Contexts;

namespace PasswordManager.SecureData.Tests;

/// <summary>
/// Tests for repositories
/// </summary>
public abstract class RepositoryTestsBase
{
    [SetUp]
    public virtual void SetUpDatabase()
    {
        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.Migrate();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureDeleted();
    }

    protected static SecureDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlite("Filename=fake-secure-db.db")
            .LogTo(TestContext.WriteLine)
            .EnableSensitiveDataLogging()
            .Options;
        return new SecureDbContext(options);
    }
}
