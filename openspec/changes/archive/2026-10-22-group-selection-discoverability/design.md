# Design

## Context

See proposal.md (Why) for motivation and the two spec deltas for the required behavior.

Current state relevant to the approach:

- The gear card is the `GearCardTemplate` `DataTemplate` in `PrimeGearView.axaml`: a `Panel` layering the card `Border` (header grid with name, owned ✓ and the `⋮` button, then the `Parts` `ItemsControl`) under a full-card `Button.card-select-overlay`. The overlay is visible only in group mode (`IsVisible` bound through `RelativeSource AncestorType=UserControl` to `DataContext.PrimeGear.IsGroupMode`). It sits at the same `Margin="8"` as the card, so it exactly covers it, and its command is the card's `ToggleSelectCommand`.
- Because the overlay is on top, the card's own controls already don't receive clicks in group mode. Nothing changes visually, though.
- `MainWindow.axaml` styles the overlay so its background stays transparent on `:pointerover` and `:pressed` (targeting `/template/ ContentPresenter#PART_ContentPresenter`), and gives `Border.gear-card.selected` the `CardSelectedBrush` wash.
- The template comment already describes a "⋮ / selection-checkbox swap" that the markup doesn't implement.
- `PrimeGearViewModel.SelectedCount` is an `int` with change notification. The view shows it with `StringFormat='{}{0} selected'`. `PrimeGearViewModelTests` (integration tests) already cover selection counting, select all/none, and clearing.
- Owned gear cards render at `Opacity` 0.55 (`Border.gear-card.owned`).

## Goals / Non-Goals

**Goals:**
- Add the checkbox, dimming, hover, and hint with no change to how selection works: the overlay stays the single click target.
- Keep the hint/count logic in the view model, where it can be tested.

