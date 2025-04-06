using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.SecureData.Models;

namespace PasswordManager.SecureData.Configurations;

/// <summary>
/// Configuration for <see cref="SecureItemDbModel"/>
/// </summary>
public class SecureItemDbModelConfiguration : IEntityTypeConfiguration<SecureItemDbModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SecureItemDbModel> builder)
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
