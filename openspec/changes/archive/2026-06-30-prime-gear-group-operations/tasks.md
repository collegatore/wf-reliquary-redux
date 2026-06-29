## 1. Visual status separation (owned vs vaulted)

- [x] 1.1 In `MainWindow.axaml`, remove the opacity setter from `Border.gear-vaulted` so vaulted cards are no longer muted by vaulted status
- [x] 1.2 Update `Border.owned` to keep opacity 0.55 and add an unintrusive green outline using `OwnedAccentBrush` (`#5E9E6E`) — add a base `Border` style (or base card class) carrying the default gray border so the `.owned` brush override wins over what is currently the inline `BorderBrush="Gray"`
- [x] 1.3 Remove the now-inert `Classes.gear-vaulted="True"` binding from the Vaulted card template (keep the `fading` class behavior intact)
- [x] 1.4 Add a non-interactive `✓` marker immediately after the gear name, visible when `IsOwned`, styled in the unintrusive green `OwnedAccentBrush` (mirror the existing `⚠` ghost-part glyph)
- [x] 1.5 Verify non-owned cards still render the gray border at the original thickness after moving the brush out of the inline attribute

## 2. Card template consolidation

- [x] 2.1 Extract the duplicated Active/Vaulted card body into a single shared `DataTemplate` resource keyed on `PrimeGearCardViewModel`
- [x] 2.2 Reference the shared template from both the Active and Vaulted `ItemsControl`s; confirm both sections render unchanged

## 3. Selection state on the view models

- [x] 3.1 Add an `IsSelected` bound bool to `PrimeGearCardViewModel`, independent of `IsOwned` and part `IsChecked`
- [x] 3.2 Add `IsGroupMode` (ephemeral, default false), `SelectedCount`, and a way for the card template to observe `IsGroupMode` (ancestor binding to `PrimeGearViewModel`)
- [x] 3.3 Keep `SelectedCount` in sync as individual cards toggle `IsSelected`
- [x] 3.4 Clear the selection when `IsGroupMode` is turned off

## 4. Group-operations UI

- [x] 4.1 Add the **Enable group operations** toggle to the control panel (row 1), bound to `IsGroupMode`
- [x] 4.2 Make the whole card a selection target in group mode: a transparent full-card overlay `Button` (visible only when `IsGroupMode`) bound to the card's `ToggleSelectCommand`, which also suppresses the inner controls; the `⋮` menu stays in place. Mark selected cards with a subtle background wash (`Border.gear-card.selected`)
- [x] 4.3 Add the second control-panel row, visible only when `IsGroupMode`: selection count, **Select all**, **Select none**, **Mark owned**, **Unmark owned**

## 5. Bulk commands

- [x] 5.1 Implement `SelectAllCommand` to select every card whose `IsShown` is true (both sections); `SelectNoneCommand` to clear all `IsSelected`
- [x] 5.2 Implement `UnmarkSelectedCommand` to clear owned on each selected owned card via `IOwnedGearStore.SetOwnedAsync(name, false)` and re-run the owned-changed path
- [x] 5.3 Implement `MarkSelectedOwnedCommand`: detect selected cards with wished parts; if any, invoke a single batched confirm hook (`Func<int, Task<OwnedConfirmChoice>>`); otherwise mark directly
- [x] 5.4 Apply the batched choice to the whole selection — Keep tracking (mark all owned), Release tracking (uncheck/persist removal of each selected card's wished parts, then mark all owned), Cancel (mark none, selection unchanged)
- [x] 5.5 Ensure bulk-marked cards reuse the existing fade/collapse (`OnCardOwnedChangedAsync`) when Show owned is off, and that the selection ends empty after a completed bulk action
- [x] 5.6 Wire the batched confirm hook in `PrimeGearView` (reuse the `OwnedGearTrackingDialog` / `OwnedConfirmChoice` flow, keyed on selection count)

## 6. Selection lifetime

- [x] 6.1 Clear the selection on user-initiated view changes: `SearchText` change, `ShowVaulted` toggle, `ShowOwned` toggle (driven from those setters, not from internal `ApplyFilters` calls)
- [x] 6.2 Confirm switching the active wishlist drops the selection (card VMs are rebuilt by `ReloadAsync`)

## 7. Tests

- [x] 7.1 Selection: toggling `IsSelected`, `SelectedCount`, Select all selects only visible cards, Select none clears
- [x] 7.2 Bulk unmark: all selected owned cards cleared and persisted
- [x] 7.3 Bulk mark with no wished parts: marks directly, no prompt; with wished parts: single prompt, and Keep/Release/Cancel each apply to the whole batch
- [x] 7.4 Selection clears on search change, Show vaulted toggle, Show owned toggle, and group-mode exit
- [x] 7.5 Bulk mark with Show owned off fades/collapses cards and leaves the selection empty

## 8. Verification

- [x] 8.1 Run the full build and test suite; fix any regressions
- [x] 8.2 Confirm owned, vaulted, and owned-and-vaulted cards are each visually distinct, and that bulk-marking vaulted gear owned produces a visible change
