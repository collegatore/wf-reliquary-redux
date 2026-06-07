## Why

The farming missions panel shows one card per mission source, so Defense nodes on Io and Hydron each get a separate card even when they offer identical relics at identical chances. Grouping same-type missions with the same reward table reduces visual noise and makes interchangeable nodes obvious. Additionally, a per-relic chance row for every relic adds math-level detail that some users do not need; a compact view showing only relic names in one row makes the panel easier to scan.

## What Changes

- Missions of the same `MissionType` (e.g., "Defense") whose relic reward structure is identical (same relics at the same individual chances, same rotations) are merged into a single card.
- The card title becomes the mission type instead of the individual mission label.
- A "Nodes" section at the top of the card body lists the mission display labels that belong to the group (e.g., "Io (Jupiter), Hydron (Sedna)").
- The existing rotation / flat-relic display follows below the nodes section, unchanged.
- A "Show exact drop chances" checkbox is added to the "Relics and Missions" tab control panel (alongside the existing "Show vaulted relics" and "Show all rewards" toggles).
  - **Toggled ON (default):** existing behaviour — one row per relic with its chance percentage.
  - **Toggled OFF:** all relics for a section rendered in a single compact row (comma-separated names), no chance text shown.
- Missions whose `MissionType` is null are not grouped; they keep one card per mission with their existing display label as the title.

## Capabilities

### New Capabilities
- `mission-type-grouping`: Aggregate farming mission cards by mission type and identical reward table (relics + per-relic chances per rotation or flat pool). Missions that differ in any relic or chance remain separate cards even within the same mission type.
- `exact-chances-toggle`: Tab-level toggle ("Show exact drop chances") that switches the relic list between the current per-row-with-percentage layout and a compact single-row names-only display.

### Modified Capabilities
- `wishlist-farming-missions-view`: Card structure changes — title is now mission type, card body gains a nodes list section before the existing relic/rotation content; both new capabilities integrate here.

## Impact

- **SQL query** (`GetWishlistFarmingMissions.sql`): add `ms.mission_type` and `ms.node` to both `SELECT` branches so the reader can group and label nodes.
- **Application models**: extend `WishlistFarmingMissionEntry` with `MissionType` (nullable string) and `Node` (nullable string); add `WishlistFarmingMissionGroupEntry` record that holds a type label, list of node labels, effective chance, and the shared rotation/relic structure.
- **Port** (`IWishlistFarmingMissionsReader`): return `IReadOnlyList<WishlistFarmingMissionGroupEntry>` instead of per-mission entries (or add a new method; existing method is internal to one consumer so replacement is safe).
- **Infrastructure** (`SqliteWishlistFarmingMissionsReader`): grouping logic in `BuildEntries` — after building per-mission entries, group by `(MissionType, canonical-reward-fingerprint)` and emit group entries; missions with null type form singleton groups.
- **Presentation** (`WishlistFarmingMissionsViewModel`): swap card VM to `WishlistFarmingMissionGroupCardViewModel`; add `ShowExactDropChances` bool property.
- **AXAML** (`MainWindow.axaml`): update mission card `DataTemplate` to render type title, nodes list, and conditionally show/hide chance columns; add toggle to control panel row.
