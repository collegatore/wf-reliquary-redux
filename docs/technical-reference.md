# Technical Reference

Single source of truth for stack decisions, project layout, commands, architecture, database design, testing, and code conventions. Referenced by both [README.md](../README.md) and [CLAUDE.md](../CLAUDE.md).

## Tech Stack

| Concern | Choice |
|---|---|
| Language / Runtime | C# 13, .NET 10 SDK |
| UI | Avalonia 12 (AXAML + MVVM) |
| Database | SQLite via EF Core 9 |
| HTML parsing | HtmlAgilityPack |
| Testing | NUnit 4.5 + Moq |
| IDE | Visual Studio Code |
| Target OS | Windows and Linux |

## Commands

```bash
dotnet restore
dotnet build
dotnet test
dotnet test --logger "console;verbosity=detailed"          # verbose output
dotnet test --filter "FullyQualifiedName~<TestClassName>"  # single test class
dotnet run --project src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj
```

VS Code workspace tasks (`Ctrl+Shift+B`): `dotnet: restore`, `dotnet: build`, `dotnet: test`, `dotnet: run presentation`. Launch configurations: `.NET: Debug Presentation` (with debugger), `.NET: Launch Presentation` (without).

## Project Layout

```
WF.Reliquary.Redux.sln
src/
  Application/   — domain models, port interfaces, workflow orchestration
  Infrastructure/ — EF Core/SQLite, HttpClient, HTML parser, file storage
  Composition/   — ReliquaryServiceFactory: manual wiring of app ↔ infra
  Presentation/  — Avalonia ViewModels + AXAML views
tests/
  Unit/          — NUnit, Application layer only, Moq for all ports
  Integration/   — NUnit, real SQLite stack, ViewModel wiring, boundary checks
```

## Architecture

Strict four-layer architecture. Dependency direction is enforced by `ProjectReferenceBoundaryTests`:

```
Presentation  →  Application  ←  Infrastructure
     ↓                ↑               ↑
 Composition  ────────┴───────────────┘
```

- **Application** — zero external dependencies; defines all port interfaces (`IWishlistStore`, `IDropTableParser`, etc.) and orchestrates workflows.
- **Infrastructure** — implements every Application port; owns EF Core DbContext, HTTP client, HTML parser, and local file I/O.
- **Composition** — `ReliquaryServiceFactory` is the only place that references both Application and Infrastructure; no DI container.
- **Presentation** — calls Composition to obtain services; never references Infrastructure directly.

**Extending the app:** add an interface in Application → implement it in Infrastructure → wire it in `ReliquaryServiceFactory` → consume it via the interface in Presentation.

## Database

**Location:** `%LOCALAPPDATA%\WF Reliquary Redux\DropTables\drop-table-import.sqlite`

**Migrations:** `src/Infrastructure/Storage/Migrations/`; see the pre-V1 persistence rule below.

**Queries:** complex joins (prime gear, wishlist relics, farming missions) use embedded `.sql` files in `src/Infrastructure/Storage/Queries/`, loaded by `SqlQueryLoader`. Prefer embedded SQL over LINQ when it is stylistically or functionally clearer.

**Soft references:** `wishlist_items` stores `part_normalized_name` as a plain string with no FK to `reward_items`, so wishlist entries survive drop-table re-imports.

**Profiles & wishlist scoping:** `profiles` is a container one level above `wishlists` so players can track multiple accounts separately. Each `wishlists` row carries a `profile_id` (FK → `profiles`, `ON DELETE CASCADE`), so deleting a profile removes its wishlists and their items. Each profile remembers its own last-active wishlist via `profiles.active_wishlist_id` (FK → `wishlists`, `ON DELETE SET NULL`). The singleton `user_preferences` row holds `last_profile_id` (the active profile). `IWishlistStore` keeps no-`profileId` method signatures but internally scopes every query to the active profile resolved from `last_profile_id` (falling back to the earliest profile), so the Prime Gear / Relics / Farming views need no profile awareness. `IProfileStore` owns profile CRUD; its control surface lives in the **Settings** tab, not the Prime Gear tab. `DefaultProfileSeeder` seeds the "Default" profile and is shared by both stores so wishlist seeding is never order-dependent on the profile store.

