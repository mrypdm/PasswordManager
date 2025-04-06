using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Configurations;

/// <summary>
/// Configuration for <see cref="MasterKeyDataDbModel"/>
/// </summary>
public sealed class MasterKeyDataDbModelConfiguration : IEntityTypeConfiguration<MasterKeyDataDbModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MasterKeyDataDbModel> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();
        builder.Property(m => m.Salt).IsRequired();
        builder.Property(m => m.Data).IsRequired();
        builder.Property(m => m.Version).HasDefaultValue(0).IsRowVersion();
    }
}