**Non-Goals:**
- No change to selection semantics, bulk actions, or the batched prompt.
- No change to the selected-card wash.
- No styling of the Group operations button or the group row beyond the status text (that's `restyle-control-bars`).

## Decisions

### D1. The selection checkbox is a display-only `CheckBox` that swaps with the `⋮` button via visibility
In the header grid's right column, a `CheckBox` with `IsChecked="{Binding IsSelected, Mode=OneWay}"` and `IsHitTestVisible="False"` sits next to the existing `⋮` button. The checkbox is visible in group mode and the `⋮` button outside it, both bound to the same `DataContext.PrimeGear.IsGroupMode` ancestor binding the overlay already uses (the `⋮` with `!`).
- *Why:* the overlay already turns any click on the card, including on the checkbox's area, into `ToggleSelectCommand`. A display-only checkbox therefore can't double-toggle or fight the overlay, and its checked state just mirrors `IsSelected`. Reusing the overlay's binding avoids adding a group-mode flag to every card view model.
- *Alternatives:* an interactive checkbox bound two-way to `IsSelected` would need the overlay to leave a hole for it, giving two click paths to keep consistent. A per-card `IsGroupMode` property would mean plumbing and change notification across all cards for a view-only concern.
- Fix the stale template comment so it describes the swap as it now exists.

### D2. The parts list is dimmed through a style class bound to group mode
The `Parts` `ItemsControl` gets `Classes.group-mode="{Binding DataContext.PrimeGear.IsGroupMode, RelativeSource=…}"`, and a style `ItemsControl.group-mode` lowers its `Opacity`. The header (name, ✓, checkbox) stays at full strength, so the selection checkbox reads clearly.
- *Why:* dimming keeps the layout identical when the mode toggles; hiding the part checkboxes would reflow every card. A style class keeps the value next to the other card styles.
- *Alternatives:* hiding part checkboxes (reflow); dimming the whole card (would also fade the checkbox that teaches the interaction).

### D3. Hover feedback is drawn by the overlay itself on `:pointerover`
The `Button.card-select-overlay:pointerover` style, which today only keeps the background transparent, also gives the template's `ContentPresenter` a 2px border in the system accent color (`SystemAccentColor`, already used by the footer spinner) with the card's `CornerRadius` (4). The overlay covers the card exactly, so the outline traces the card's edge. Background stays transparent, so hovering never looks like the selection wash.
- *Why:* uses the element that already receives the pointer; no code-behind or extra visual tree.
- *Alternatives:* a wash on hover would be confused with the selected wash, which we explicitly kept. Styling the sibling card `Border` from the overlay's hover state isn't expressible with selectors without code.

### D4. `PrimeGearViewModel` exposes `SelectionStatusText`
A read-only `string SelectionStatusText` returns "Click cards to select them" when `SelectedCount` is 0, and "{n} selected" otherwise. It's raised from the `SelectedCount` setter. The group row's status `TextBlock` binds to it in place of the `StringFormat` binding.
- *Why:* the text depends on state the view model owns, and putting it there makes the hint testable alongside the existing selection tests. Every path that empties the selection (Select none, bulk actions, view changes, mode off) already funnels through `RecountSelected` / `SelectedCount`, so the hint returns automatically.
- *Alternatives:* two `TextBlock`s toggled by a `HasSelection` binding work too, but split the wording across the view and can't be unit-tested.

### D5. Unmark owned and the joined class both bind to `ShowOwned`
The Unmark owned button binds `IsVisible` to `PrimeGear.ShowOwned`, and its group binds `Classes.joined` to the same value, so the pair is joined only while both buttons are visible.
- *Why:* joined-pair corners and borders are chosen by `:nth-child` / `:nth-last-child`, which count hidden children. Leaving the group joined while Unmark is hidden would give Mark owned a squared right side and no right border. Dropping `joined` makes Mark owned an ordinary pill.
- *Alternatives:* disabling Unmark owned instead of hiding it (keeps a control that can never act; the user asked for it hidden); computing first/last visible children in code (more machinery than one bound class).

### D6. Selection-dependent actions use `CanExecute`; inactive bar buttons fade to 45% opacity
Each action's `CanExecute` is "there is something to act on": Select none needs any selected card, Mark owned a selected unowned card, and Unmark owned a selected owned card, matching the targets the bulk actions already compute. Select none depends only on the count, so the `SelectedCount` setter re-raises it when the count crosses zero. Mark and Unmark depend on which cards are selected, so `RecountSelected`, which runs on every selection toggle, re-raises both each time. Avalonia disables a button whose command can't execute: it gets no `:pointerover` and no clicks (checked headlessly), so the hover styles never apply. `StackPanel.button-group > Button:disabled` sets `Opacity` to 0.45, greying text, outline, and fill together.
- *Why:* `CanExecute` keeps the rule in the view model, where it's tested, and the view needs no extra bindings. Opacity is an interim look: `add-color-palette-setting` decides the final inactive style and gives it a palette color.
- *Alternatives:* binding `IsEnabled` to a `HasSelection` property in the view (duplicates what `CanExecute` already expresses); hiding the actions (the row would jump as the selection changes).

## Risks / Trade-offs

- [Dimming compounds with the owned card's 0.55 opacity, making owned parts lists very faint in group mode when Show owned gear is on] → choose a mild dim (around 0.6) and check an owned card in group mode. The parts list is informational in this mode anyway.
- [The checkbox and the `⋮` button differ in width, so the header shifts slightly when the mode toggles] → keep the right column at a fixed width matching the wider of the two. Verify by toggling the mode on a card with a long name.
- [The hover outline overlaps the owned card's green 2px outline] → accepted: the hover is transient, and it should win while the pointer is over the card. Verify that both look acceptable.
- [The hint is wider than "N selected" and could shift the row's buttons when the selection changes] → size the status cell with an invisible copy of the hint (same grid cell, `Opacity` 0), so it is always as wide as the hint at the current font size and the buttons stay put; a fixed `MinWidth` would only fit one font size. Verify by selecting and deselecting a card.
