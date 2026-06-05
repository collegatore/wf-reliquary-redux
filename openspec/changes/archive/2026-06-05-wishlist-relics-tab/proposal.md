## Why

Users have wishlisted prime parts but currently have no way to see which relics contain those parts — they must mentally cross-reference the Prime Gear Parts view against an external relic list. A dedicated Wishlist Relics tab surfaces exactly the relics worth farming, ordered by era and filtered to exclude unobtainable (vaulted) relics by default.

## What Changes

- Add a new "Wishlist Relics" tab to the main navigation
- Display relic cards showing all rewards, with wishlisted parts highlighted in bold
- Cards ordered by era (Lith → Meso → Neo → Axi) then alphabetically by name
- Control panel above cards with a checkbox to show/hide vaulted relics (hidden by default)
- Vaulted relics (those not appearing as a reward in any active mission) are visually marked the same way as vaulted gear elsewhere in the app

## Capabilities

### New Capabilities
- `wishlist-relics-view`: Tab displaying relic cards for relics that contain wishlisted prime parts, with era ordering, vaulted filtering, and reward highlighting

### Modified Capabilities
<!-- None — no existing spec-level requirements are changing -->

## Impact

- New tab component added to the main UI tab bar
- Requires joining wishlist data → prime part → relic reward → relic → mission data
- Vaulted relic detection reuses the same "no active mission reward" logic used for prime gear vault status
- Read-only view; no data mutations
