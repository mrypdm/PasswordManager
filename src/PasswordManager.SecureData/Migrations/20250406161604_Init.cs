using Microsoft.EntityFrameworkCore.Migrations;
using PasswordManager.SecureData.Extensions;

#nullable disable

namespace PasswordManager.SecureData.Migrations;

/// <inheritdoc />
public partial class Init : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MasterKeyData",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                Version = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MasterKeyData", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SecureItems",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                Salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                Version = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false, defaultValue: 0L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SecureItems", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SecureItems_Name",
            table: "SecureItems",
            column: "Name");

        migrationBuilder.AddVersionTrigger(
            table: "MasterKeyData",
            column: "Version");

        migrationBuilder.AddVersionTrigger(
            table: "SecureItems",
            column: "Version");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MasterKeyData");

        migrationBuilder.DropTable(
            name: "SecureItems");
    }
}
