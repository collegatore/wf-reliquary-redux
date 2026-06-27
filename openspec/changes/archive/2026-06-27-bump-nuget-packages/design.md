## Context

Eight NuGet packages across three projects (`Presentation`, `UnitTests`, `IntegrationTests`) have newer versions available. The security scan was clean. No architectural change is involved — this is a pure version-number maintenance update.

## Goals / Non-Goals

**Goals:**
- Bring all packages to their latest stable release
- Ensure the build and test suite remain green after the bump

**Non-Goals:**
- Changing any application code or behavior
- Updating transitive/implicit SDK packages (only explicit `<PackageReference>` entries)

## Decisions

**Bump all at once** rather than one at a time. All upgrades were individually assessed as safe, so batching them reduces the number of restore/build cycles and keeps the git history clean with a single commit.

**AsyncAwaitBestPractices 9→10**: Despite the major version label, the sole change is the addition of a `net10.0` TFM. No API surface changed. The project already targets `net10.0`, so this is safe and slightly beneficial (resolves to a framework-matched build instead of a fallback TFM).

**NUnit 4.6 delegate change**: The `TestDelegate`/`ActualValueDelegate` → `Action`/`Func` conversion in 4.6.0 is only a compile-time break when those types are used with explicit variable declarations on .NET < 9. This project is on `net10.0` and uses neither type explicitly.

## Risks / Trade-offs

- [Avalonia 12.0.5 regression] → Mitigation: run the full integration test suite and smoke-test the UI after the bump. All Avalonia 12.0.x changes are fixes, not features, so regression surface is low.
- [NUnit 4.6 partial break] → Mitigation: confirmed via codebase grep that `TestDelegate`/`ActualValueDelegate` are not referenced; no action needed.
