using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireCRM.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesPlans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    TenantId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedById = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedById = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPlans", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesPlans");
        }
    }
}
