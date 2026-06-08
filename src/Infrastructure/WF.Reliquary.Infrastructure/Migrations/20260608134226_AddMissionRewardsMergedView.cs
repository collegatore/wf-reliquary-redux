using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WF.Reliquary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionRewardsMergedView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deduplicate rewards that appear more than once within a single mission pool
            // or rotation (same reward_item_id, e.g. Axi Y1 listed twice in Void/Mot) by
            // summing their chances. Grain is reward_item_id, which is the universal key
            // (1:1 with relic_id for relic rewards, the natural key for Aya/Endo). relic_id
            // is carried as a functionally-dependent passthrough. mission_rewards remains the
            // raw, un-deduplicated source of truth; this view is computed, not materialized.
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
        }
    }
}
