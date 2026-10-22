# Proposal

## Why

Group-operations mode on the Prime Gear tab works once you know it, but nothing tells you how. Turning the mode on does not change the cards at all: they keep drawing their part checkboxes and overflow menu, which no longer respond, while the real click target is an invisible full-card overlay whose hover and press feedback is deliberately suppressed. The only cues are a hand cursor and a delayed tooltip. Users have to guess that the whole card has become a selector, and the controls they can see actively suggest otherwise.

## What Changes

- In group-operations mode, each card shows a **selection checkbox in place of the overflow-menu button**. It reflects the card's selection state; clicking anywhere on the card (including the checkbox) still toggles selection.
- In group-operations mode, the card's **parts list is dimmed**, so the controls that no longer respond visibly look inactive and the card visibly changes when the mode turns on.
- In group-operations mode, a card shows **hover feedback** (an outline around the whole card) while the pointer is over it, so the card reacts as one target.
- The group-operations row shows **"Click cards to select them"** in place of the selection count while nothing is selected, and the count ("N selected") once at least one card is selected.
- Selection actions are **inactive when they have nothing to act on** (greyed out, with no hover or click): Select none while nothing is selected, Mark owned while no selected card is unowned, Unmark owned while no selected card is owned. Select all stays active. (The greyed look is interim; `add-color-palette-setting` settles the final inactive style.)
- **Unmark owned** is shown only while Show owned gear is checked. Otherwise owned cards are hidden, so nothing it could act on can be selected. While it's hidden, Mark owned stands alone instead of forming a joined pair.
- The selected-card treatment is unchanged (the existing selection wash). The checked selection checkbox adds to it.

Behavior is otherwise unchanged: selection, Select all/none, bulk mark/unmark, the batched tracking prompt, and selection clearing all work as today.

Out of scope: showing the Group operations button's own on/off state (the spec still conveys it through the second row appearing); any restyle of the control bars themselves (the `restyle-control-bars` change).

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `prime-gear-group-operations`: "Click-to-select cards in group-operations mode" gains the selection checkbox, dimmed parts list, and card hover feedback; "Group-operations control row" shows a selection hint in place of a zero count.
- `prime-gear-group-operations` (cont.): "Group-operations control row" also shows Unmark owned only while Show owned gear is checked.
- `control-bar-visual-style`: "Control-bar buttons are outlined chips" lets Mark owned stand alone while Unmark owned is hidden.
- `prime-gear-owned-status`: "Owned overflow menu on gear card header" says the overflow-menu button is replaced by the selection checkbox while group operations is enabled, instead of being shown but unresponsive.

## Impact

- Code: `PrimeGearView.axaml` (gear-card template: checkbox/overflow swap, parts dimming), `MainWindow.axaml` (card-select overlay hover style), `PrimeGearViewModel.cs` (a selection-status text property for the hint/count).
- Tests: `PrimeGearViewModelTests` gains coverage for the selection-status text.
- No data, settings, or persistence changes. No new dependencies.
- Independent of `restyle-control-bars`. If both land, the hint and count stay plain text in the group-operations row, as that change already specifies for the count.
