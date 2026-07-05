## Why

The app has been running as `1.0.0-alpha` since 2026-07-03, and three more EF Core migrations (`RemoveSourceFilePath`, `AddSourceFilesUniqueVersionIndex`, plus the alpha `InitialSchema`) have accumulated during the preview period. The pre-V1 persistence rule (`docs/technical-reference.md`) mandates a final migration squash on the verge of release and states the rule lifts at V1. This change performs that final squash, bumps the version to the stable `1.0.0`, and updates the release documentation so the stable release can be tagged and shipped.

## What Changes

- **Version bump.** `Directory.Build.props` `<Version>` changes from `1.0.0-alpha` to `1.0.0`; `app.manifest`'s `assemblyIdentity` version syncs to `1.0.0.0`. No mechanism changes — `ApplicationInfo.Version` already derives from the assembly informational version.
- **Final migration squash. BREAKING (last time, no external impact).** The three accumulated migrations (`InitialSchema`, `RemoveSourceFilePath`, `AddSourceFilesUniqueVersionIndex`) are squashed into a single fresh `InitialSchema` migration. The developer's local database carries migration-history rows that no longer match and must be wiped (`%LOCALAPPDATA%\WF Reliquary Redux\`); no public changelog notice is needed since no external users ran a preview build. The **Pre-V1 Persistence Rule** section in `docs/technical-reference.md` is removed entirely (it documented a rule that is now moot), and the handful of other doc mentions that pointed to it are reworded to drop the "pre-V1" framing while keeping their factual content. The Code Conventions bullet that permitted skipping backwards-compatibility/migration code is replaced with the opposite, permanent guidance: from `1.0.0` onward, schema and stored-format changes must migrate forward — no more wipe-and-reinitialize.
- **Release documentation.** `CHANGELOG.md` gains a `1.0.0` section (newest-first) marking the transition out of preview and carrying forward the Windows x64-only platform scope note (Linux support is still not built/tested). The data-wipe notice is **dropped** from both the `1.0.0-alpha` and `1.0.0` sections — no external users saw either build, so the notice serves no one. The `release-documentation` spec's changelog requirement is updated to drop the data-wipe clause entirely and generalize the platform-scope clause beyond `1.0.0-alpha`.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `release-documentation`: the changelog requirement drops the data-wipe clause and generalizes the platform-scope clause to any release section published while Windows-only remains true (not just `1.0.0-alpha`); the existing `1.0.0-alpha` data-wipe notice is removed from `CHANGELOG.md` and no equivalent notice is added for `1.0.0`.

## Impact

- **Build system:** `Directory.Build.props` version bump; `app.manifest` version sync.
- **Database:** `src/Infrastructure/WF.Reliquary.Infrastructure/Migrations/` regenerated as a single migration; the developer's local database must be wiped (no end-user impact).
- **Docs:** `docs/technical-reference.md` (Pre-V1 Persistence Rule section removed; migrations line, log-directory line, and recruiting-template line reworded; Code Conventions bullet replaced with permanent forward-migration guidance), `CHANGELOG.md` (data-wipe notice removed from `1.0.0-alpha`; new `1.0.0` section added with only the platform-scope note).
- **Tests:** none expected to change behavior; full suite re-run against the fresh migration to confirm no regression.
