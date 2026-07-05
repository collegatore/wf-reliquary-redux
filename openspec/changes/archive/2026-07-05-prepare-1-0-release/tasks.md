## 1. Version bump

- [x] 1.1 Change `<Version>` in `Directory.Build.props` from `1.0.0-alpha` to `1.0.0`
- [x] 1.2 Sync `app.manifest`'s `assemblyIdentity` version to `1.0.0.0` (unchanged numeric value, confirm it already matches)

## 2. Migration squash

- [x] 2.1 Record the raw `Sql(...)` statements from the three existing migrations (`InitialSchema`, `RemoveSourceFilePath`, `AddSourceFilesUniqueVersionIndex`) for the carry-over diff
- [x] 2.2 Delete `src/Infrastructure/WF.Reliquary.Infrastructure/Migrations/` and scaffold a fresh `InitialSchema` migration with `dotnet ef migrations add`
- [x] 2.3 Carry any raw SQL the model snapshot cannot express into the new `InitialSchema.Up()`; diff against 2.1 to confirm nothing was lost (dropped `source_file_path` column stays dropped, `AddSourceFilesUniqueVersionIndex` unique index is present)
- [x] 2.4 Run the full integration suite against a fresh database; verify the unique-index and dropped-column behavior
- [x] 2.5 Delete the local `%LOCALAPPDATA%\WF Reliquary Redux\` data directory (developer machine re-initialization only — no public notice needed)

## 3. Documentation

- [x] 3.1 Remove the `## Pre-V1 Persistence Rule` section from `docs/technical-reference.md` entirely (it documents a rule that is now moot at `1.0.0`)
- [x] 3.2 Reword the "Migrations" line in the Database section to drop "see the pre-V1 persistence rule below" (state plainly that schema changes use the standard EF Core migration workflow with incremental migrations going forward)
- [x] 3.3 Reword the Recruiting Chat Message Constructor persistence note to drop the "**Pre-V1 note**" framing while keeping the factual behavior (missing `recruitingMessageTemplate` field loads the default until Settings is saved; no wipe or migration needed)
- [x] 3.4 Reword the log-directory line to drop "Pre-V1 persistence rule:" while keeping the safe-to-delete fact
- [x] 3.5 Replace the Code Conventions bullet ("Pre-V1: no backwards-compatibility shims, data-migration helpers, or feature flags.") with the opposite, permanent guidance: from `1.0.0` onward, schema and stored-format changes must migrate forward — no more wipe-and-reinitialize shortcuts
- [x] 3.6 Remove the data-wipe notice blockquote from the existing `1.0.0-alpha` section in `CHANGELOG.md`, keeping only its platform-scope note
- [x] 3.7 Add a `1.0.0` section to `CHANGELOG.md` (above `1.0.0-alpha`) marking the transition out of preview, with only the platform-scope note (no data-wipe notice)

## 4. Verification

- [x] 4.1 Full `dotnet build` + `dotnet test` green; confirm zero warnings introduced
- [x] 4.2 Confirm the built executable's file properties and the About dialog show the `1.0.0` version
