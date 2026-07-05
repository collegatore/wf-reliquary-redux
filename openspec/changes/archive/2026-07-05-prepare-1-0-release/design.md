## Context

The app shipped its first public preview as `1.0.0-alpha` on 2026-07-03, which included a migration squash (six development migrations collapsed into a single `InitialSchema`). Since then, two more migrations landed (`RemoveSourceFilePath`, `AddSourceFilesUniqueVersionIndex`), on top of the alpha `InitialSchema` — three migrations total. The pre-V1 persistence rule requires a final squash "on the verge of release" and states the rule lifts at V1; `1.0.0` is that V1 release.

## Goals / Non-Goals

**Goals:**
- Bump the single-source version to `1.0.0`.
- Perform the final migration squash (three migrations → one `InitialSchema`), matching the same process used for the alpha squash.
- Remove the now-moot `## Pre-V1 Persistence Rule` section from `docs/technical-reference.md`, reword the other mentions that pointed to it, and replace the Code Conventions bullet with permanent forward-migration guidance.
- Drop the data-wipe notice from `CHANGELOG.md` entirely (both `1.0.0-alpha` and `1.0.0`), keeping only the platform-scope note, and update the `release-documentation` spec to match.

**Non-Goals:**
- No new features, UI changes, or settings-defaults changes (unlike the alpha prep, which also touched settings defaults and window sizing — those are already stable since alpha).
- No changes to `release-distribution` (CI/release workflows already handle arbitrary tag versions generically) or `app-version-identity` (mechanism already reads from `Directory.Build.props`; only the value changes).
- No actual forward-migrating logic is introduced yet — that only becomes relevant the next time schema or stored-format changes are made post-1.0.0.

## Decisions

- **Squash approach mirrors the alpha squash.** Record the three migrations' raw `Sql(...)` carry-over content, delete the `Migrations/` folder, scaffold a fresh `InitialSchema` via `dotnet ef migrations add`, then diff against the recorded content to confirm nothing is lost (index, unique constraint, and dropped-column behavior in particular).
- **The rule section is removed, not amended.** The alpha-era design amended the rule text in place to record each squash. That approach doesn't fit anymore: the rule states it lifts at V1, and `1.0.0` is V1. Rather than leaving a section that documents a now-inactive rule, the whole `## Pre-V1 Persistence Rule` section is deleted. Its permanent takeaway — schema/format changes migrate forward from here on — moves into the Code Conventions bullet that used to permit skipping that work.
- **No data-wipe notice, for either version.** The alpha squash's changelog notice assumed someone might have run a preview build. In practice, no external users ran `1.0.0-alpha` — it's an internal fact the developer already knows, not something anyone needs to read in a changelog. The data-wipe clause is dropped from the `release-documentation` spec requirement outright (not conditioned on squash-vs-not), and the existing `1.0.0-alpha` `CHANGELOG.md` entry is edited to remove its wipe notice retroactively. The platform-scope clause is kept and generalized since it's still true and still useful to a reader.

## Risks / Trade-offs

- [Risk] A future contributor who did happen to run a preview build could still be caught by the squash without a changelog warning → Mitigation: acceptable given this is a solo/pre-launch project with no known external installs; the developer's own machine is handled directly in tasks (§2.5).
- [Risk] Diffing three migrations' raw SQL by hand could miss something the model snapshot already expresses declaratively (columns/indexes are fine; only genuinely raw SQL needs manual carry-over) → Mitigation: full integration suite run against the fresh migration before considering the squash done.

## Migration Plan

1. Record existing migrations' raw SQL and full test baseline.
2. Regenerate `InitialSchema`, diff, and confirm parity via the integration suite.
3. Wipe local `%LOCALAPPDATA%\WF Reliquary Redux\` on the dev machine (no other machines are affected).
4. Bump version, update docs and changelog.
5. No rollback path once shipped — this is a one-way squash, same as alpha, but with no public notice needed since there are no external installs to account for.

## Open Questions

None — scope is narrow and mirrors the already-executed alpha squash process.
