# Proposal

## Why

The control bars on the Relics and Missions and Prime Gear tabs (the primary bars, their collapsible Filters/Sorting rows, the group-operations row, and the missions separator bar) look flat and ambiguous. Their buttons are transparent and borderless, so nothing reads as clickable until hovered; grouping is conveyed only by free-standing grey vertical rules; the pane titles ("Relics:", "Missions:") are styled exactly like the field label "Wishlist:", so the former looks like it labels the button next to it and the latter looks like it governs the whole row; and each checkbox is a separate element from its text, so the two do not read (or behave) as one control. Every bar also repeats the same hand-written tint, separator, and button values inline, which makes any visual adjustment a multi-file edit.

## What Changes

- Introduce a shared control-bar visual language, defined once as named styles:
  - **Button chips**: bar buttons are outlined pills, lighter than the bar. Related buttons sit closer together than unrelated ones (Filters and Sorting; Filters and Group operations; the sort-direction button alone). The two paired actions, Select all / Select none and Mark owned / Unmark owned, are each joined into one outlined control with a single divider.
  - **Section headings**: "Relics" and "Missions" become headings of the pane beneath them, with no colon, SemiBold, slightly larger, and set apart from the following controls by a gap rather than a separator. Prime Gear's primary bar has no heading.
  - **Field label inside its control**: the wishlist selector shows a muted "Wishlist:" caption inside the dropdown button, before the active wishlist name; there is no separate label in front of the button.
  - **Bar dropdowns match the field**: the relic sort-mode dropdown has the wishlist selector's background, outline, and hover.
  - **Checkbox chips**: every control-bar checkbox carries its label as its own content (one click/hover target) and sits in an outlined pill; a checked chip is tinted with the app's existing selection color (`CardSelectedBrush`).
  - **No free-standing separators** in these bars; grouping comes from spacing between outlined controls.
- Replace the duplicated inline tint/separator/button values on these bars with the shared styles and resources.
- Correct the stale placement wording of the "Show exact drop chances" requirement (it still describes a separator after the Relics group, although the checkbox now lives in the missions separator panel).

Behavior is unchanged: every button, checkbox, dropdown, panel toggle, and persisted setting works exactly as before. Only presentation changes.

Out of scope (discussed and deferred): icons on bar buttons; showing a toggle's open/closed state on its button; summarizing filter state on a closed panel; showing the active wishlist on the Relics and Missions tab; group-selection discoverability on gear cards (planned as its own follow-up change, which will reuse the selection color introduced here for chips).

Observed discrepancy, left as is: the `relics-sorting-panel` requirement "Sorting button label reflects the active sort" (e.g. "Sorting: Score ↓") is not implemented; the button reads a static "Sorting". This change neither implements nor removes that requirement.

## Capabilities

### New Capabilities
- `control-bar-visual-style`: the shared visual language of the tab control bars and their panel rows: button chips, section headings vs. in-control field labels, bar dropdowns sharing the field look, checkbox chips, the selected/on tint, and the absence of free-standing separators.

### Modified Capabilities
- `relics-filter-panel`: the Filters button is no longer specified as transparent/borderless, and the panel's checkboxes are chips rather than items split by a thin vertical divider.
- `relics-sorting-panel`: the Sorting button is no longer specified as transparent/borderless; it is an outlined button chip grouped with Filters.
- `wishlist-relics-view`: the Relics control panel and missions separator panel use "Relics" / "Missions" section headings (no colon, heading style) instead of plain "Relics:" / "Missions:" text labels, and their controls follow the shared control-bar style.
- `exact-chances-toggle`: the checkbox's placement is described as a chip in the missions separator panel under the "Missions" heading, replacing the stale separator/"Missions:" wording.

## Impact

- Code: a new shared control-bar styles file included from `App.axaml`, a heading font-size resource scaled by the existing font-size setting (`AppearanceViewModel`), `RelicsPanelView.axaml`, `RelicsMissionsView.axaml` (both layout variants of the missions separator panel), `PrimeGearView.axaml` (primary bar, filter row, group-operations row). The Prime Gear "Vaulted" section header and the main-window footer keep their current look (the footer's divider is mandated by `drop-tables-footer-status`), though they may read the shared tint resource.
- No view-model, data, settings, or persistence changes. No new dependencies.
- Manual visual verification on both tabs, in both Relics and Missions layout orientations and all three relic view modes.
