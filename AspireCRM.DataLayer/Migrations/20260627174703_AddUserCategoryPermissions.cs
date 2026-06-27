using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireCRM.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCategoryPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCategoryPermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<long>(type: "INTEGER", nullable: false),
                    PermissionLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TenantId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedById = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedById = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategoryPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategoryPermissions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryPermissions_CategoryId",
                table: "UserCategoryPermissions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategoryPermissions_UserId_CategoryId",
                table: "UserCategoryPermissions",
                columns: new[] { "UserId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCategoryPermissions");
        }
    }
}
