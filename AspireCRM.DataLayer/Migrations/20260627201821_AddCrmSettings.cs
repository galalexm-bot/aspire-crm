using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireCRM.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantId = table.Column<long>(type: "INTEGER", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    EntityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Field = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ChangedById = table.Column<long>(type: "INTEGER", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CrmSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DefaultCurrencyId = table.Column<long>(type: "INTEGER", nullable: true),
                    DefaultCountry = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyAddress = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyPhone = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyEmail = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedById = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedById = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmSettings_Currencies_DefaultCurrencyId",
                        column: x => x.DefaultCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ChangedAt",
                table: "AuditLogs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CrmSettings_DefaultCurrencyId",
                table: "CrmSettings",
                column: "DefaultCurrencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CrmSettings");
        }
    }
}
