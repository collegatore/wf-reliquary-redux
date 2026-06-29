## Why

The Relics and Missions tab today is fixed to a **stacked** layout — relics on top,
missions on the bottom, split by a horizontal `GridSplitter`. On wide windows this wastes
horizontal space and forces lots of vertical scrolling. A **side-by-side** layout (relics
left, missions right) would use wide displays far better. We want to add a layout toggle,
but first validate with a cheap POC that side-by-side is actually useful before investing
in persistence and a polished toggle.

## What Changes

This change is delivered in two phases with an approval gate between them.

**Phase A — POC (throwaway-grade, no persistence, zero code-behind):**
- Add a side-by-side layout (two columns split by a column `GridSplitter`) to the
  Relics/Missions view, alongside the existing stacked layout.
- The side-by-side layout uses **star-sized columns** (native splitter drag) and a
  declarative `MinWidth` per column based on the fixed relic card width — so it needs no
  star→pixel conversion, no tallest-card measurement, and no drag persistence, i.e. **no
  code-behind**.
- A non-persisted in-session toggle switches between stacked and side-by-side so the
  layouts can be compared live.
- **GATE:** evaluate usefulness. If approved, proceed to Phase B; otherwise the POC is
  discarded.

**Phase B — full implementation (only if Phase A is approved):**
- Replace the throwaway toggle with a real control: a Stacked / Side-by-side radio group
  in Settings under the Appearance group (next to font size), applied live and auto-saved
  like the font-size control.
- Persist the chosen orientation and restore it on launch.
- Remember each orientation's split position independently (stacked = relics pixel
  height as today; side-by-side = relics column pixel width, with the relics column fixed
  and the missions column absorbing window-resize slack).
- Hoist the relic and mission card markup into shared `DataTemplate` resources so both
  layouts reference one card definition instead of duplicating it.

## Capabilities

### New Capabilities
- `relics-missions-layout-orientation`: the Relics/Missions tab supports two layout
  orientations (stacked and side-by-side), a user toggle to switch between them, the
  side-by-side layout's column split and minimum-width behavior, and the default
  orientation.

### Modified Capabilities
- `split-panel-persistence`: persistence is extended to store the chosen orientation and
  to remember the side-by-side split position independently of the stacked height (Phase
  B only). The existing stacked-layout behavior is unchanged.

## Impact

- `RelicsMissionsView.axaml(.cs)` (introduced by `decompose-main-window-tabs`, which is a
  prerequisite) — gains the side-by-side layout subtree and the orientation switch.
- Phase B: `UserSettings` gains an `Orientation` field and a `SideBySideRelicsWidth`
  (pixel) field; `LocalUserSettingsStore`, `SettingsViewModel`/`MainWindowShellViewModel`,
  and the window close-time save are updated accordingly. The layout radio group is added
  to `SettingsView` under Appearance.
- Phase B: new/relocated shared card `DataTemplate` resources (e.g. a `ResourceDictionary`).
- No changes to readers, application services, or the database.
- **Depends on** `decompose-main-window-tabs` landing first (the POC and feature live in
  the extracted `RelicsMissionsView`).
