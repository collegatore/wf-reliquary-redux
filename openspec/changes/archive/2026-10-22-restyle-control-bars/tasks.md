# Tasks

## 1. Shared style layer

- [x] 1.1 Inspect Avalonia 12.0.5's Fluent `Button` and `CheckBox` control templates (from the package's theme sources or the decompiled `Avalonia.Themes.Fluent` assembly) and note the template part names and state brush keys the selectors will target (D2, D3); verify by listing them in a comment at the top of the new styles file
- [x] 1.2 Create `src/Presentation/WF.Reliquary.Presentation/Styles/ControlBars.axaml` with the brush resources (`ControlBarBackgroundBrush` = current `#1A808080`, `ControlSegmentBackgroundBrush`, `ControlOutlineBrush`) and include it from `App.axaml` after the TreeDataGrid theme; verify `dotnet build` succeeds and the app still looks unchanged
- [x] 1.3 Add the `Border.control-bar` and `Border.control-bar.compact` styles (D6) to `ControlBars.axaml`; verify the build succeeds
- [x] 1.4 Add the segment styles (`Border.segment`, inner buttons, `:not(:nth-child(1))` hairline divider, hover/press washes, clipped rounded corners) per D2, including a comment noting that segment members must not be hidden conditionally; verify the build succeeds
- [x] 1.5 Add the `CheckBox.chip` styles (outline pill, reduced `MinHeight`, hover wash, `:checked` tint from `CardSelectedBrush`) per D3; verify the build succeeds
- [x] 1.6 Add `TextBlock.section-heading` and `TextBlock.field-caption` styles (D4, D5), declare `SectionHeadingFontSize` (16) in `App.axaml`, and rescale it in `AppearanceViewModel.ApplyFontSize` alongside the card widths; verify the build succeeds and the existing unit and integration tests pass (`dotnet test`)

## 2. Relics and Missions tab

- [x] 2.1 In `RelicsPanelView.axaml`, convert the Relics control panel: `control-bar` class, "Relics" as `section-heading`, Filters | Sorting in one `segment`, separators removed, "Updating…" kept; verify in the running app that the bar matches the `control-bar-visual-style` scenarios for headings and segments
- [x] 2.2 In `RelicsPanelView.axaml`, convert the filter panel (Show vaulted and Show all rewards as `chip` checkboxes, divider removed, Show all rewards still hidden in Table mode) and the sorting panel (direction button as a one-button `segment`); verify clicking a chip's label toggles it, checked chips are tinted, and Show all rewards hides and restores in Table mode with its state kept
- [x] 2.3 In `RelicsMissionsView.axaml`, convert both layout variants of the missions separator panel (`control-bar compact`, squared bottom corners kept, "Missions" as `section-heading`, Show exact drop chances as a `chip`); verify in both the stacked and side-by-side orientations that the bar is as tall as the Prime Gear "Vaulted" header bar and the splitter still resizes both areas

## 3. Prime Gear tab

- [x] 3.1 In `PrimeGearView.axaml`, convert the primary bar: `control-bar` class, wishlist selector with the inner "Wishlist" `field-caption` before the name (truncation and tooltip kept), Filters | Group operations in one `segment`, separators removed; verify clicking the caption opens the wishlist flyout and a long wishlist name still truncates with a tooltip
- [x] 3.2 In `PrimeGearView.axaml`, convert the filter row (three `chip` checkboxes, separators removed) and the group-operations row (count as plain text, Select all | Select none and Mark owned | Unmark owned as two `segment`s); verify each filter still filters, and select all / none and bulk mark / unmark still work
- [x] 3.3 Point the Prime Gear "Vaulted" header and the main-window footer at `ControlBarBackgroundBrush` without changing their look; verify with a search that no inline `#1A808080` remains in the views (`grep -rn "1A808080" src/Presentation --include=*.axaml` shows only `ControlBars.axaml`)

## 4. Documentation

- [x] 4.1 Add a short "Control-bar styling" note to `docs/technical-reference.md` (UI Design section): where `ControlBars.axaml` lives, the available classes (`control-bar`, `compact`, `segment`, `chip`, `section-heading`, `field-caption`, `bar-field`), and the rule that bars use these classes instead of inline colors and separators; verify the note names files and classes that exist

## 5. Review revisions

- [x] 5.1 Replace the joined segments with separate button chips: `StackPanel.button-group` (6px spacing) with each `Button` given the field look and hover/pressed tints, shared with `Button.bar-field` (D2); convert all five segment wrappers in `RelicsPanelView.axaml` and `PrimeGearView.axaml`; verify in snapshots (`prime-gear-hover`, `relics-stacked`) that each button has its own outline and related buttons sit closer together
- [x] 5.2 Rename `ControlSegmentBackgroundBrush` to `ControlFieldBackgroundBrush`; verify `grep -rn ControlSegment src` finds nothing
- [x] 5.3 Change the wishlist caption to "Wishlist:"; verify in the `prime-gear` snapshot
- [x] 5.4 Add the `Border.control-bar ComboBox` field and hover styles (D7); verify in the `relics-sort-hover` snapshot that the sort-mode dropdown shows the same hover tint and outline as the wishlist selector, and that `dotnet test` passes
- [x] 5.5 Update the docs note in `docs/technical-reference.md` for `button-group`, the bar dropdown style, and `ControlFieldBackgroundBrush`; verify the note names classes that exist

- [x] 5.6 Join the group-operations pairs (Select all / Select none, Mark owned / Unmark owned) with `StackPanel.button-group.joined` (D2), keeping all other bar buttons separate; verify in the `prime-gear-joined-hover` snapshot that each pair shows one outline with a single divider and rounded outer corners, and that `dotnet test` passes

## 6. Integration check

- [x] 6.1 Walk both tabs in the running app at Small, Medium, and Large font sizes and in all three relic view modes: confirm buttons read as clickable at rest, no free-standing separators remain, headings scale with the font size, the Prime Gear primary bar does not crowd the search box in a narrow window, and every control behaves exactly as before; run `dotnet test` once more and verify it passes (Signed off by the user after reviewing the snapshots, 2026-10-22.)