## EF Core Data Layer

### Why EF Core

The project started with raw ADO.NET (string DDL, manual `IDataReader` loops). Once the schema grew to 11 tables with FKs, CHECK constraints, and several multi-join queries, the cost of maintaining boilerplate (`SqliteConnection`, `SqliteCommand`, schema strings, PRAGMA introspection) outweighed the simplicity benefit. EF Core was adopted to:

- own DDL via code-first migrations (single source of truth for schema)
- eliminate redundant connection/command helpers spread across store classes
- enable set-based bulk deletes without loading entities (`ExecuteDeleteAsync`)

**Dapper note:** given that the four most important queries are kept as raw SQL anyway, Dapper would likely be a better fit for this workload — thin mapping over hand-written SQL with none of EF's change-tracking overhead. EF Core was chosen here deliberately as a learning exercise to observe how code-first migrations and the EF workflow handle themselves in a real project.

### What stays as embedded SQL

Queries that are stylistically or functionally clearer as SQL than LINQ are kept as `.sql` files in `Infrastructure/Storage/Queries/` rather than expressed in LINQ. Typical candidates are CTEs, correlated UPDATEs, and joins on computed expressions — for example `GetWishlistRelics.sql` uses a CTE with a 4-way JOIN and a LEFT JOIN that would be verbose and hard to read in LINQ.

Loaded at construction time via `SqlQueryLoader` (reads embedded resource stream). Mapped with `Database.SqlQueryRaw<T>` using named `@param` parameters and explicit PascalCase column aliases (e.g. `ri.name AS ItemName`) because EF does not auto-map snake_case columns to PascalCase properties on raw-query result types.

Raw-query DTO properties for SQLite INTEGER 0/1 columns use `int`, not `bool`, to avoid type-mapping uncertainty (`row.IsVaulted != 0` at the call site).

### Bulk import pattern

The snapshot import (replace everything on each drop-table import) does not fit EF change-tracking well. Approach:

1. **Delete** in FK-dependency order with `ExecuteDeleteAsync` — no entity loading.
2. **Insert in tiers** with a `SaveChangesAsync` between each tier so EF populates auto-generated PKs before dependent rows reference them.
3. **Post-insert computed steps** run as `ExecuteSqlRawAsync` (vault status CTE insert + correlated relic vault update).
4. All three phases execute inside a single explicit transaction.

### Soft reference

`wishlist_items.part_normalized_name` is a plain string column with **no FK** to `reward_items.normalized_name`. This is intentional: drop-table re-imports truncate `reward_items`, and wishlist entries must survive that cycle. Ghost detection (wishlisted part absent from current import) depends on the constraint being absent.

### Connection string

`SqliteConnectionStringBuilder.ForeignKeys = true` is set on every context via `ReliquaryDbContext.BuildOptions`. All FK constraints are enforced at the SQLite level, not just the EF model.

### Design-time migrations

`ReliquaryDesignTimeDbContextFactory` implements `IDesignTimeDbContextFactory<ReliquaryDbContext>` so `dotnet ef migrations add` works without a startup project. The tool is pinned as a local tool in `.config/dotnet-tools.json`; run `dotnet tool restore` before first use.

## Pre-V1 Persistence Rule

Applies to **all persistent state**: database data, settings files, config files, and any other stored format.

- **Non-schema formats evolve in place.** Do not write migration, upgrade, or backwards-compatibility code for settings, config, data files, or other stored formats — change the format directly.
- **EF Core schema migrations are allowed.** Schema changes use the normal EF Core migration workflow; incremental migrations accrue in the migrations folder throughout development. (At present there happens to be only the one init migration because the schema has changed only once.)
- **Notify on re-initialization.** Whenever a change requires the developer to wipe and re-initialize persisted state (database, settings, etc.) — typically a non-schema format change, since those are not migrated — say so explicitly at the point the change is made.
- **Squash to a single init at V1.** Squashing all accumulated migrations into one clean init migration is a **one-time** mandatory step of V1 release preparation, performed on the verge of release — *not* an ongoing pre-V1 activity.

