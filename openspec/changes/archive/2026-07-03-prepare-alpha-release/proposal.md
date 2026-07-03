# Prepare Alpha Release (1.0.0-alpha)

## Why

The app is functionally ready for its first public preview, but it still carries development-phase leftovers: a stale `0.1.0-boilerplate` version constant shown nowhere in the UI, no assembly/package metadata, six accumulated EF Core migrations, settings defaults duplicated across three code locations, stale technical documentation, and no user-facing docs at all. A release audit (2026-07-03) identified these gaps; this change closes them so the app can be tagged and shipped as **1.0.0-alpha**. Distribution/packaging (publish profiles, CI release pipeline) is deliberately out of scope — it is still under discussion and will be a separate change.

## What Changes

- **Version identity (single source).** Add a repo-root `Directory.Build.props` carrying `Version` (`1.0.0-alpha`), `Product`, `Authors`, `Copyright`, and `RepositoryUrl` for all projects. `ApplicationInfo` reads the assembly informational version instead of holding its own constant. The main window title shows the version (`WF Reliquary Redux 1.0.0-alpha`), and the app logs its name + version to `app.log` on startup. `app.manifest`'s `assemblyIdentity` version is synced.
- **Migration squash.** The six EF Core migrations are squashed into a single `InitialSchema` migration. Prerequisite: bump the pinned `dotnet-ef` local tool from 9.0.6 to 10.0.9 to match the EF Core 10.0.9 packages. **BREAKING (pre-V1 sanctioned):** existing databases carry migration-history rows that no longer match; users/developers must delete `%LOCALAPPDATA%\WF Reliquary Redux\` before first run. The pre-V1 persistence rule in `docs/technical-reference.md` is updated to record that the squash was performed at 1.0-alpha.
- **Settings defaults consolidated and brushed.** Every fallback in `LocalUserSettingsStore` routes through `UserSettings.Default` (no hardcoded literals in the store or record parameter defaults drifting apart). Default window size grows from 900×600 to 1280×800; default relic card base width changes from 300 px to 200 px; default Relics/Missions layout orientation changes from Stacked to Side-by-side; the default drop-table source URL changes from the raw DigitalOcean CDN address to the canonical `https://www.warframe.com/droptables` (which redirects to the current CDN asset — verified 2026-07-03).
- **Documentation refresh.** Fix `docs/technical-reference.md` staleness (EF Core 9 → 10, wrong migrations path, "only one init migration" claim). Add a `CHANGELOG.md` carrying the 1.0.0-alpha notes including the mandatory data-wipe notice. Create a **User Guide** (`docs/user-guide.md`) briefly describing all non-obvious features.
- **Attribution and triage aids.** Add a `NOTICE` file with third-party attributions (Caly TreeDataGrid fork and other dependencies). `FileErrorLog` entries include the app version so preview bug reports are traceable to a build.

## Capabilities

### New Capabilities

- `app-version-identity`: single-source version/assembly metadata, version display in the window title, and startup version logging.
- `release-documentation`: user-facing release documents — CHANGELOG with release notes, User Guide covering non-obvious features, NOTICE with third-party attributions.

### Modified Capabilities

- `app-window-persistence`: the built-in default window dimensions change from 900 × 600 to 1280 × 800.
- `relic-card-width-setting`: the default relic card base width changes from 300 px to 200 px.
- `error-reporting`: log entries additionally record the application version.
- `relics-missions-layout-orientation`: the default layout when no preference is persisted changes from Stacked to Side-by-side.
- `drop-table-source-refresh`: the default source URL changes to `https://www.warframe.com/droptables`; downloads follow the HTTP redirect to DE's current CDN location.

*(The migration squash and settings-defaults consolidation do not change any spec-level requirement — `ef-core-data-layer` still mandates migrations as the sole schema mechanism, and settings behavior is unchanged apart from the default values above — so they appear only in design/tasks.)*

## Impact

- **Build system:** new `Directory.Build.props` affects all six projects; `.config/dotnet-tools.json` bumps `dotnet-ef` to 10.0.9.
- **Code:** `ApplicationInfo` (Application), `MainWindowViewModel.Title` (Presentation), startup logging path (Presentation/Infrastructure), `FileErrorLog` (Infrastructure), `UserSettings` + `LocalUserSettingsStore` defaults (Application/Infrastructure), `app.manifest` (Presentation).
- **Database:** `src/Infrastructure/WF.Reliquary.Infrastructure/Migrations/` regenerated as a single migration; existing local databases must be wiped (pre-V1 rule, notify in CHANGELOG).
- **Docs:** `docs/technical-reference.md` (staleness + rule update), new `docs/user-guide.md`, new `CHANGELOG.md`, new `NOTICE`.
- **Tests:** `ApplicationInfoTests` (version format), settings-store default tests, window-persistence default-size tests, relic-card-width default tests, error-log entry format tests.
