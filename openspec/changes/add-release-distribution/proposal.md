# Add Release Distribution

## Why

The repo currently has no way to ship the app: no publish configuration, no CI, no release artifacts — the only run instruction is `dotnet run`. With `prepare-alpha-release` giving the app its 1.0.0-alpha identity, this change adds the pipeline that turns a version tag into downloadable Windows builds, so the preview can actually reach users.

## What Changes

- **Publish profiles.** Two `.pubxml` profiles under `src/Presentation/WF.Reliquary.Presentation/Properties/PublishProfiles/`: `win-x64-self-contained` (single-file, no .NET install required) and `win-x64-framework-dependent` (small download, requires the .NET 10 desktop runtime). No trimming in either.
- **CI workflow.** `.github/workflows/ci.yml` builds the solution and runs the full test suite on every push and pull request targeting `develop` and `main` — the repo's first CI.
- **Release workflow.** `.github/workflows/release.yml` triggers on `v*` tags: verifies the tag matches the authoritative version in `Directory.Build.props` (fails on mismatch), builds and tests, publishes both profiles, zips them, and creates a GitHub Release with both zips attached and the matching `CHANGELOG.md` section as the release body. Uses the built-in `GITHUB_TOKEN` with `permissions: contents: write`; no manual GitHub configuration is needed beyond pushing the workflow files.
- **Download documentation.** README gains a download/install section for end users: which artifact to pick (self-contained vs framework-dependent), the expected Windows SmartScreen warning for the unsigned executable and how to proceed past it, and an explicit note that Linux support is planned but not yet built or tested — Windows x64 is the only supported platform at this release.
- **No code signing** at alpha (documented, not implemented).
- **No Linux artifacts** — publishing is win-x64 only until Linux support is actually exercised.

## Capabilities

### New Capabilities

- `release-distribution`: publish profiles, CI build/test workflow, tag-triggered release workflow with version verification, and end-user download documentation.

### Modified Capabilities

*(None. `release-documentation` — introduced by the in-flight `prepare-alpha-release` change — gets its Windows-only release-notes wording amended directly in that change's delta spec rather than via a second delta here, since it has not been synced to main specs yet.)*

## Impact

- **New files:** `Properties/PublishProfiles/win-x64-self-contained.pubxml`, `Properties/PublishProfiles/win-x64-framework-dependent.pubxml`, `.github/workflows/ci.yml`, `.github/workflows/release.yml`.
- **Docs:** README download/install section (SmartScreen + Windows-only notes); `docs/technical-reference.md` gains the publish commands.
- **Dependencies:** builds on `prepare-alpha-release` (`Directory.Build.props` version, `CHANGELOG.md`) — that change must be applied first.
- **Systems:** GitHub Actions (free for this public repo); GitHub Releases as the distribution channel.
