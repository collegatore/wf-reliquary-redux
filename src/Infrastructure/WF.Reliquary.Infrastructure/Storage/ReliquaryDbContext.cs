using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.Storage;

internal sealed class ReliquaryDbContext : DbContext
{
    public ReliquaryDbContext(DbContextOptions<ReliquaryDbContext> options) : base(options) { }

    public DbSet<RewardItemEntity> RewardItems => Set<RewardItemEntity>();
    public DbSet<RelicEntity> Relics => Set<RelicEntity>();
    public DbSet<RelicRewardEntity> RelicRewards => Set<RelicRewardEntity>();
    public DbSet<MissionSourceEntity> MissionSources => Set<MissionSourceEntity>();
    public DbSet<RewardRotationEntity> RewardRotations => Set<RewardRotationEntity>();
    public DbSet<MissionRewardEntity> MissionRewards => Set<MissionRewardEntity>();
    public DbSet<GearVaultStatusEntity> GearVaultStatuses => Set<GearVaultStatusEntity>();
    public DbSet<ImportMetadataEntity> ImportMetadata => Set<ImportMetadataEntity>();
    public DbSet<ProfileEntity> Profiles => Set<ProfileEntity>();
    public DbSet<WishlistEntity> Wishlists => Set<WishlistEntity>();
    public DbSet<WishlistItemEntity> WishlistItems => Set<WishlistItemEntity>();
    public DbSet<OwnedGearEntity> OwnedGear => Set<OwnedGearEntity>();
    public DbSet<UserPreferencesEntity> UserPreferences => Set<UserPreferencesEntity>();
    public DbSet<SourceFileEntity> SourceFiles => Set<SourceFileEntity>();

    internal static DbContextOptions<ReliquaryDbContext> BuildOptions(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true
        }.ConnectionString;

