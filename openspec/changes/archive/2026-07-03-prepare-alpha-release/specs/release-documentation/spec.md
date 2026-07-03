## ADDED Requirements

### Requirement: Changelog with Release Notes

The repository SHALL contain a `CHANGELOG.md` at the root with a section per released version, newest first. The `1.0.0-alpha` section SHALL prominently state that pre-release users must delete the application data directory (`%LOCALAPPDATA%\WF Reliquary Redux\`) before first launch, because the squashed database migration does not upgrade earlier databases. The `1.0.0-alpha` section SHALL also state that Windows x64 is the only supported platform at this release, and that Linux support is planned but has not yet been built or tested.

#### Scenario: Data-wipe notice present for 1.0.0-alpha

- **WHEN** a reader opens `CHANGELOG.md`
- **THEN** the `1.0.0-alpha` section instructs deleting the application data directory before first launch

#### Scenario: Platform scope stated for 1.0.0-alpha

- **WHEN** a reader opens `CHANGELOG.md`
- **THEN** the `1.0.0-alpha` section states that only Windows x64 is supported and Linux support is planned but not yet delivered

### Requirement: User Guide for Non-Obvious Features

The repository SHALL contain a User Guide (`docs/user-guide.md`), linked from the README, that briefly describes each non-obvious feature of the application — at minimum: the separate download vs import steps for drop tables, wishlist entries surviving drop-table re-imports (ghost entries), player profiles and per-profile wishlists, the Recruiting Chat message template and its placeholders, the relic view modes (Verbatim / Compact / Table), owned/vaulted prime-gear separation and group operations, the Appearance window controls (font size, relic card width, layout orientation), and the locations of the data directory and log file.

#### Scenario: User Guide reachable from README

- **WHEN** a reader opens the README
- **THEN** a link leads to the User Guide

#### Scenario: Non-obvious feature is explained

- **WHEN** a user consults the User Guide about a listed non-obvious feature (e.g. why a wishlisted part shows as missing after a re-import)
- **THEN** the guide contains a brief explanation of that behavior

### Requirement: Third-Party Attribution Notice

The repository SHALL contain a `NOTICE` file at the root attributing bundled third-party dependencies (including the Caly TreeDataGrid fork, Avalonia, CommunityToolkit.Mvvm, AsyncAwaitBestPractices, SQLite/SQLitePCLRaw, and EF Core) with their licenses, and crediting the original Warframe Reliquary website (wf.xuerian.net) as the project's inspiration.

#### Scenario: Dependency attribution present

- **WHEN** a reader opens the `NOTICE` file
- **THEN** each bundled third-party dependency is listed with its license, and the original Warframe Reliquary site is credited
