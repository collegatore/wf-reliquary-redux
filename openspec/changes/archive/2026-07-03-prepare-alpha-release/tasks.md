## 1. Version identity

- [x] 1.1 Add repo-root `Directory.Build.props` with `Version` (`1.0.0-alpha`), `Product`, `Authors`, `Copyright`, and `RepositoryUrl`; verify all six projects pick it up on rebuild
- [x] 1.2 Rework `ApplicationInfo.Version` to read the Application assembly's informational version (trimming any `+<metadata>` suffix); remove the `0.1.0-boilerplate` constant; update `ApplicationInfoTests`
- [x] 1.3 Change `MainWindowViewModel.Title` to include the version (via `GetStartupSummary()`); update shell/title tests
- [x] 1.4 Log the startup summary (name + version) to `app.log` from `MainWindow`'s production startup path via `IErrorLog.AppendAsync`
- [x] 1.5 Sync `app.manifest` `assemblyIdentity` version to `1.0.0.0`

## 2. Migration squash

- [x] 2.1 Bump `dotnet-ef` to `10.0.9` in `.config/dotnet-tools.json` and run `dotnet tool restore`
- [x] 2.2 Record the raw `Sql(...)` statements from all six existing migrations (merged view, vault-status objects, unique indexes, seeds) for the carry-over diff
- [x] 2.3 Delete `src/Infrastructure/WF.Reliquary.Infrastructure/Migrations/` and scaffold a fresh `InitialSchema` migration with `dotnet ef migrations add`
- [x] 2.4 Carry any raw SQL the model snapshot cannot express into the new `InitialSchema.Up()`; diff against 2.2 to confirm nothing was lost
- [x] 2.5 Run the full integration suite against a fresh database; verify merged-view, vault-status, and unique-index behavior
- [x] 2.6 Delete the local `%LOCALAPPDATA%\WF Reliquary Redux\` data directory (developer machine re-initialization — pre-V1 rule notification)

## 3. Settings defaults

- [x] 3.1 Consolidate defaults so `UserSettings.Default` is the single authority; remove duplicated literals from the record's parameter defaults and from `LocalUserSettingsStore.LoadAsync` fallbacks
- [x] 3.2 Change defaults in `UserSettings.Default`: window size 1280 × 800, relic card base width 200 px, Relics/Missions orientation Side-by-side
- [x] 3.3 Change `DropTableSourceDefaults.SourceUrl` to `https://www.warframe.com/droptables`; verify a real download follows the redirect and imports (one-off manual check or existing integration coverage)
- [x] 3.4 Update affected tests (settings-store defaults, window-persistence default size, relic-card-width default, default orientation, default source URL) and any spec-referenced literals in test names/assertions

## 4. Error-log version stamp

- [x] 4.1 Include `ApplicationInfo.Version` in each `FileErrorLog` entry alongside the timestamp; update `FileErrorLog` tests

## 5. Documentation

- [x] 5.1 Fix `docs/technical-reference.md`: EF Core 9 → 10 (stack table + prose), correct migrations path, remove the "only the one init migration" parenthetical
- [x] 5.2 Amend the pre-V1 persistence rule in `docs/technical-reference.md` to record the squash performed at 1.0-alpha and that migrations accrue again until V1
- [x] 5.3 Create `CHANGELOG.md` with the `1.0.0-alpha` section, prominently including the data-directory wipe notice and the platform-scope note (Windows x64 only; Linux planned but not yet built/tested)
- [x] 5.4 Create `docs/user-guide.md` covering all non-obvious features listed in the release-documentation spec; link it from `README.md`
- [x] 5.5 Update the README "Drop Table Source" section to show `https://www.warframe.com/droptables` as the default and mention the redirect to DE's CDN
- [x] 5.6 Create the root `NOTICE` file with third-party attributions and the Warframe Reliquary (wf.xuerian.net) inspiration credit
- [x] 5.7 Finish the LFG → Recruiting rename in main specs: rename `openspec/specs/lfg-message-constructor/` to `recruiting-message-constructor` and fix the stray "LFG template" mention in its spec body

## 6. Verification

- [x] 6.1 Full `dotnet build` + `dotnet test` green; confirm zero warnings introduced
- [ ] 6.2 Confirm the built executable's file properties show the 1.0.0-alpha product metadata, and the window title and startup log line show the version
