## Context

The "Relics and Missions" tab is a 4-row grid: top control panel (Row 0), relic-cards scroll (Row 1), a 5 px GridSplitter (Row 2), and missions scroll (Row 3). The current top control panel holds both a Relics group and a Missions group. The thin GridSplitter gives no visual hint about what lives below it.

Moving the Missions group to the separator area reinforces the spatial relationship between controls and content: Relics controls sit above relic cards; Missions controls sit immediately above mission cards.

## Goals / Non-Goals

**Goals:**
- Missions controls ("Show exact drop chances") appear as part of the separator row, not the top panel.
- Separator row is visually styled like the tab's control bars (same `Background` tint) but compact — matching the "Vaulted" separator bar's height, with squared bottom corners against the splitter.
- The GridSplitter drag-to-resize behavior is preserved so the user can still resize the two halves (resizing the relic-cards scroll and the missions scroll, not the panel).
- Top panel only contains the Relics group.

**Non-Goals:**
- No change to the Relics controls or their binding logic.
- No change to any ViewModel or backend code — this is a pure AXAML restructure.
- No change to card content, ordering, or data loading (separators and node-name font are presentation-only chrome).

## Decisions

### Decision: Pin the separator panel to the bottom of the relic-cards section (as built)

An initial attempt gave the separator panel its own outer grid row between the relic-cards scroll and the splitter (`RowDefinitions="Auto,*,Auto,4,*"`). This **did not work**: an Avalonia `GridSplitter` resizes the rows immediately adjacent to it, so with the panel sitting between the relic scroll and the splitter, dragging stretched the *panel* (an `Auto` row) instead of the relic-cards scroll, and the panel's height changed on drag.

**Final structure (`RowDefinitions="Auto,*,4,*"`):**
- Row 0: top control panel (Relics group only).
- Row 1: a nested `Grid` with `RowDefinitions="*,Auto"`:
  - nested Row 0 (`*`): relic-cards scroll.
  - nested Row 1 (`Auto`): Missions separator panel, pinned to the bottom of the section.
- Row 2: GridSplitter (4 px).
- Row 3: missions scroll.

Because the splitter is once again directly between the two star-sized rows (relic section and missions scroll), dragging transfers height between the relic-cards scroll and the missions scroll, while the separator panel (`Auto`) keeps its fixed height and rides along just above the splitter. This is the key constraint: the splitter must stay adjacent to both resizable areas.

### Decision: Slim the separator panel to the "Vaulted" bar height

The panel is styled like the tab's other control bars (`Background="#1A808080"`, `Margin="8,4,8,0"`) but tuned to read as a divider rather than a full control panel:
- `Padding="12,2"` — the checkbox's intrinsic ~32 px height (a Fluent theme floor that `MinHeight="0"` does not override, and that clips if a smaller explicit `Height` is forced) means matching the compact "Vaulted" separator bar's total height requires trimming the vertical padding rather than shrinking the checkbox. `12,2` lands the panel on the "Vaulted" bar height.
- `CornerRadius="4,4,0,0"` — top corners rounded, bottom corners squared where the panel meets the splitter.

### Decision: Card chrome adjustments (separators and node font)

- Relic card: the single separator between the relic-name header and the rewards body is removed; tier accent bars and spacing already convey structure.
- Mission card: keep the existing separator after the title/chance header, and add a second separator after the node-names line. The node-names line changes from `FontStyle="Italic"` + `Opacity="0.7"` to regular font.

## Risks / Trade-offs

- [Splitter adjacency] The splitter only works while it is directly between the two star rows. The separator panel must stay inside the relic section's nested grid (not its own outer row). → Captured as the primary decision above.
- [Padding-based height match] Matching the "Vaulted" bar height via reduced padding is sensitive to the checkbox's themed height; a future theme change to checkbox sizing could shift it. → The single knob is the separator Border's vertical padding.
