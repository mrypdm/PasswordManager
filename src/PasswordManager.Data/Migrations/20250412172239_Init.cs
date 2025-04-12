using Microsoft.EntityFrameworkCore.Migrations;
using PasswordManager.Data.Extensions;

#nullable disable

namespace PasswordManager.Data.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "EncryptedItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                Salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                Version = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EncryptedItems", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "KeyData",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                Salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                Version = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_KeyData", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_EncryptedItems_Name",
            table: "EncryptedItems",
            column: "Name");

        migrationBuilder.AddVersionTrigger(
            table: "EncryptedItems",
            column: "Version");

        migrationBuilder.AddVersionTrigger(
            table: "KeyData",
            column: "Version");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "EncryptedItems");

        migrationBuilder.DropTable(
            name: "KeyData");
    }
}
