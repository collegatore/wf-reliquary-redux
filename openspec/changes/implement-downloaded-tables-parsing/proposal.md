## Why

The app can now download and preserve the official Warframe drop-table HTML,
but PI1 still has no runtime way to turn that stored source into relic, Prime
Part, mission, rotation, or chance data. Parsing the downloaded file now creates
the first usable product data set for future search and acquisition-path views.

## What Changes

- Add an application-layer import workflow that loads the previously downloaded
  drop-table HTML and parses the `Relics` and `Missions` sections relevant to
  Void Relics and Prime Parts.
- Parse relic reward tables into base relics, Prime Part reward items, reward
  tiers, reward slots, and source-row context while deriving tier from Intact
  relic chances.
- Parse mission reward tables into mission sources, optional rotations, reward
  rows, source rarity labels, and mission drop percentages, linking relic reward
  rows back to recognized base relics.
- Persist parsed, query-ready records in SQLite through infrastructure-owned
  storage while preserving source fidelity needed for audit and future refresh
  diagnostics.
- Add a minimal presentation-layer import test button that invokes only the
  composed import workflow against the already stored source file for manual
  validation and displays the latest import status.
- Surface import results through application models, including source Last
  Update, imported row counts, and failures for missing or malformed source
  files.
- Add validation coverage for parser behavior, import orchestration,
  persistence, duplicate/source-context preservation, and layer boundaries.
- Keep item lookup, acquisition-path UI, mission ranking, wishlist behavior,
  live download/refresh from the import button, and background refresh out of
  scope.

## Capabilities

### New Capabilities
- `drop-table-parsing-import`: Parses a stored Warframe drop-table HTML source
  into PI1 relic, Prime Part, mission source, rotation, and mission reward data,
  then persists the normalized import for later queries.

### Modified Capabilities
- None.

## Impact

- Touches application-layer drop-table import models, parser contracts,
  orchestration services, and storage ports.
- Touches infrastructure for HTML parsing implementation and SQLite persistence.
- May add a focused HTML parsing package if standard .NET APIs are not
  sufficient for robust table traversal.
- Extends composition wiring so presentation or future workflows can invoke the
  import service without referencing infrastructure directly.
- Extends presentation with a narrow manual import testing action while keeping
  search, acquisition-path UI, and source download/refresh out of scope.
- Extends unit and integration tests with parser fixtures based on the preserved
  Warframe drop-table sample and focused minimal HTML cases.
- Does not add user-facing search screens, ranking logic, database migrations
  beyond the import schema, or live network behavior.
