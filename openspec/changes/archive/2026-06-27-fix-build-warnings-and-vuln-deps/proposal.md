## Why

The build produces 14 warnings — 9 flagging a **high-severity CVE** in a transitive
SQLite dependency and 5 compiler nullable warnings — that add noise, mask real issues,
and leave the shipped binary linked against a known-vulnerable native library.
Addressing them now keeps the baseline clean before the project adds more features.

## What Changes

- **Upgrade `Microsoft.EntityFrameworkCore.Sqlite` from 9.0.6 to 10.0.9** — aligns
  EF Core with the project's `net10.0` target framework.
- **Override `SQLitePCLRaw.bundle_e_sqlite3` to 3.0.3** — the entire 2.1.x line is
  covered by GHSA-2m69-gcr7-jv3q; the 3.x generation bundles SQLite 3.50.4 via
  `SourceGear.sqlite3` and is not affected. EF Core 10 has not yet adopted 3.x
  officially, but the override is runtime-compatible (verified by the full test suite).
- **Fix CS8601** in `LocalDropTableSourceStore.cs:76` — null value assigned to a
  non-nullable entity property (`SourceUrl`).
- **Fix CS8602 ×5** in `MainWindowViewModel.cs:58–62` — five command fields
  dereferenced inside a `BusyScope` lambda before they are assigned.

## Capabilities

### New Capabilities

_(none — this change contains no new user-visible capabilities)_

### Modified Capabilities

_(none — no spec-level behavioral requirements change)_

## Impact

- **`src/Infrastructure/WF.Reliquary.Infrastructure/WF.Reliquary.Infrastructure.csproj`** —
  bumps `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design`
  from 9.0.6 to 10.0.9; adds direct `PackageReference` to
  `SQLitePCLRaw.bundle_e_sqlite3 3.0.3` to override the vulnerable 2.1.x transitive.
- **`src/Infrastructure/.../DropTables/LocalDropTableSourceStore.cs`** — fix null
  assignment at line 76 (`SourceUrl` property).
- **`src/Presentation/.../MainWindowViewModel.cs`** — fix five null-dereference
  warnings (lines 58–62) in the `BusyScope` constructor lambda.
- No database changes, no API changes, no user-visible behavior changes.
- Build should produce **0 warnings** after this change.
