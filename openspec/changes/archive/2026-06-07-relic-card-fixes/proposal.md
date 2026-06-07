## Why

Two visual defects in relic cards make the UI misleading: wishlisted-part bold styling is applied even when it carries no information (when only wishlisted rewards are visible), and each tier group renders a separator below it — including the last one, leaving an orphaned line at the bottom of every card.

## What Changes

- Bold highlighting on reward items is conditional on **Show all rewards** being active (highlighting only makes sense when non-wishlisted items are also visible).
- The separator between tier groups is suppressed for the last group in each card so delimiters appear only between tiers, not after the final one.

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `wishlist-relics-view`: Two requirement changes to the relic card rendering rules — wishlisted highlight is conditional on Show all rewards; last tier group has no trailing separator.

## Impact

- `WishlistRelicsViewModel.cs` — `WishlistRelicRewardViewModel` gains a computed property for whether to apply the wishlisted style; `WishlistRelicTierGroupViewModel` gains an `IsLast` flag set by `WishlistRelicCardViewModel` during construction.
- `MainWindow.axaml` — `Classes.wishlisted` binding updated; separator `IsVisible` bound to `!IsLast`.