**This rule lifts at V1.** After V1, migrations are kept incrementally and stored formats migrate forward rather than re-initializing.

## Testing

- **Unit tests** — cover Application-layer logic only. One test class per service class. All ports are mocked with Moq.
- **Integration tests** — exercise the real SQLite stack end-to-end, ViewModel wiring, and project structure. `ReadmeGuidanceTests` and `VsCodeWorkflowTests` enforce documentation and build-tooling compliance; keep README and VS Code tasks consistent with them.

## LFG Message Constructor

Each relic card on the Relics and Missions tab has a `⋮` overflow menu (same pattern as prime gear cards). "Compose LFG message…" opens a modal popup with per-option-group selectors, a live preview, and a copy-to-clipboard button, generating Warframe recruiting-chat messages like `LF [Axi A1 Relic] rad`.

**Pattern engine** (`Application/Lfg/`): `LfgMessageTemplate` holds the pattern string and custom option groups (`LfgOptionGroup` → `LfgOption` with Descriptor/Value). `LfgPatternParser.Validate` runs a forward-scan checking for unclosed/nested/empty placeholders and requires `{relic_name}`. `LfgPatternParser.Reconcile` implements Parse-as-reconcile (keep draft group → restore from saved group → empty group). `LfgMessageBuilder` substitutes via `Regex.Replace`. System placeholders: `{relic_name}` (resolves to full display name, e.g. `Axi A1 Relic`); `{relic_reward}` is reserved but has no UI yet.

**Settings**: a sub-section in the Settings tab edits the pattern and option groups with Parse / Save / Restore defaults. The template is held as a draft; Save persists; Restore defaults resets to `LfgMessageTemplate.Default` without persisting.

**Persistence**: the template is stored in `user-settings.json` (nested `lfgTemplate` object). **Pre-V1 note**: existing `user-settings.json` files have no `lfgTemplate` field and will load `LfgMessageTemplate.Default` until the user opens Settings and saves. No wipe or migration needed.

## UI Design

- **Card display** — the rationale behind the relic and farming-mission cards, the three relic view modes (Verbatim / Compact / Table), the Compact word-clipping rule, the Table mode TreeDataGrid decision, tier accent bars, the light-theme palette, and font-scaled card widths, is documented in [card-display-design.md](card-display-design.md).

## UI Concurrency

The Presentation layer must never block the UI thread. The contract:

