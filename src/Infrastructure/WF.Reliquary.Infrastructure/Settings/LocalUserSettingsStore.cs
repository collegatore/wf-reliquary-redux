using System.Text.Json;
using System.Text.Json.Serialization;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Ports;
using WF.Reliquary.Application.RecruitingMessage;
using WF.Reliquary.Application.Settings;

namespace WF.Reliquary.Infrastructure.Settings;

/// <summary>
/// Persists all user preferences to a single <c>user-settings.json</c> file under
/// <c>%LOCALAPPDATA%\WF Reliquary Redux\</c>.  Also implements
/// <see cref="IDropTableSourceSettingsStore"/> so the drop-table refresh service
/// can read the configured source URL without knowing about the broader settings model.
/// </summary>
public sealed class LocalUserSettingsStore : IUserSettingsStore, IDropTableSourceSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private readonly string rootPath;

    public LocalUserSettingsStore(string? rootPath = null)
    {
        this.rootPath = rootPath ?? DefaultRootPath();
    }

    public static string DefaultRootPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(localAppData))
        {
            localAppData = AppContext.BaseDirectory;
        }

        return Path.Combine(localAppData, "WF Reliquary Redux");
    }

    // ── IUserSettingsStore ──────────────────────────────────────────────────

    public async Task<UserSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = SettingsPath();

        if (!File.Exists(path))
        {
            return UserSettings.Default;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var file = JsonSerializer.Deserialize<SettingsFile>(json, SerializerOptions);

            if (file is null)
            {
                return UserSettings.Default;
            }

            var sourceUrl = Uri.TryCreate(file.SourceUrl, UriKind.Absolute, out var parsed)
                ? parsed
                : UserSettings.Default.SourceUrl;

            var fontSizePt = file.FontSizePt > 0 ? file.FontSizePt : UserSettings.Default.FontSizePt;

            return new UserSettings(
                SourceUrl: sourceUrl,
                WindowWidth: file.WindowWidth > 0 ? file.WindowWidth : UserSettings.Default.WindowWidth,
                WindowHeight: file.WindowHeight > 0 ? file.WindowHeight : UserSettings.Default.WindowHeight,
                FontSizePt: fontSizePt,
                RelicsPanelHeight: file.RelicsPanelHeight > 0 ? file.RelicsPanelHeight : null,
                RecruitingMessageTemplate: ToTemplate(file.RecruitingMessageTemplate),
                WindowX: file.WindowX,
                WindowY: file.WindowY,
                Orientation: file.Orientation ?? UserSettings.Default.Orientation,
                SideBySideRelicsWidth: file.SideBySideRelicsWidth > 0 ? file.SideBySideRelicsWidth : null,
                RelicCardWidthPx: file.RelicCardWidthPx > 0 ? file.RelicCardWidthPx : UserSettings.Default.RelicCardWidthPx,
                ViewMode: file.ViewMode ?? UserSettings.Default.ViewMode,
                SortMode: file.SortMode ?? UserSettings.Default.SortMode,
                SortDescending: file.SortDescending ?? UserSettings.Default.SortDescending,
                Palette: ParsePalette(file.Palette),
                RelicsHideVaulted: file.RelicsHideVaulted ?? UserSettings.Default.RelicsHideVaulted,
                GearHideVaulted: file.GearHideVaulted ?? UserSettings.Default.GearHideVaulted,
                GearHideOwned: file.GearHideOwned ?? UserSettings.Default.GearHideOwned,
                GearHideNotWishlisted: file.GearHideNotWishlisted ?? UserSettings.Default.GearHideNotWishlisted,
                ShowAllRewards: file.ShowAllRewards ?? UserSettings.Default.ShowAllRewards);
        }
        catch (JsonException)
        {
            return UserSettings.Default;
        }
    }

    public async Task SaveAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(rootPath);

        var file = new SettingsFile(
            settings.SourceUrl.ToString(),
            settings.WindowWidth,
            settings.WindowHeight,
            settings.FontSizePt,
            settings.RelicsPanelHeight,
            FromTemplate(settings.RecruitingMessageTemplate),
            settings.WindowX,
            settings.WindowY,
            settings.Orientation,
            settings.SideBySideRelicsWidth,
            settings.RelicCardWidthPx,
            settings.ViewMode,
            settings.SortMode,
            settings.SortDescending,
            settings.Palette.ToString(),
            settings.RelicsHideVaulted,
            settings.GearHideVaulted,
            settings.GearHideOwned,
            settings.GearHideNotWishlisted,
            settings.ShowAllRewards);

        var json = JsonSerializer.Serialize(file, SerializerOptions);
        await File.WriteAllTextAsync(SettingsPath(), json, cancellationToken);
    }

    // ── IDropTableSourceSettingsStore (adapter) ─────────────────────────────

    async Task<DropTableSourceSettings> IDropTableSourceSettingsStore.LoadAsync(CancellationToken cancellationToken)
    {
        var settings = await LoadAsync(cancellationToken);
        return new DropTableSourceSettings(settings.SourceUrl);
    }

    async Task IDropTableSourceSettingsStore.SaveAsync(DropTableSourceSettings settings, CancellationToken cancellationToken)
    {
        var current = await LoadAsync(cancellationToken);
        await SaveAsync(current with { SourceUrl = settings.SourceUrl }, cancellationToken);
    }

    // ── Private ─────────────────────────────────────────────────────────────

    private string SettingsPath() => Path.Combine(rootPath, "user-settings.json");

    /// <summary>
    /// Reads the palette leniently: a missing or unrecognized value falls back to the
    /// default. It is stored as a plain string (not an enum converter) because a JSON
    /// error would reset every setting, so one bad palette name must not throw.
    /// </summary>
    private static ColorPalette ParsePalette(string? value) =>
        Enum.TryParse<ColorPalette>(value, ignoreCase: true, out var palette) && Enum.IsDefined(palette)
            ? palette
            : UserSettings.Default.Palette;

    private static RecruitingMessageTemplate ToTemplate(RecruitingMessageTemplateFile? file)
    {
        if (file is null || string.IsNullOrWhiteSpace(file.Pattern))
            return RecruitingMessageTemplate.Default;

        var groups = (file.Groups ?? [])
            .Select(g => new RecruitingMessageOptionGroup(
                g.Placeholder ?? string.Empty,
                (g.Options ?? []).Select(o => new RecruitingMessageOption(o.Descriptor ?? string.Empty, o.Value ?? string.Empty)).ToList(),
                g.DefaultValue ?? string.Empty))
            .ToList();

        return new RecruitingMessageTemplate(file.Pattern, groups);
    }

    private static RecruitingMessageTemplateFile FromTemplate(RecruitingMessageTemplate template) =>
        new(
            template.Pattern,
            template.Groups
                .Select(g => new RecruitingMessageGroupFile(
                    g.Placeholder,
                    g.Options.Select(o => new RecruitingMessageOptionFile(o.Descriptor, o.Value)).ToList(),
                    g.DefaultValue))
                .ToList());

    // ── DTOs ────────────────────────────────────────────────────────────────

    private sealed record SettingsFile(
        string SourceUrl,
        double WindowWidth,
        double WindowHeight,
        double FontSizePt,
        double? RelicsPanelHeight,
        RecruitingMessageTemplateFile? RecruitingMessageTemplate = null,
        int? WindowX = null,
        int? WindowY = null,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        RelicsMissionsOrientation? Orientation = null,
        double? SideBySideRelicsWidth = null,
        double RelicCardWidthPx = 0,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        RelicViewMode? ViewMode = null,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        RelicSortMode? SortMode = null,
        bool? SortDescending = null,
        string? Palette = null,
        bool? RelicsHideVaulted = null,
        bool? GearHideVaulted = null,
        bool? GearHideOwned = null,
        bool? GearHideNotWishlisted = null,
        bool? ShowAllRewards = null);

    private sealed record RecruitingMessageTemplateFile(
        string? Pattern,
        List<RecruitingMessageGroupFile>? Groups);

    private sealed record RecruitingMessageGroupFile(
        string? Placeholder,
        List<RecruitingMessageOptionFile>? Options,
        string? DefaultValue);

    private sealed record RecruitingMessageOptionFile(
        string? Descriptor,
        string? Value);
}
