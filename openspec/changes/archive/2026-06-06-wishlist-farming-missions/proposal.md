## Why

The "Wishlist Relics" tab tells players which relics contain their wishlisted parts, but not where to acquire those relics. Players must cross-reference an external source to find missions that drop the relics they need, breaking the workflow the app is meant to support.

## What Changes

- The Wishlist tab gains a second panel (side-by-side split layout) showing **Farming Missions**: every mission that rewards at least one wishlisted relic, ranked by cumulative wishlist-relic drop chance descending.
- Each mission card shows the mission name and its effective wishlist chance. Rotation-based missions (Defense, Survival, etc.) show a per-rotation breakdown with each rotation's cumulative chance and the individual relics contributing to it; rotations with no wishlisted relics are shown explicitly at 0% so the player can see how rotation dilution affects the average. Non-rotation missions list their contributing relics directly.
- The effective chance used for ranking is: the direct pool sum for non-rotation missions, and the mean across **all** rotations (including 0% ones) for rotation-based missions.
- Vaulted relics require no special handling: a relic is vaulted if and only if no mission currently rewards it, so the join through `mission_rewards` excludes them implicitly.

## Capabilities

### New Capabilities

- `wishlist-farming-missions-view`: The farming missions panel — reading, computing, and displaying per-mission and per-rotation wishlist relic drop chances alongside the existing relic cards.

### Modified Capabilities

- `wishlist-relics-view`: The tab is renamed to "Relics and Missions" and its layout changes from a single-panel scroll to a two-panel split to host the new farming missions panel.

## Impact

- **New application port**: `IWishlistFarmingMissionsReader`
- **New infrastructure**: `GetWishlistFarmingMissions.sql`, `SqliteWishlistFarmingMissionsReader`
- **New presentation**: `WishlistFarmingMissionsViewModel` and associated card/rotation/relic view models
- **Modified AXAML**: Tab renamed to "Relics and Missions"; tab view restructured to a two-column `Grid`
- **DI registration**: new reader registered in `ReliquaryServiceFactory`
- No schema migrations required; all data is already present in `mission_sources`, `reward_rotations`, `mission_rewards`, and `relics`
