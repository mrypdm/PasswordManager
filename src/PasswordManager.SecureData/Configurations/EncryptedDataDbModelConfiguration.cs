using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Configurations;

/// <summary>
/// Configuration for <see cref="EncryptedDataDbModel"/>
/// </summary>
public sealed class EncryptedDataDbModelConfiguration : IEntityTypeConfiguration<EncryptedDataDbModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EncryptedDataDbModel> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired();
        builder.Property(m => m.Salt).IsRequired();
        builder.Property(m => m.Data).IsRequired();
        builder.Property(m => m.Version).HasDefaultValue(0).IsRowVersion();
        builder.HasIndex(m => m.Name);
    }
}
