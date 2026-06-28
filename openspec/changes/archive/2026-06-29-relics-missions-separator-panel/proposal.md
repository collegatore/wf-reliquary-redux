## Why

The Missions controls currently sit in the top control panel alongside the Relics controls, but mission cards are in the bottom half of the tab. Moving the Missions controls to the separator between the two halves places them visually adjacent to what they govern and gives each panel a clear local header.

## What Changes

- The thin GridSplitter between the relic cards scroll area and the missions scroll area is accompanied by a styled control-panel strip. The separator panel is pinned to the bottom of the relic-cards section so the GridSplitter still sits directly between the two scrollable card areas and resizes them (it must remain adjacent to both star-sized rows to work).
- The new separator panel contains: "Missions:" label and the "Show exact drop chances" checkbox. It is styled like the tab's other control bars but slimmed to the compact height of the "Vaulted" separator bar, with its bottom corners squared where it meets the splitter.
- The top control panel retains only the Relics group: "Relics:" label, "Show vaulted" checkbox, and "Show all rewards" checkbox.
- The thick vertical divider separating Relics from Missions in the top panel is removed (no longer needed).
- Relic cards: the horizontal separator between the relic-name header and the rewards body is removed.
- Mission cards: a second horizontal separator is added after the node-names line (so a card has a divider after the title header and another after the node names), and the node-names line is changed from italic/dimmed to regular font.

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `wishlist-relics-view`: The control panel now contains only the Relics group; the Missions group has moved to a separator panel pinned to the bottom of the relic-cards section. The relic-card header/body separator is removed.
- `wishlist-farming-missions-view`: Mission cards gain a divider after the node-names line, and node names render in regular (non-italic) font.

## Impact

- `MainWindow.axaml`: The `RelicsMissionsGrid` is restructured — the relic-cards scroll and the Missions separator panel share a nested `*,Auto` grid in Row 1, with the GridSplitter (Row 2) and missions scroll (Row 3) below. Top control panel loses the Missions section. Relic-card and mission-card item templates change their separators/fonts.
- `openspec/specs/wishlist-relics-view/spec.md` and `openspec/specs/wishlist-farming-missions-view/spec.md`: Requirements describing the control-panel layout and card chrome are updated.