- **SQLite is sync-over-async.** `Microsoft.Data.Sqlite` runs its `…Async` methods *synchronously*, so awaiting EF Core calls does **not** yield the UI thread. Offloading is therefore mandatory and lives in **Infrastructure**, not the consumer.
- **The data-context seam.** `ReliquaryDbExecutor` (`Infrastructure/Storage/`) owns each `ReliquaryDbContext` lifetime and runs the whole unit of work inside one `Task.Run`. Every `Sqlite*` store/reader routes its method body through `executor.RunAsync(...)` — one offload per logical operation, never per EF call, never per consumer method. The context stays confined to that single thread. Infrastructure delegates may use `ConfigureAwait(false)`.
- **Synchronous CPU adapters offload themselves.** `IDropTableParser.ParseAsync` wraps the regex-heavy parse in `Task.Run` inside `HtmlDropTableParser`.
- **ViewModels never call `Task.Run` and never use `ConfigureAwait(false)`** — work is already offloaded below them, and continuations must resume on the UI thread to mutate bound `ObservableCollection`s. (Enforced by `UiConcurrencyContractTests`.)
- **MVVM plumbing** uses `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `AsyncRelayCommand` with built-in `IsRunning`/reentrancy) and `AsyncAwaitBestPractices` (`SafeFireAndForget` for constructor loads and property-setter persistence).
- **Failures are observed.** No discarded async tasks: fire-and-forget goes through `SafeFireAndForget`; commands catch and report. All failures route to `IErrorReporter` (`AppErrors.Reporter`). The real implementation (`ErrorReporter`) logs to file, sets the footer error text, and adds a dismissible `ErrorToastCard` to the window's overlay panel.
- **Busy + reentrancy.** `BusyScope` gives commands a busy state and a reentrancy guard. Content views (Prime Gear / Relics / Missions) expose `IsBusy` and show an in-content "Updating…" indicator; global Drop Tables source operations use the footer spinner.
- **Reloads are latest-wins.** `ReloadGate` cancels a superseded reload's `CancellationToken`; reloads check the token after their awaited fetches and only then mutate bound collections, so a cancelled reload never leaves a half-rebuilt view.
- **Blocking-on-async is confined to the startup/shutdown boundary** — only `MainWindow`'s constructor and `OnWindowClosing` may `Task.Run(...).GetAwaiter().GetResult()` (no message loop to starve there). No interactive code path blocks. (Enforced by `UiConcurrencyContractTests`.)

## Error Reporting

All runtime failures (caught command exceptions, `SafeFireAndForget` callbacks, failed background loads) are routed through `IErrorReporter` → `AppErrors.Reporter`.

### What the reporter does

`ErrorReporter` (Presentation) does three things on each `Report(userMessage, exception)` call:

1. **Writes to the log file** via `IErrorLog.AppendAsync` — timestamped entry with the user message and exception detail. Fire-and-forget; a log failure does not suppress the other actions.
2. **Sets the footer error text** via an injected `Action<string>` delegate — the text `"{userMessage}. Refer to logs for the error details."` appears in the footer in OrangeRed. Stays until replaced by the next `Report()` call.
3. **Adds an `ErrorToastCard`** to the `StackPanel` overlay at the bottom-right of `MainWindow`. The card shows the message, a "Copy log path" button (puts the absolute log file path in the clipboard), and an "Open log" button (`IFileLauncher`). No auto-expiry; the user dismisses via the ✕ button.

All UI mutations are posted to `Dispatcher.UIThread` so `Report()` is safe to call from any thread.

### Ports and adapters

| Port (Application) | Adapter (Infrastructure) | Purpose |
|---|---|---|
| `IErrorLog` | `FileErrorLog` | Appends timestamped entries to `app.log`; serializes concurrent writes with `SemaphoreSlim` |
| `IFileLauncher` | `ShellFileLauncher` | Opens a file in the OS default app; Windows → `UseShellExecute=true`; Linux → `xdg-open` |

### Log file location

`%LOCALAPPDATA%\WF Reliquary Redux\logs\app.log`

The directory is created on first write. Pre-V1 persistence rule: no migration — safe to delete.

### Settings / Maintenance section

**Settings → Maintenance → "Open log file"** — always enabled, calls `IFileLauncher.Open(IErrorLog.LogFilePath)`. Launch failures are silently swallowed; "Copy log path" in any active toast is the always-available fallback.

### Wiring

`ErrorReporter` is constructed in `MainWindow` (production constructor) after `InitializeComponent()` so the `ToastPanel` named control is available. `AppErrors.Reporter` (a static holder in Presentation, initialized to `NoOpErrorReporter.Instance`) is replaced with the real `ErrorReporter` before any ViewModel is created.

`ReliquaryServiceFactory.CreateErrorLog()` and `CreateFileLauncher()` are the Composition-layer factory methods; Presentation calls them via Composition, never Infrastructure directly.

## Code Conventions

- All I/O methods are `async` and accept `CancellationToken`.
- Nullable reference types are enabled project-wide; do not use `!` without a comment explaining why the null is impossible.
- Immutable domain models use `record` types (`PrimeGearEntry`, `WishlistEntry`, etc.).
- Pre-V1: no backwards-compatibility shims, data-migration helpers, or feature flags.
