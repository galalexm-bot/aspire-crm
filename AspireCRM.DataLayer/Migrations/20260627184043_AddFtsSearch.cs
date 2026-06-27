using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspireCRM.DataLayer.Migrations
{
    public partial class AddFtsSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIRTUAL TABLE IF NOT EXISTS FtsSearch USING fts5(
    entity_type UNINDEXED,
    entity_id UNINDEXED,
    title,
    content
);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS FtsSearch");
        }
    }
}
