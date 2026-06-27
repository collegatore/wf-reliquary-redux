## Context

The build reports 14 warnings across two categories:

**NU1903 (×9) — high-severity CVE in transitive SQLite dependency**
`Microsoft.EntityFrameworkCore.Sqlite` 9.0.6 transitively pulls
`SQLitePCLRaw.bundle_e_sqlite3` 2.1.10, which in turn pulls
`SQLitePCLRaw.lib.e_sqlite3` 2.1.10 — the native SQLite binary.
NuGet published advisory GHSA-2m69-gcr7-jv3q against the entire 2.1.x line.

Before the EF Core pivot the project used `Microsoft.Data.Sqlite` 10.0.0 directly,
which explicitly pins `SQLitePCLRaw.bundle_e_sqlite3` to 2.1.11. The vulnerability
was introduced when the pivot replaced that dependency with EF Core 9.x, whose
`SQLitePCLRaw.bundle_e_sqlite3` minimum is 2.1.10.

The warning fires in every project whose transitive closure includes the EF Core
SQLite package (Infrastructure, Composition, Presentation, IntegrationTests).

**CS8601 (×1) — null assigned to non-nullable entity property**
`LocalDropTableSourceStore.cs:76` assigns `content.SourceUrl?.ToString()` —
type `string?` — to `SourceUrl` on `SourceFileEntity`, which the compiler treats
as `string` (non-nullable).

**CS8602 (×5) — commands dereferenced before assignment**
`MainWindowViewModel` initializes `BusyScope` at line 55 with a callback lambda
that calls `.NotifyCanExecuteChanged()` on five command fields. Those fields are
assigned at lines 65–69, after the `BusyScope` constructor call returns. The
compiler correctly flags the lambda body as a potential null dereference because
it can see the fields are uninitialized at the point of lambda creation.

## Goals / Non-Goals

**Goals:**
- Eliminate all 14 build warnings so the output is clean.
- Replace the vulnerable native SQLite binary with a patched version.
- Fix both nullable compiler warnings without suppression attributes.

**Non-Goals:**
- Auditing or updating packages beyond what is needed to clear the CVE.
- Changing any user-visible behaviour.

## Decisions

### 1 — CVE fix: upgrade EF Core to 10.0.9 + override bundle to SQLitePCLRaw 3.0.3

**What we planned:** Upgrade EF Core 9.0.6 → 10.0.9. EF Core 10.0.0 pins
`SQLitePCLRaw.bundle_e_sqlite3` to 2.1.11, matching what `Microsoft.Data.Sqlite`
10.0.0 (the pre-EF baseline) already used.

**What we found during implementation:** GHSA-2m69-gcr7-jv3q covers the entire
2.1.x series, including 2.1.11. Upgrading EF Core alone therefore does not clear
the advisory — NuGet still reports NU1903 against 2.1.11. EF Core 10.0.9 has not
yet adopted the SQLitePCLRaw 3.x generation.

**SQLitePCLRaw versioning history (discovered during implementation):**

| Generation | Bundle package | Native library package | SQLite version |
|---|---|---|---|
| 2.1.x | `SQLitePCLRaw.bundle_e_sqlite3` | `SQLitePCLRaw.lib.e_sqlite3` | up to 3.46.x |
| 3.x | `SQLitePCLRaw.bundle_e_sqlite3` | `SourceGear.sqlite3` | 3.50.x+ |

The 3.x generation switched the native library to `SourceGear.sqlite3` and
dropped `SQLitePCLRaw.lib.e_sqlite3` entirely. `bundle_e_sqlite3 3.0.3` pulls in
`SourceGear.sqlite3 3.50.4.5` (SQLite 3.50.4), which is not covered by the advisory.

**Actual fix:** Add a direct `PackageReference` to `SQLitePCLRaw.bundle_e_sqlite3
3.0.3` in `WF.Reliquary.Infrastructure.csproj`. NuGet's minimum-version-wins rule
resolves to 3.0.3 over EF Core's 2.1.11 minimum. Although EF Core 10.0.9 does not
explicitly declare support for SQLitePCLRaw 3.x, the 3.x bundle is runtime-compatible:
the full test suite (238 tests, including all integration tests hitting a real SQLite
database) passes without modification.

**Alternative considered:** Suppress the warning with `<NoWarn>NU1903</NoWarn>`.
Rejected — suppression hides the vulnerability without fixing it.

**Alternative considered:** Wait for EF Core to adopt SQLitePCLRaw 3.x officially.
EF Core 11 (currently in preview) is likely to do this; not actionable now.

### 2 — CS8601: null-coalesce to `string.Empty`

Change `content.SourceUrl?.ToString()` to `content.SourceUrl?.ToString() ?? string.Empty`.
`SourceUrl` in `SourceFileEntity` is `string` (non-nullable); a stored URL is never
meaningfully absent after a successful fetch, so an empty string is the correct
sentinel for "no URL" rather than making the column nullable.

**Alternative considered:** Change `SourceFileEntity.SourceUrl` to `string?`.
Rejected — would cascade nullable annotations through query code and LINQ projections
for a column that is always populated in practice.

### 3 — CS8602: swap initialization order in `MainWindowViewModel` constructor

Move the five command field assignments to run **before** the `BusyScope`
construction. The BusyScope callback lambda then captures fields that are already
initialized, satisfying the compiler.

This is safe because the `canExecute` predicates passed to `AsyncRelayCommand`
are lazy delegates — they are never invoked during the command constructor. By
the time any UI binding evaluates `CanExecute`, the full constructor has completed
and `busy` is non-null. The behavioral semantics are unchanged.

**Alternative considered:** Null-conditional calls in the lambda
(`DownloadCommand?.NotifyCanExecuteChanged()`). Rejected — silently swallows a
real bug if a command is ever missing; reorder is cleaner and expresses the intent.

**Alternative considered:** Null-forgiving operator (`DownloadCommand!`). Rejected
— hides the issue from the compiler without fixing the root cause.

## Risks / Trade-offs

- **SQLitePCLRaw 3.x / EF Core 10 version mismatch** — EF Core 10.0.9 requests
  bundle 2.1.11 but we override to 3.0.3. The 3.x bundle uses a completely different
  native library (`SourceGear.sqlite3` vs `SQLitePCLRaw.lib.e_sqlite3`), so this is
  an unsupported configuration from EF Core's perspective. In practice it is verified
  by the full integration test suite (203 tests against a real SQLite database).
  → Mitigation: when EF Core officially adopts SQLitePCLRaw 3.x, remove the explicit
  override — it will become redundant.

- **EF Core 10 breaking changes** — EF Core major versions can include breaking
  changes. The project uses a narrow subset (migrations, basic CRUD, embedded SQL
  queries) and all 238 tests pass after the upgrade.

- **Constructor reorder** — swapping initialization order changes no observable
  behaviour, but a future developer adding a canExecute predicate that *does* read
  from an early-initialized field must be aware of the ordering invariant.
  → Mitigation: no comment needed; the order is self-evident once commands precede
  BusyScope.
