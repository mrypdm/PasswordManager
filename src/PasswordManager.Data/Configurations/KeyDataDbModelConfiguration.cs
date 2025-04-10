using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.Data.Models;

namespace PasswordManager.Data.Configurations;

/// <summary>
/// Configuration for <see cref="KeyDataDbModel"/>
/// </summary>
public sealed class KeyDataDbModelConfiguration : IEntityTypeConfiguration<KeyDataDbModel>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<KeyDataDbModel> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedOnAdd();
        builder.Property(m => m.Salt).IsRequired();
        builder.Property(m => m.Data).IsRequired();
        builder.Property(m => m.Version).HasDefaultValue(0).IsRowVersion();
    }
}
