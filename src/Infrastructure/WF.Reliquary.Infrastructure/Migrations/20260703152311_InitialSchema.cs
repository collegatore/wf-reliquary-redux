using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.Reliquary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gear_vault_status",
                columns: table => new
                {
                    gear_name = table.Column<string>(type: "TEXT", nullable: false),
                    is_vaulted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gear_vault_status", x => x.gear_name);
                });

            migrationBuilder.CreateTable(
                name: "import_metadata",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false),
                    source_url = table.Column<string>(type: "TEXT", nullable: true),
                    source_file_path = table.Column<string>(type: "TEXT", nullable: true),
                    downloaded_at = table.Column<string>(type: "TEXT", nullable: true),
                    source_last_update = table.Column<string>(type: "TEXT", nullable: true),
                    imported_at = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_metadata", x => x.id);
                    table.CheckConstraint("CK_import_metadata_singleton", "id = 1");
                });

            migrationBuilder.CreateTable(
                name: "mission_sources",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    display_label = table.Column<string>(type: "TEXT", nullable: false),
                    region = table.Column<string>(type: "TEXT", nullable: true),
                    node = table.Column<string>(type: "TEXT", nullable: true),
                    mission_type = table.Column<string>(type: "TEXT", nullable: true),
                    qualifier = table.Column<string>(type: "TEXT", nullable: true),
                    source_order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mission_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relics",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    era = table.Column<string>(type: "TEXT", nullable: false),
                    code = table.Column<string>(type: "TEXT", nullable: false),
                    display_name = table.Column<string>(type: "TEXT", nullable: false),
                    reward_item_id = table.Column<long>(type: "INTEGER", nullable: false),
                    is_vaulted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reward_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    normalized_name = table.Column<string>(type: "TEXT", nullable: false),
                    item_kind = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reward_items", x => x.id);
                });

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

            migrationBuilder.CreateTable(
                name: "mission_rewards",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    mission_source_id = table.Column<long>(type: "INTEGER", nullable: false),
                    rotation_id = table.Column<long>(type: "INTEGER", nullable: true),
                    reward_item_id = table.Column<long>(type: "INTEGER", nullable: false),
                    relic_id = table.Column<long>(type: "INTEGER", nullable: true),
                    rarity_label = table.Column<string>(type: "TEXT", nullable: false),
                    chance_percent = table.Column<double>(type: "REAL", nullable: false),
                    source_order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mission_rewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_mission_rewards_mission_sources_mission_source_id",
                        column: x => x.mission_source_id,
                        principalTable: "mission_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reward_rotations",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    mission_source_id = table.Column<long>(type: "INTEGER", nullable: false),
                    rotation_label = table.Column<string>(type: "TEXT", nullable: false),
                    display_label = table.Column<string>(type: "TEXT", nullable: false),
                    source_order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reward_rotations", x => x.id);
                    table.ForeignKey(
                        name: "FK_reward_rotations_mission_sources_mission_source_id",
                        column: x => x.mission_source_id,
                        principalTable: "mission_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "relic_rewards",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    relic_id = table.Column<long>(type: "INTEGER", nullable: false),
                    reward_item_id = table.Column<long>(type: "INTEGER", nullable: false),
                    reward_tier = table.Column<string>(type: "TEXT", nullable: false),
                    source_rarity_label = table.Column<string>(type: "TEXT", nullable: false),
                    source_chance_percent = table.Column<double>(type: "REAL", nullable: false),
                    source_order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relic_rewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_relic_rewards_relics_relic_id",
                        column: x => x.relic_id,
                        principalTable: "relics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "owned_gear",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    profile_id = table.Column<long>(type: "INTEGER", nullable: false),
                    gear_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_owned_gear", x => x.id);
                });

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
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false),
                    last_profile_id = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.id);
                    table.CheckConstraint("CK_user_preferences_singleton", "id = 1");
                    table.ForeignKey(
                        name: "FK_user_preferences_profiles_last_profile_id",
                        column: x => x.last_profile_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "wishlists",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    profile_id = table.Column<long>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlists", x => x.id);
                    table.ForeignKey(
                        name: "FK_wishlists_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wishlist_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    wishlist_id = table.Column<long>(type: "INTEGER", nullable: false),
                    part_normalized_name = table.Column<string>(type: "TEXT", nullable: false),
                    gear_name = table.Column<string>(type: "TEXT", nullable: false),
                    part_display_name = table.Column<string>(type: "TEXT", nullable: false),
                    added_at = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlist_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_wishlist_items_wishlists_wishlist_id",
                        column: x => x.wishlist_id,
                        principalTable: "wishlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mission_rewards_mission_source_id",
                table: "mission_rewards",
                column: "mission_source_id");

            migrationBuilder.CreateIndex(
                name: "IX_owned_gear_profile_id_gear_name",
                table: "owned_gear",
                columns: new[] { "profile_id", "gear_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_active_wishlist_id",
                table: "profiles",
                column: "active_wishlist_id");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_name",
                table: "profiles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_relic_rewards_relic_id",
                table: "relic_rewards",
                column: "relic_id");

            migrationBuilder.CreateIndex(
                name: "IX_relics_display_name",
                table: "relics",
                column: "display_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reward_items_normalized_name",
                table: "reward_items",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reward_rotations_mission_source_id_rotation_label",
                table: "reward_rotations",
                columns: new[] { "mission_source_id", "rotation_label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_preferences_last_profile_id",
                table: "user_preferences",
                column: "last_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_items_wishlist_id_part_normalized_name",
                table: "wishlist_items",
                columns: new[] { "wishlist_id", "part_normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_profile_id",
                table: "wishlists",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_wishlists_profile_id_name",
                table: "wishlists",
                columns: new[] { "profile_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_owned_gear_profiles_profile_id",
                table: "owned_gear",
                column: "profile_id",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profiles_wishlists_active_wishlist_id",
                table: "profiles",
                column: "active_wishlist_id",
                principalTable: "wishlists",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Deduplicate rewards that appear more than once within a single mission pool
            // or rotation by summing their chances. mission_rewards remains the raw source.
            migrationBuilder.Sql("""
                CREATE VIEW mission_rewards_merged AS
                SELECT
                    mission_source_id,
                    rotation_id,
                    reward_item_id,
                    relic_id,
                    SUM(chance_percent) AS chance_percent,
                    MIN(source_order)   AS source_order
                FROM mission_rewards
                GROUP BY mission_source_id, rotation_id, reward_item_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW mission_rewards_merged;");

            migrationBuilder.DropForeignKey(
                name: "FK_wishlists_profiles_profile_id",
                table: "wishlists");

            migrationBuilder.DropTable(
                name: "gear_vault_status");

            migrationBuilder.DropTable(
                name: "import_metadata");

            migrationBuilder.DropTable(
                name: "mission_rewards");

            migrationBuilder.DropTable(
                name: "owned_gear");

            migrationBuilder.DropTable(
                name: "relic_rewards");

            migrationBuilder.DropTable(
                name: "reward_items");

            migrationBuilder.DropTable(
                name: "reward_rotations");

            migrationBuilder.DropTable(
                name: "source_files");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "wishlist_items");

            migrationBuilder.DropTable(
                name: "relics");

            migrationBuilder.DropTable(
                name: "mission_sources");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "wishlists");
        }
    }
}
