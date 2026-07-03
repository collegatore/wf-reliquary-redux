# Design — Add Release Distribution

## Context

`prepare-alpha-release` establishes version identity (`Directory.Build.props`, 1.0.0-alpha) and `CHANGELOG.md`; this change adds the delivery pipeline on top. The repo is public on GitHub (`collegatore/wf-reliquary-redux`), so GitHub-hosted Actions runners are free with no minute caps. There is currently no `.github/` directory at all. The app has only ever been built and run on Windows; Linux is an aspirational target that has never been exercised.

## Goals / Non-Goals

**Goals:**
- A `v*` tag push is the *entire* release procedure — everything else (build, test, publish, zip, GitHub Release with notes) is automatic.
- Two Windows artifacts per release serving both audiences: zero-prerequisite (self-contained) and small-download (framework-dependent).
- Every push/PR to `develop`/`main` gets an automated build + full test run.
- End users can find, download, and get past SmartScreen without asking questions.

**Non-Goals:**
- Linux artifacts (Linux has never been built/tested; publishing untested binaries would be worse than none).
- Code signing, auto-update, package managers (winget/Flatpak/etc.), installer (MSI/MSIX) — all post-alpha candidates.
- Trimming/AOT — Avalonia + EF Core reflection make it a debugging tax with no alpha payoff.

## Decisions

### D1: Two artifacts, both win-x64, defined as `.pubxml` publish profiles

`Properties/PublishProfiles/win-x64-self-contained.pubxml`: `RuntimeIdentifier=win-x64`, `SelfContained=true`, `PublishSingleFile=true`, `IncludeNativeLibrariesForSelfExtract=true` (SQLitePCLRaw and Avalonia native libs must ride inside the single file), `Configuration=Release`. `win-x64-framework-dependent.pubxml`: same minus self-contained/single-file (framework-dependent folder output).

- *Why pubxml over raw CLI flags or csproj properties?* The recipe lives in the repo, works identically for a local `dotnet publish -p:PublishProfile=<name>` and for CI, and is discoverable by a fork maintainer. Csproj-global publish properties would leak into normal `dotnet build`.
- Zip naming: `WF.Reliquary.Redux-<version>-win-x64-self-contained.zip` / `...-win-x64.zip` — version and flavor readable at a glance in the Releases page.

### D2: Two workflows, not one

- `ci.yml`: `on: push`/`pull_request` for `develop` and `main`; single `windows-latest` job — checkout, setup .NET 10, `dotnet build`, `dotnet test`. Windows runner because the product targets Windows and integration tests exercise Windows-specific branches (`FileLauncherTests` skips its Linux branch there).
- `release.yml`: `on: push: tags: ['v*']`; verifies tag ↔ props version (D3), then build/test/publish both profiles, zip, create the GitHub Release.
- *Why not fold release into ci.yml with conditions?* Separate files keep each trigger's permissions minimal — `ci.yml` needs only default read; `release.yml` declares `permissions: contents: write` for creating the release.
- Committing the yml files is sufficient: GitHub registers workflows automatically once they exist on the relevant ref (for tag triggers, the workflow must exist at the tagged commit). The auto-provided `GITHUB_TOKEN` covers release creation; no PATs, secrets, or repository-settings changes required.

### D3: `Directory.Build.props` is authoritative; the tag is verified against it

The release workflow extracts `<Version>` from `Directory.Build.props` and compares it to the tag (stripped of the leading `v`). Mismatch → the job fails before building anything.

- *Why not derive the version from the tag (`-p:Version=...`)?* Two sources again; a typoed tag would silently ship a build whose internal version (title bar, logs) disagrees with the release name. Failing loudly is the point.
- Release body: extract the tag's version section from `CHANGELOG.md` (simple awk/PowerShell range match on the version heading). If the section is missing, fail — a release without notes is a process error, and the alpha notes carry the mandatory data-wipe warning.

### D4: Unsigned executable, documented SmartScreen path

No code-signing certificate at alpha (cost, and reputation builds only over time anyway). Instead the README download section and the User Guide state plainly: Windows will show "Windows protected your PC" — click *More info* → *Run anyway*; browsers may also warn on the download. Honest and standard for OSS pre-releases.

### D5: Windows-only release scope, stated explicitly

Linux remains in the project goals (tech reference targets it; `ShellFileLauncher` has an `xdg-open` branch) but has never been built or run. Therefore: no `linux-x64` publish profile, no Linux job, and both the README platform note and the `CHANGELOG.md` 1.0.0-alpha section state that Linux support is planned but not delivered at this release. The `prepare-alpha-release` change's release-documentation delta is amended accordingly (that change owns CHANGELOG content and is not yet synced/archived, so the amendment happens there, not as a delta here).

## Risks / Trade-offs

- [Single-file extraction quirks with native libs (e_sqlite3, Avalonia/SkiaSharp)] → `IncludeNativeLibrariesForSelfExtract=true`; release workflow can't smoke-test the GUI headlessly, so first releases warrant a manual download-and-launch check of both artifacts.
- [Framework-dependent users lack the .NET 10 Desktop Runtime] → README links the official runtime download next to that artifact and says which runtime ("The .NET Desktop Runtime 10.x, x64").
- [CHANGELOG section extraction is format-sensitive] → keep the heading format fixed (Keep-a-Changelog style, already specified in prepare-alpha-release); the workflow fails visibly if extraction comes up empty.
- [Windows runner CI is slower than Linux] → acceptable; suite is ~15 s locally, checkout+setup dominates either way.
- [`v*` tag pushed on a commit lacking the workflows] → only affects tags older than this change; document that releases are tagged on `main` after merge.

## Open Questions

- None. Installer/package-manager distribution and Linux artifacts are explicitly deferred until after Linux support is actually exercised.