        return new DbContextOptionsBuilder<ReliquaryDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── reward_items ──────────────────────────────────────────────────────────
        modelBuilder.Entity<RewardItemEntity>(entity =>
        {
            entity.ToTable("reward_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.NormalizedName).HasColumnName("normalized_name").IsRequired();
            entity.Property(e => e.ItemKind).HasColumnName("item_kind").IsRequired();
            entity.HasIndex(e => e.NormalizedName).IsUnique();
        });

        // ── relics ────────────────────────────────────────────────────────────────
        modelBuilder.Entity<RelicEntity>(entity =>
        {
            entity.ToTable("relics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Era).HasColumnName("era").IsRequired();
            entity.Property(e => e.Code).HasColumnName("code").IsRequired();
            entity.Property(e => e.DisplayName).HasColumnName("display_name").IsRequired();
            entity.Property(e => e.RewardItemId).HasColumnName("reward_item_id");
            entity.Property(e => e.IsVaulted).HasColumnName("is_vaulted").HasDefaultValue(false);
            entity.HasIndex(e => e.DisplayName).IsUnique();
            entity.HasMany(e => e.RelicRewards)
                  .WithOne()
                  .HasForeignKey(rr => rr.RelicId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── relic_rewards ─────────────────────────────────────────────────────────
        modelBuilder.Entity<RelicRewardEntity>(entity =>
        {
            entity.ToTable("relic_rewards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.RelicId).HasColumnName("relic_id");
            entity.Property(e => e.RewardItemId).HasColumnName("reward_item_id");
            entity.Property(e => e.RewardTier).HasColumnName("reward_tier").IsRequired();
            entity.Property(e => e.SourceRarityLabel).HasColumnName("source_rarity_label").IsRequired();
            entity.Property(e => e.SourceChancePercent).HasColumnName("source_chance_percent");
            entity.Property(e => e.SourceOrder).HasColumnName("source_order");
        });

        // ── mission_sources ───────────────────────────────────────────────────────
        modelBuilder.Entity<MissionSourceEntity>(entity =>
        {
            entity.ToTable("mission_sources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.DisplayLabel).HasColumnName("display_label").IsRequired();
            entity.Property(e => e.Region).HasColumnName("region");
            entity.Property(e => e.Node).HasColumnName("node");
            entity.Property(e => e.MissionType).HasColumnName("mission_type");
            entity.Property(e => e.Qualifier).HasColumnName("qualifier");
            entity.Property(e => e.SourceOrder).HasColumnName("source_order");
            entity.HasMany(e => e.RewardRotations)
                  .WithOne()
                  .HasForeignKey(rr => rr.MissionSourceId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.MissionRewards)
                  .WithOne()
                  .HasForeignKey(mr => mr.MissionSourceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── reward_rotations ──────────────────────────────────────────────────────
        modelBuilder.Entity<RewardRotationEntity>(entity =>
        {
            entity.ToTable("reward_rotations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.MissionSourceId).HasColumnName("mission_source_id");
            entity.Property(e => e.RotationLabel).HasColumnName("rotation_label").IsRequired();
            entity.Property(e => e.DisplayLabel).HasColumnName("display_label").IsRequired();
            entity.Property(e => e.SourceOrder).HasColumnName("source_order");
            entity.HasIndex(e => new { e.MissionSourceId, e.RotationLabel }).IsUnique();
        });

        // ── mission_rewards ───────────────────────────────────────────────────────
        modelBuilder.Entity<MissionRewardEntity>(entity =>
        {
            entity.ToTable("mission_rewards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.MissionSourceId).HasColumnName("mission_source_id");
            entity.Property(e => e.RotationId).HasColumnName("rotation_id");
            entity.Property(e => e.RewardItemId).HasColumnName("reward_item_id");
            entity.Property(e => e.RelicId).HasColumnName("relic_id");
            entity.Property(e => e.RarityLabel).HasColumnName("rarity_label").IsRequired();
            entity.Property(e => e.ChancePercent).HasColumnName("chance_percent");
            entity.Property(e => e.SourceOrder).HasColumnName("source_order");
        });

        // ── gear_vault_status ─────────────────────────────────────────────────────
        modelBuilder.Entity<GearVaultStatusEntity>(entity =>
        {
            entity.ToTable("gear_vault_status");
            entity.HasKey(e => e.GearName);
            entity.Property(e => e.GearName).HasColumnName("gear_name").ValueGeneratedNever();
            entity.Property(e => e.IsVaulted).HasColumnName("is_vaulted");
        });

        // ── import_metadata ───────────────────────────────────────────────────────
        modelBuilder.Entity<ImportMetadataEntity>(entity =>
        {
            entity.ToTable("import_metadata", t => t.HasCheckConstraint("CK_import_metadata_singleton", "id = 1"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.SourceUrl).HasColumnName("source_url");
            entity.Property(e => e.DownloadedAt).HasColumnName("downloaded_at");
            entity.Property(e => e.SourceLastUpdate).HasColumnName("source_last_update");
            entity.Property(e => e.ImportedAt).HasColumnName("imported_at").IsRequired();
        });

        // ── profiles ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<ProfileEntity>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.ActiveWishlistId).HasColumnName("active_wishlist_id");
            // Profile names are unique. Case-insensitive uniqueness is enforced by the
            // name modal/view-model guard; this index is a best-effort exact-case backstop.
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasOne<WishlistEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.ActiveWishlistId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        // ── wishlists ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<WishlistEntity>(entity =>
        {
            entity.ToTable("wishlists");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.HasIndex(e => e.ProfileId);
            // Wishlist names are unique within their profile (exact-case backstop; the
            // case-insensitive rule is enforced by the name modal/view-model guard).
            entity.HasIndex(e => new { e.ProfileId, e.Name }).IsUnique();
            entity.HasOne<ProfileEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.ProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── wishlist_items ────────────────────────────────────────────────────────
        modelBuilder.Entity<WishlistItemEntity>(entity =>
        {
            entity.ToTable("wishlist_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.WishlistId).HasColumnName("wishlist_id");
            entity.Property(e => e.PartNormalizedName).HasColumnName("part_normalized_name").IsRequired();
            entity.Property(e => e.GearName).HasColumnName("gear_name").IsRequired();
            entity.Property(e => e.PartDisplayName).HasColumnName("part_display_name").IsRequired();
            entity.Property(e => e.AddedAt).HasColumnName("added_at").IsRequired();
            entity.HasIndex(e => new { e.WishlistId, e.PartNormalizedName }).IsUnique();
            entity.HasOne<WishlistEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.WishlistId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── owned_gear ────────────────────────────────────────────────────────────
        modelBuilder.Entity<OwnedGearEntity>(entity =>
        {
            entity.ToTable("owned_gear");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.GearName).HasColumnName("gear_name").IsRequired();
            entity.HasIndex(e => new { e.ProfileId, e.GearName }).IsUnique();
            entity.HasOne<ProfileEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.ProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── source_files ──────────────────────────────────────────────────────────
        modelBuilder.Entity<SourceFileEntity>(entity =>
        {
            entity.ToTable("source_files");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.SourceUrl).HasColumnName("source_url").IsRequired();
            entity.Property(e => e.DownloadedAt).HasColumnName("downloaded_at").IsRequired();
            entity.Property(e => e.SourceLastUpdate).HasColumnName("source_last_update").IsRequired();
            entity.HasIndex(e => e.SourceLastUpdate).IsUnique();
        });

        // ── user_preferences ──────────────────────────────────────────────────────
        modelBuilder.Entity<UserPreferencesEntity>(entity =>
        {
            entity.ToTable("user_preferences", t => t.HasCheckConstraint("CK_user_preferences_singleton", "id = 1"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.LastProfileId).HasColumnName("last_profile_id");
            entity.HasOne<ProfileEntity>()
                  .WithMany()
                  .HasForeignKey(e => e.LastProfileId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });
    }
}
