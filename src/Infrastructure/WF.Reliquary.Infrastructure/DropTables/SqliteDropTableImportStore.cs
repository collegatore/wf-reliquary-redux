using Microsoft.EntityFrameworkCore;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Infrastructure.Storage;
using WF.Reliquary.Infrastructure.Storage.Entities;

namespace WF.Reliquary.Infrastructure.DropTables;

public sealed class SqliteDropTableImportStore : IDropTableImportStore
{
    private readonly DbContextOptions<ReliquaryDbContext> options;

    public SqliteDropTableImportStore(string? databasePath = null)
    {
        DatabasePath = databasePath ?? DefaultDatabasePath();
        options = ReliquaryDbContext.BuildOptions(DatabasePath);
    }

    public string DatabasePath { get; }

    public static string DefaultDatabasePath()
    {
        return Path.Combine(LocalDropTableSourceStore.DefaultRootPath(), "drop-table-import.sqlite");
    }

    public async Task ReplaceAsync(ParsedDropTableImport import, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(import);

        var directory = Path.GetDirectoryName(DatabasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var context = new ReliquaryDbContext(options);
        await context.Database.MigrateAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Clear snapshot tables in FK-dependency order.
        await context.MissionRewards.ExecuteDeleteAsync(cancellationToken);
        await context.RewardRotations.ExecuteDeleteAsync(cancellationToken);
        await context.RelicRewards.ExecuteDeleteAsync(cancellationToken);
        await context.MissionSources.ExecuteDeleteAsync(cancellationToken);
        await context.Relics.ExecuteDeleteAsync(cancellationToken);
        await context.RewardItems.ExecuteDeleteAsync(cancellationToken);
        await context.GearVaultStatuses.ExecuteDeleteAsync(cancellationToken);
        await context.ImportMetadata.ExecuteDeleteAsync(cancellationToken);

        // Insert reward items — SaveChanges to populate auto-generated IDs.
        var rewardItemEntities = import.RewardItems
            .Select(ri => new RewardItemEntity { Name = ri.Name, NormalizedName = ri.NormalizedName, ItemKind = ri.ItemKind })
            .ToList();
        context.RewardItems.AddRange(rewardItemEntities);
        await context.SaveChangesAsync(cancellationToken);
        var rewardItemIdByName = rewardItemEntities.ToDictionary(e => e.Name, e => e.Id, StringComparer.OrdinalIgnoreCase);

        // Insert relics — SaveChanges to populate IDs.
        var relicEntities = import.Relics
            .Select(r => new RelicEntity
            {
                Era = r.Era,
                Code = r.Code,
                DisplayName = r.DisplayName,
                RewardItemId = rewardItemIdByName[r.RewardItemName]
            })
            .ToList();
        context.Relics.AddRange(relicEntities);
        await context.SaveChangesAsync(cancellationToken);
        var relicIdByDisplayName = relicEntities.ToDictionary(e => e.DisplayName, e => e.Id, StringComparer.OrdinalIgnoreCase);

        // Insert relic rewards.
        context.RelicRewards.AddRange(import.RelicRewards.Select(rr => new RelicRewardEntity
        {
            RelicId = relicIdByDisplayName[rr.RelicDisplayName],
            RewardItemId = rewardItemIdByName[rr.RewardItemName],
            RewardTier = rr.RewardTier,
            SourceRarityLabel = rr.SourceRarityLabel,
            SourceChancePercent = (double)rr.SourceChancePercent,
            SourceOrder = rr.SourceOrder
        }));

        // Insert mission sources — SaveChanges to populate IDs.
        var missionSourceEntities = import.MissionSources
            .Select(ms => new MissionSourceEntity
            {
                DisplayLabel = ms.DisplayLabel,
                Region = ms.Region,
                Node = ms.Node,
                MissionType = ms.MissionType,
                Qualifier = ms.Qualifier,
                SourceOrder = ms.SourceOrder
            })
            .ToList();
        context.MissionSources.AddRange(missionSourceEntities);
        await context.SaveChangesAsync(cancellationToken);
        var missionSourceIdByKey = missionSourceEntities
            .ToDictionary(e => MissionSourceKey.From(e.DisplayLabel, e.SourceOrder), e => e.Id);

        // Insert reward rotations — SaveChanges to populate IDs.
        var rotationEntities = import.RewardRotations
            .Select(r => new RewardRotationEntity
            {
                MissionSourceId = missionSourceIdByKey[MissionSourceKey.From(r.MissionSourceLabel, r.MissionSourceOrder)],
                RotationLabel = r.RotationLabel,
                DisplayLabel = r.DisplayLabel,
                SourceOrder = r.SourceOrder
            })
            .ToList();
        context.RewardRotations.AddRange(rotationEntities);
        await context.SaveChangesAsync(cancellationToken);
        var rotationIdByKey = rotationEntities
            .Select((e, i) => (e, rotation: import.RewardRotations[i]))
            .ToDictionary(
                x => RotationKey(x.rotation.MissionSourceLabel, x.rotation.MissionSourceOrder, x.rotation.RotationLabel),
                x => x.e.Id);

        // Insert mission rewards and import metadata.
        context.MissionRewards.AddRange(import.MissionRewards.Select(mr => new MissionRewardEntity
        {
            MissionSourceId = missionSourceIdByKey[MissionSourceKey.From(mr.MissionSourceLabel, mr.MissionSourceOrder)],
            RotationId = mr.RotationLabel is null
                ? null
                : rotationIdByKey[RotationKey(mr.MissionSourceLabel, mr.MissionSourceOrder, mr.RotationLabel)],
            RewardItemId = rewardItemIdByName[mr.RewardItemName],
            RelicId = mr.RelicDisplayName is null ? null : relicIdByDisplayName[mr.RelicDisplayName],
            RarityLabel = mr.RarityLabel,
            ChancePercent = (double)mr.ChancePercent,
            SourceOrder = mr.SourceOrder
        }));

        context.ImportMetadata.Add(new ImportMetadataEntity
        {
            Id = 1,
            SourceUrl = import.SourceUrl.ToString(),
            SourceFilePath = null,
            DownloadedAt = import.DownloadedAt.ToString("O"),
            SourceLastUpdate = import.SourceLastUpdate,
            ImportedAt = import.ImportedAt.ToString("O")
        });

        await context.SaveChangesAsync(cancellationToken);

        // Compute gear vault status and relic vault flags via set-based SQL.
        var insertGearVaultSql = SqlQueryLoader.Load("InsertGearVaultStatus.sql");
        var updateRelicVaultSql = SqlQueryLoader.Load("UpdateRelicVaultStatus.sql");
        await context.Database.ExecuteSqlRawAsync(insertGearVaultSql, cancellationToken);
        await context.Database.ExecuteSqlRawAsync(updateRelicVaultSql, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static string RotationKey(string missionSourceLabel, int missionSourceOrder, string rotationLabel)
        => $"{missionSourceLabel}|{missionSourceOrder}|{rotationLabel}";

    private readonly record struct MissionSourceKey(string DisplayLabel, int SourceOrder)
    {
        public static MissionSourceKey From(string displayLabel, int sourceOrder)
            => new(displayLabel.ToUpperInvariant(), sourceOrder);
    }
}
