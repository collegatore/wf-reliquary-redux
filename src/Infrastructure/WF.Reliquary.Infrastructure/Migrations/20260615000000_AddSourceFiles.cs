using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.Reliquary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "source_files",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    source_url = table.Column<string>(type: "TEXT", nullable: false),
                    downloaded_at = table.Column<string>(type: "TEXT", nullable: false),
                    source_last_update = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_files", x => x.id);
                });

            // Make import_metadata.source_file_path nullable (SQLite column recreation).
            migrationBuilder.Sql(
                "CREATE TABLE import_metadata_new (" +
                "id INTEGER NOT NULL, " +
                "source_url TEXT, " +
                "source_file_path TEXT, " +
                "downloaded_at TEXT, " +
                "source_last_update TEXT, " +
                "imported_at TEXT NOT NULL, " +
                "CONSTRAINT CK_import_metadata_singleton CHECK (id = 1), " +
                "CONSTRAINT PK_import_metadata PRIMARY KEY (id))");

            migrationBuilder.Sql(
                "INSERT INTO import_metadata_new " +
                "SELECT id, source_url, source_file_path, downloaded_at, source_last_update, imported_at " +
                "FROM import_metadata");

            migrationBuilder.Sql("DROP TABLE import_metadata");

            migrationBuilder.Sql("ALTER TABLE import_metadata_new RENAME TO import_metadata");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "source_files");

            migrationBuilder.Sql(
                "CREATE TABLE import_metadata_new (" +
                "id INTEGER NOT NULL, " +
                "source_url TEXT, " +
                "source_file_path TEXT NOT NULL DEFAULT '', " +
                "downloaded_at TEXT, " +
                "source_last_update TEXT, " +
                "imported_at TEXT NOT NULL, " +
                "CONSTRAINT CK_import_metadata_singleton CHECK (id = 1), " +
                "CONSTRAINT PK_import_metadata PRIMARY KEY (id))");

            migrationBuilder.Sql(
                "INSERT INTO import_metadata_new " +
                "SELECT id, source_url, COALESCE(source_file_path, ''), downloaded_at, source_last_update, imported_at " +
                "FROM import_metadata");

            migrationBuilder.Sql("DROP TABLE import_metadata");

            migrationBuilder.Sql("ALTER TABLE import_metadata_new RENAME TO import_metadata");
        }
    }
}
