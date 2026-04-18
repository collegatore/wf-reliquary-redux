# Quickstart: Drop Table Schema Preparation

## Prerequisites

- Repository opened from the project root.
- The preserved sample exists at
  `specs/001-drop-table-schema/artifacts/warframe-drop-tables-sample.html`.
- .NET 10 SDK is not required to review this feature's design artifacts; it is
  listed only for later implementation work.

## Validate Planning Artifacts

From the repository root:

```powershell
Get-ChildItem specs/001-drop-table-schema
Get-ChildItem specs/001-drop-table-schema/artifacts
Get-ChildItem specs/001-drop-table-schema/contracts
```

Expected files:

- `specs/001-drop-table-schema/spec.md`
- `specs/001-drop-table-schema/plan.md`
- `specs/001-drop-table-schema/research.md`
- `specs/001-drop-table-schema/data-model.md`
- `specs/001-drop-table-schema/quickstart.md`
- `specs/001-drop-table-schema/artifacts/drop-table-schema-analysis.md`
- `specs/001-drop-table-schema/artifacts/warframe-drop-tables-sample.html`
- `specs/001-drop-table-schema/contracts/parser-contract.md`
- `specs/001-drop-table-schema/contracts/query-contract.md`

## Validate Source Sample

Confirm the source metadata is visible:

```powershell
Select-String -Path specs/001-drop-table-schema/artifacts/warframe-drop-tables-sample.html -Pattern 'Last Update'
Select-String -Path specs/001-drop-table-schema/artifacts/warframe-drop-tables-sample.html -Pattern 'missionRewards','relicRewards'
```

Expected result:

- The source last-update marker is present.
- `missionRewards` and `relicRewards` section anchors are present.

## Future Implementation Validation

This feature does not create a runnable codebase. Once a later implementation
feature creates source projects, use standard .NET CLI commands from the repo
root:

```powershell
dotnet restore
dotnet build
dotnet test
```

Required future test coverage identified by this feature:

- Parser finds Mission Rewards and Relics sections.
- Parser handles rotated and unrotated mission reward pools.
- Parser identifies relic era and code from relic headings.
- Parser derives relic reward tier from Intact chances: 25.33% as `Common`,
  11.00% as `Uncommon`, and 2.00% as `Rare`.
- Parser does not persist refinement records.
- Normalization preserves duplicate reward rows using source order.
- Query behavior resolves Prime Part to base relics and reward tiers.
- Query behavior resolves relics to mission sources.
- Derived relic reward chance and mission reward chance remain separate fields.

## Manual Review Checklist

- Confirm `data-model.md` can represent at least three sample acquisition paths.
- Confirm parser and query contracts do not mention presentation-layer types.
- Confirm persistence fields are SQLite-friendly but not tied to UI behavior.
- Confirm no application source code, test code, project files, or runtime
  database files are required for this feature to be complete.
- Confirm `README.md` remains accurate before implementation begins.
