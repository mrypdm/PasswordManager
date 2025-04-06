using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordManager.SecureData.Extensions;

/// <summary>
/// Extensions for <see cref="MigrationBuilder"/>
/// </summary>
public static class MigrationBuilderExtensions
{
    /// <summary>
    /// Add trigger for updating <paramref name="column"/> in <paramref name="table"/> at each row update
    /// </summary>
    /// <remarks>
    /// <paramref name="column"/> must be <see langword="int"/> or other number format
    /// </remarks>
    public static void AddVersionTrigger(this MigrationBuilder builder, string table, string column)
    {
        builder.Sql(
            $$"""
            CREATE TRIGGER Set_{{table}}_{{column}}_OnUpdate
            AFTER UPDATE ON {{table}}
            BEGIN
                UPDATE {{table}}
                SET {{column}} = {{column}} + 1
                WHERE rowid = NEW.rowid;
            END
            """);
    }
}
