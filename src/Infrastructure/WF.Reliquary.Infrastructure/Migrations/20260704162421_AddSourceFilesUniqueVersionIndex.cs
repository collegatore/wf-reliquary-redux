using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.Reliquary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceFilesUniqueVersionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM source_files WHERE id NOT IN (SELECT MAX(id) FROM source_files GROUP BY source_last_update)");

            migrationBuilder.CreateIndex(
                name: "IX_source_files_source_last_update",
                table: "source_files",
                column: "source_last_update",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_source_files_source_last_update",
                table: "source_files");
        }
    }
}
