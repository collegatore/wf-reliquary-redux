using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.Reliquary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the profiles table (active_wishlist_id FK → wishlists, SetNull).
            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<string>(type: "TEXT", nullable: false),
                    active_wishlist_id = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_profiles_wishlists_active_wishlist_id",
                        column: x => x.active_wishlist_id,
                        principalTable: "wishlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_profiles_active_wishlist_id",
                table: "profiles",
                column: "active_wishlist_id");

            // 2. Seed a Default profile so existing wishlists have a home.
            migrationBuilder.Sql(
                "INSERT INTO profiles (name, created_at) " +
                "VALUES ('Default', strftime('%Y-%m-%dT%H:%M:%fZ', 'now'));");

            // 3. Add wishlists.profile_id, assign all existing rows to the Default profile,
            //    then index it. The FK is added after data is valid (see step 6).
            migrationBuilder.AddColumn<long>(
                name: "profile_id",
                table: "wishlists",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(
                "UPDATE wishlists SET profile_id = (SELECT id FROM profiles ORDER BY id LIMIT 1);");

            // 4. Carry the previously-active wishlist into the Default profile's
            //    active_wishlist_id before the old preference column is dropped.
            migrationBuilder.Sql(
                "UPDATE profiles " +
                "SET active_wishlist_id = (SELECT last_wishlist_id FROM user_preferences WHERE id = 1) " +
                "WHERE id = (SELECT id FROM profiles ORDER BY id LIMIT 1);");

            // 5. Replace user_preferences.last_wishlist_id with last_profile_id pointing at
            //    the Default profile.
            migrationBuilder.DropForeignKey(
                name: "FK_user_preferences_wishlists_last_wishlist_id",
                table: "user_preferences");

            migrationBuilder.DropIndex(
                name: "IX_user_preferences_last_wishlist_id",
                table: "user_preferences");

            migrationBuilder.DropColumn(
                name: "last_wishlist_id",
                table: "user_preferences");

            migrationBuilder.AddColumn<long>(
                name: "last_profile_id",
                table: "user_preferences",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE user_preferences SET last_profile_id = (SELECT id FROM profiles ORDER BY id LIMIT 1) " +
                "WHERE id = 1;");

            migrationBuilder.CreateIndex(
                name: "IX_user_preferences_last_profile_id",
                table: "user_preferences",
                column: "last_profile_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_preferences_profiles_last_profile_id",
                table: "user_preferences",
                column: "last_profile_id",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // 6. Now that every wishlist has a valid profile_id, add the FK + index.
            migrationBuilder.CreateIndex(
                name: "IX_wishlists_profile_id",
                table: "wishlists",
                column: "profile_id");

            migrationBuilder.AddForeignKey(
                name: "FK_wishlists_profiles_profile_id",
                table: "wishlists",
                column: "profile_id",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore user_preferences.last_wishlist_id from the Default profile's
            // active wishlist before tearing the profile schema down.
            migrationBuilder.DropForeignKey(
                name: "FK_user_preferences_profiles_last_profile_id",
                table: "user_preferences");

            migrationBuilder.DropForeignKey(
                name: "FK_wishlists_profiles_profile_id",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "IX_wishlists_profile_id",
                table: "wishlists");

            migrationBuilder.DropIndex(
                name: "IX_user_preferences_last_profile_id",
                table: "user_preferences");

            migrationBuilder.DropColumn(
                name: "last_profile_id",
                table: "user_preferences");

            migrationBuilder.AddColumn<long>(
                name: "last_wishlist_id",
                table: "user_preferences",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE user_preferences SET last_wishlist_id = " +
                "(SELECT active_wishlist_id FROM profiles ORDER BY id LIMIT 1) WHERE id = 1;");

            migrationBuilder.DropColumn(
                name: "profile_id",
                table: "wishlists");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.CreateIndex(
                name: "IX_user_preferences_last_wishlist_id",
                table: "user_preferences",
                column: "last_wishlist_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_preferences_wishlists_last_wishlist_id",
                table: "user_preferences",
                column: "last_wishlist_id",
                principalTable: "wishlists",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
