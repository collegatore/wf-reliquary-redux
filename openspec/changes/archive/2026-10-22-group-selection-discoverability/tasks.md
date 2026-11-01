# Tasks

## 1. Selection status text

- [x] 1.1 Add `SelectionStatusText` to `PrimeGearViewModel` ("Click cards to select them" at zero, "{n} selected" otherwise), raised from the `SelectedCount` setter (D4); verify `dotnet build` succeeds
- [x] 1.2 Add tests to `PrimeGearViewModelTests`: the hint shows when group mode is enabled with nothing selected, the count text shows after selecting cards, and the hint returns after Select none and after a bulk action empties the selection; verify `dotnet test` passes
- [x] 1.3 In `PrimeGearView.axaml`, bind the group row's status `TextBlock` to `SelectionStatusText` (replacing the `StringFormat` binding), sized by an invisible copy of the hint in the same grid cell so its width never changes; verify in the running app that the row reads "Click cards to select them" on entering group mode, switches to "1 selected" on the first click, and the action buttons don't shift

## 2. Gear card in group mode

- [x] 2.1 In `GearCardTemplate`, add the display-only selection `CheckBox` (`IsChecked` one-way to `IsSelected`, not hit-testable) and swap its visibility with the `⋮` button on group mode, keeping the right header column at a fixed width; update the stale template comment (D1); verify that toggling group mode swaps `⋮` and the checkbox without the header shifting, and that clicking the checkbox or elsewhere on the card toggles selection exactly once
- [x] 2.2 Add the `group-mode` class binding to the `Parts` `ItemsControl` and an `ItemsControl.group-mode` opacity style (D2); verify the parts list dims on entering group mode and restores on leaving, including on an owned card with Show owned gear on
- [x] 2.3 Extend the `Button.card-select-overlay:pointerover` style in `MainWindow.axaml` with a 2px `SystemAccentColor` border and corner radius 4 on the template's content presenter, keeping the background transparent (D3); verify the outline traces the card edge on hover in group mode, never appears outside group mode, and doesn't look like the selection wash

- [x] 2.4 Show Unmark owned only while Show owned gear is checked, binding the Mark/Unmark group's `joined` class to the same value (D5); add a `prime-gear-owned-hidden` snapshot scene; verify that with Show owned gear off, Mark owned is a standalone pill with full outline and rounded corners, and with it on, the pair is joined

- [x] 2.5 Make Select none, Mark owned, and Unmark owned inactive while nothing is selected (`CanExecute`, refreshed when the count crosses zero) and grey out disabled bar buttons (`Opacity` 0.45) (D6); add `PrimeGearViewModelTests` for the inactive/active transitions; verify `dotnet test` passes, the `prime-gear-group-mode` snapshot shows the three actions greyed with Select all active, and a disabled bar button gets no hover or click

- [x] 2.6 Make Mark owned inactive unless a selected card is unowned, and Unmark owned inactive unless a selected card is owned, re-evaluated on every selection change (D6); replace the transitions test with tests for owned-only, unowned-only, and mixed selections and for re-evaluation on each change; verify `dotnet test` passes and the `prime-gear-group-mode-selected-hover` snapshot (an unowned card selected) shows Mark owned active and Unmark owned greyed

## 3. Integration check

- [x] 3.1 Walk the group-operations flow in the running app: enable the mode, select cards by clicking bodies and checkboxes, Select all / Select none, bulk Mark owned (including the batched prompt and the fade-out with Show owned gear off) and Unmark owned, then disable the mode. Confirm every behavior in the `prime-gear-group-operations` and `prime-gear-owned-status` specs still holds and the `⋮` menu works again afterwards; run `dotnet test` and verify it passes (Confirmed by the user in the running app, 2026-10-22.)
