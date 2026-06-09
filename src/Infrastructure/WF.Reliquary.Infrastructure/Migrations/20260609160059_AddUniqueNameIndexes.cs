using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.Reliquary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNameIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reconcile any pre-existing duplicate names before applying the unique
            // indexes, so the migration cannot fail on databases created before names
            // were constrained. Keep the earliest (lowest-id) row in each group and
            // disambiguate the rest by appending their id, which is itself unique.
            migrationBuilder.Sql(
                "UPDATE profiles SET name = name || ' (' || id || ')' " +
                "WHERE id NOT IN (SELECT MIN(id) FROM profiles GROUP BY name);");

            migrationBuilder.Sql(
                "UPDATE wishlists SET name = name || ' (' || id || ')' " +
                "WHERE id NOT IN (SELECT MIN(id) FROM wishlists GROUP BY profile_id, name);");

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_profile_id_name",
                table: "wishlists",
                columns: new[] { "profile_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_name",
                table: "profiles",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wishlists_profile_id_name",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "IX_profiles_name",
                table: "profiles");
        }
    }
}
