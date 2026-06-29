## Context

The Relics and Missions tab is fixed to a stacked layout built on a
`Grid RowDefinitions="Auto,*,4,*"` with a `GridSplitter ResizeDirection="Rows"`. That
layout requires non-trivial code-behind (see `split-panel-persistence`): star→pixel row
conversion deferred to first measure, drag-completion persistence to
`shell.RelicsPanelHeight`, and a minimum-height enforced from the tallest visible relic
card (cards vary in height).

The key insight driving this design: **side-by-side is structurally simpler than
stacked.** A column split between two `*` columns is dragged natively by `GridSplitter`
with no code-behind, and its only minimum-size concern is column *width* — which is the
fixed `RelicCardWidth` resource (223) and can be declared as `MinWidth`. So the POC can be
pure markup. This change deliberately validates the idea cheaply (Phase A) before adding
persistence and a real toggle (Phase B).

This change assumes `decompose-main-window-tabs` has landed, so the work happens inside
`RelicsMissionsView`.

## Goals / Non-Goals

**Goals:**
- Phase A: a working side-by-side layout with zero code-behind, switchable in-session to
  judge usefulness.
- Phase B (if approved): persistent orientation, independent per-orientation split
  position, a discoverable toggle, and shared card templates.
- Preserve the existing stacked layout and its behavior exactly.

**Non-Goals:**
- No change to relic/mission data, readers, or the database.
- No redesign of the stacked layout's existing code-behind.
- Phase A persists nothing and is not intended to ship as-is.

## Decisions

### Decision: Two layout subtrees toggled by visibility (Approach A)

Define stacked and side-by-side as two sibling subtrees whose visibility is bound to an
orientation flag. The existing stacked code-behind stays attached to the stacked subtree;
the side-by-side subtree stays declarative.

- **Alternative considered:** one grid reconfigured at runtime (swap
  RowDefinitions↔ColumnDefinitions, reparent the missions control bar, flip
  `ResizeDirection`). Rejected — the missions-bar reparenting and axis-swapping logic is
  exactly the kind of imperative layout surgery that breeds bugs, and it would entangle
  the careful stacked code-behind. Two subtrees keep each layout independently simple.

### Decision: Side-by-side uses a fixed-width relics column + star missions column

`ColumnDefinitions="<relicsWidth>,4,*"` with `GridSplitter ResizeDirection="Columns"`.
Each column carries a `MinWidth` derived from `RelicCardWidth` plus margins (316), and the
relics column defaults to a fixed pixel width (640).

- **Phase A (POC):** both columns were star-sized (`*,4,*`), dragged natively with zero
  code-behind, because nothing was persisted.
- **Phase B (shipped):** the relics column is a **fixed pixel** column and the missions
  column is the lone `*` column. This makes the relics card grid static on window resize
  (only the missions column grows/shrinks) — a deliberate UX correction made during apply.
  It requires a small amount of code-behind: on `GridSplitter.DragCompleted` the relics
  column's `ActualWidth` is saved, and on launch / when switching to side-by-side the saved
  pixel width is reapplied (relics → pixel, missions → star). This is far lighter than the
  stacked layout's machinery (no star→pixel conversion of a star drag, no tallest-card
  measurement — the constrained axis is fixed-width).

- **Alternative considered (and shipped in Phase A, then revised):** keep both columns
  star-sized. Rejected for the production layout — two star columns rescale *both* sides on
  window resize, so the relic grid would not stay static.

### Decision: Card markup hoisted to shared DataTemplates (Phase B)

Move the relic-card and mission-card markup into `DataTemplate`s keyed by `DataType` in a
shared `ResourceDictionary`. Both subtrees then contain only
`<ItemsControl ItemsSource="{Binding …Cards}"/>` and the template resolves by item type.
This removes duplication and keeps the hidden subtree cheap (it realizes no cards while
collapsed).

- **Alternative considered:** duplicate the ~70-line card markup in each subtree.
  Rejected for maintainability. Deferred to Phase B so the POC stays minimal.

### Decision: Persistence model — orientation enum + separate side-by-side position (Phase B)

Add a `RelicsMissionsOrientation` enum and a `SideBySideRelicsWidth` value (the relics
column width in **pixels**, not a ratio) to `UserSettings`, alongside the existing
`RelicsPanelHeight`. Each orientation remembers its own position; the stacked path is
untouched. Persistence routes through the shell and the existing window-close save,
consistent with today's `RelicsPanelHeight`.

- **Note (apply correction):** the value was originally specified as a column *ratio*.
  Because the production layout fixes the relics column width (see the layout decision
  above), a pixel width is the natural and correct unit — a ratio would not keep the relics
  grid static across window sizes. The store reads back a non-positive/missing value as
  "no preference" and falls back to the default fixed width.
- **Alternative considered:** a single shared position for both orientations. Rejected —
  the two orientations have different natural split points; remembering each independently
  is the better UX.

### Decision: Layout control lives in Settings → Appearance (apply correction)

The orientation control shipped as a **radio group (Stacked / Side by side) in Settings,
under the Appearance group next to font size**, rather than a control-bar toggle on the
tab. It behaves exactly like the font-size control: selecting an option applies live to
the shell and auto-saves immediately (loading the current settings file and writing only
the orientation field), so it is excluded from the Settings unsaved-changes guard. This
groups the two "appearance" preferences together and reuses the existing
`EnumToBoolConverter` radio pattern.

- **Alternative considered:** a control-bar button or a `ToggleButton` on the tab itself
  (used as the Phase A throwaway control). Rejected for the production UI — Settings is the
  established home for persisted preferences, and the live-apply + auto-save pattern already
  exists there for font size.

## Risks / Trade-offs

- **POC may not justify Phase B** → that is the explicit purpose of the gate; Phase A is
  cheap and discardable by design.
- **Two subtrees risk drift** → mitigated by hoisting shared card templates (Phase B) so
  the only per-subtree markup is small container scaffolding.
- **Collapsed subtree still instantiating cards** → bind visibility so the hidden
  `ItemsControl` does not realize items; verify no double data cost.
- **Toggle discoverability/placement** → resolved in Phase B (control-bar button vs View
  menu vs Settings); Phase A uses a throwaway control purely for evaluation.
- **WrapPanel re-wrap cost on resize** → the existing note about fixed card `Width` (not
  `MinWidth`) for mission cards applies to the side-by-side columns too; keep card widths
  fixed to avoid re-wrap churn on horizontal resize.

## Migration Plan

1. **Phase A:** add the side-by-side subtree + throwaway in-session toggle in
   `RelicsMissionsView`. No settings changes. Build, run, evaluate. **GATE.**
2. **Phase B (if approved):** hoist card templates; add orientation + side-by-side
   position to `UserSettings` and the store; add the Settings → Appearance layout radio
   group (live-apply + auto-save); wire per-orientation restore through the shell and
   window-close save; update tests.

Rollback: Phase A is self-contained and removable; Phase B builds on it additively.

## Resolved Questions

- **Final placement of the production control** → resolved: a Stacked / Side-by-side radio
  group in Settings → Appearance (next to font size), applied live and auto-saved.
- **Default side-by-side split** → resolved: the relics column defaults to a fixed **640 px**
  width (≈ two relic-card columns) with a 316 px minimum; the missions column takes the
  rest. Stored/restored as a pixel width, not a ratio.
