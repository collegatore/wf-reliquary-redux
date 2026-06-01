## Why

Players need to see at a glance which sub-parts make up each Prime item so they can plan and track what they still need to farm. The current drop-table import already stores all reward items (parts and blueprints) but the app exposes no way to browse them as assembled gear.

## What Changes

- Add a **Prime Gear** tab to the main UI alongside any existing tabs.
- Each Prime item is displayed as a card with the gear name as the title and its component parts listed as checkboxes.
- A query layer groups `reward_items` by derived parent gear name (parts share a common name prefix, e.g. `Afuris Prime Receiver` → parent `Afuris Prime`).
- No schema changes required; grouping is derived from item names at query time.

## Capabilities

### New Capabilities

- `prime-gear-parts-view`: UI tab displaying Prime gear cards — each card shows the gear name and a checkbox list of its component parts derived from the drop-table `reward_items`.

### Modified Capabilities

<!-- None — no existing spec requirements change. -->

## Impact

- **Presentation**: New tab view, card control, checkbox list component.
- **Application**: New query use-case that groups reward items by parent gear name and returns a structured list of gear with parts.
- **Infrastructure**: New read query against the existing SQLite `reward_items` table — no schema changes.
- **Tests**: Unit tests for the name-grouping logic; integration test verifying the query returns grouped results from a seeded database.
