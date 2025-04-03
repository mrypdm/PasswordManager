using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordManager.SecureData.Extensions;

/// <summary>
/// Extensions for <see cref="MigrationBuilder"/>
/// </summary>
public static class MigrationBuilderExtensions
{
    /// <summary>
    /// Add trigger for updating <paramref name="columnName"/> in <paramref name="tableName"/> at each row update
    /// </summary>
    /// <remarks>
    /// <paramref name="columnName"/> must be <see langword="int"/> or other number format
    /// </remarks>
    public static void AddVersionTrigger(this MigrationBuilder builder, string tableName, string columnName)
    {
        builder.Sql(
            $$"""
            CREATE TRIGGER Set_{{tableName}}_{{columnName}}_OnUpdate
            AFTER UPDATE ON {{tableName}}
            BEGIN
                UPDATE {{tableName}}
                SET {{columnName}} = {{columnName}} + 1
                WHERE rowid = NEW.rowid;
            END
            """);
    }
}
