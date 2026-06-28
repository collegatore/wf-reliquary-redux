using System.Text.Json;
using System.Text.Json.Serialization;
using WF.Reliquary.Application.DropTables;
using WF.Reliquary.Application.Lfg;
using WF.Reliquary.Application.Ports;
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
        Converters = { new JsonStringEnumConverter() },
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

            // FontSizePt takes precedence; fall back to legacy enum for old settings files.
            var fontSizePt = file.FontSizePt > 0
                ? file.FontSizePt.Value
                : LegacyFontSizeToPt(Enum.IsDefined(file.FontSize) ? file.FontSize : UserFontSize.Medium);

            return new UserSettings(
                SourceUrl: sourceUrl,
                WindowWidth: file.WindowWidth > 0 ? file.WindowWidth : UserSettings.Default.WindowWidth,
                WindowHeight: file.WindowHeight > 0 ? file.WindowHeight : UserSettings.Default.WindowHeight,
                FontSizePt: fontSizePt,
                RelicsPanelHeight: file.RelicsPanelHeight > 0 ? file.RelicsPanelHeight : null,
                LfgTemplate: ToTemplate(file.LfgTemplate),
                WindowX: file.WindowX,
                WindowY: file.WindowY);
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
            PtToLegacyFontSize(settings.FontSizePt),
            settings.RelicsPanelHeight,
            FromTemplate(settings.LfgTemplate),
            settings.WindowX,
            settings.WindowY,
            settings.FontSizePt);

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

    private static double LegacyFontSizeToPt(UserFontSize fs) => fs switch
    {
        UserFontSize.Small => 11.0,
        UserFontSize.Large => 18.0,
        _ => 14.0,
    };

    private static UserFontSize PtToLegacyFontSize(double pt) => pt switch
    {
        11.0 => UserFontSize.Small,
        18.0 => UserFontSize.Large,
        _ => UserFontSize.Medium,
    };

    private static LfgMessageTemplate ToTemplate(LfgTemplateFile? file)
    {
        if (file is null || string.IsNullOrWhiteSpace(file.Pattern))
            return LfgMessageTemplate.Default;

        var groups = (file.Groups ?? [])
            .Select(g => new LfgOptionGroup(
                g.Placeholder ?? string.Empty,
                (g.Options ?? []).Select(o => new LfgOption(o.Descriptor ?? string.Empty, o.Value ?? string.Empty)).ToList(),
                g.DefaultValue ?? string.Empty))
            .ToList();

        return new LfgMessageTemplate(file.Pattern, groups);
    }

    private static LfgTemplateFile FromTemplate(LfgMessageTemplate template) =>
        new(
            template.Pattern,
            template.Groups
                .Select(g => new LfgGroupFile(
                    g.Placeholder,
                    g.Options.Select(o => new LfgOptionFile(o.Descriptor, o.Value)).ToList(),
                    g.DefaultValue))
                .ToList());

    // ── DTOs ────────────────────────────────────────────────────────────────

    private sealed record SettingsFile(
        string SourceUrl,
        double WindowWidth,
        double WindowHeight,
        UserFontSize FontSize,
        double? RelicsPanelHeight,
        LfgTemplateFile? LfgTemplate = null,
        int? WindowX = null,
        int? WindowY = null,
        double? FontSizePt = null);

    private sealed record LfgTemplateFile(
        string? Pattern,
        List<LfgGroupFile>? Groups);

    private sealed record LfgGroupFile(
        string? Placeholder,
        List<LfgOptionFile>? Options,
        string? DefaultValue);

    private sealed record LfgOptionFile(
        string? Descriptor,
        string? Value);
}
