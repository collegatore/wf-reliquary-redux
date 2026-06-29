## Context

The Prime Gear tab renders gear as cards in two sections (Active, Vaulted). Each card has a `⋮` overflow button whose flyout holds a single action — *Mark as owned* / *Unmark as owned* (`PrimeGearCardViewModel.MarkOwnedCommand`). Owned state is persisted per profile via `IOwnedGearStore.SetOwnedAsync(gearName, owned)`. Marking a card owned that still has *checked* (wishlisted) parts pops a three-way confirmation dialog (`HandleOwnTransitionAsync`: Cancel / Keep tracking / Release tracking). With **Show owned** off, marking a card owned fades and collapses it (`OnCardOwnedChangedAsync`). `ApplyFilters` is the single chokepoint for all view filtering (search debounce, Show vaulted, Show owned) and sets per-card `IsShown`.

Two problems motivate this change:
1. Seeding owned state for an experienced player is a per-card, one-at-a-time chore.
2. Owned and vaulted gear share the *same* visual channel — both are dimmed to ~0.55 opacity (`Border.owned` and `Border.gear-vaulted` in `MainWindow.axaml`), so a vaulted card marked owned shows no change and an owned-vaulted card cannot express both facts.

The card template is currently **duplicated** in `PrimeGearView.axaml` (the Active and Vaulted `ItemsControl` blocks are near-identical).

## Goals / Non-Goals

**Goals:**
- Let the user multi-select gear cards and bulk Mark/Unmark owned in a few clicks.
- Make owned and vaulted visually distinct so bulk actions are verifiable at a glance.
- Keep the per-card (non-group) experience unchanged.
- Reuse the existing owned-gear store and the existing fade/collapse behavior.

**Non-Goals:**
- No new bulk store method or schema change (bulk = loop over the existing single-row `SetOwnedAsync`).
- No general "group operations framework" beyond owned mark/unmark — the second row is owned-specific for now.
- No persistence of the group-operations toggle (it is intentionally ephemeral).
- No change to which gear is vaulted/owned, only how they are presented and acted on.

## Decisions

### Group-operations mode is ephemeral view state on `PrimeGearViewModel`
`IsGroupMode` is a plain bound bool, defaulting to false and never persisted. Group mode is a transient "maintenance" stance, unlike Show owned/vaulted which persist. Turning it off clears the selection.
- *Alternative considered*: persist it like the other toggles. Rejected — it is a momentary action mode, not a viewing preference.

### Per-card selection is a new flag, distinct from owned and from part-checked
`PrimeGearCardViewModel` gains `IsSelected` (bound), unrelated to `IsOwned` and to the per-part `IsChecked`. Selection is made by **clicking the whole card**: in group mode a transparent full-card overlay `Button` (bound to `IsGroupMode` via ancestor binding) covers the card, invokes the card's `ToggleSelectCommand`, and — by sitting on top — suppresses the card's own controls (part checkboxes, `⋮`). The selected state shows as a subtle background wash (`Border.gear-card.selected`). The `⋮` button is no longer swapped out; it simply becomes non-interactive under the overlay.
- *Alternatives considered*: a selection `CheckBox` replacing `⋮` (rejected on use — three checkbox-like affordances on one card was confusing, and a single hit target is faster to click through); disabling each inner control individually (rejected — the overlay achieves it in one element and gives a single, reliable click target).

### Bulk actions live on the view model and act on the current selection
`PrimeGearViewModel` exposes `SelectedCount` and commands `SelectAllCommand`, `SelectNoneCommand`, `MarkSelectedOwnedCommand`, `UnmarkSelectedCommand`. **Select all** sets `IsSelected` on every card whose `IsShown` is true (visible cards only, across both sections). Bulk Unmark loops `SetOwnedAsync(name, false)` over selected owned cards and re-runs the owned-changed path. Two explicit verbs (Mark / Unmark) rather than a toggle, because a mixed selection has no single toggle target.

### Bulk Mark asks about tracked parts exactly once
The per-card `confirmOwned` dialog cannot fire per card in a batch. The bulk Mark path inspects the selection for cards with tracked parts; if any exist it invokes a **single** batched prompt (Keep tracking / Release tracking / Cancel) and applies the choice to the whole batch:
- **Cancel** → abort the entire bulk Mark (no card is marked).
- **Keep tracking** → mark all selected owned, leave wishlist items intact.
- **Release tracking** → uncheck (and persist removal of) every tracked part on the selected cards, then mark all owned.
If no selected card has tracked parts, no prompt is shown. This reuses the existing `OwnedConfirmChoice` enum; the view supplies a batch confirm hook (`Func<int, Task<OwnedConfirmChoice>>`) alongside the existing single-card one.
- *Alternatives considered*: prompt per card (unusable at scale); always keep tracking silently (safe but removes user agency); skip cards with parts (surprising). The once-and-apply-all choice matches the "I own everything" workflow.

### Selection clears on any change to the visible set
Selection only ever references visible cards, so a stale hidden selection must not survive a view change. Clearing is driven by **user-initiated** view changes: `SearchText` change, `ShowVaulted` / `ShowOwned` toggles, and active-wishlist switch (which rebuilds card VMs and drops selection for free). Leaving group mode also clears. Bulk actions naturally end with an empty selection because the acted-on cards are consumed (faded out or deselected); we do not rely on `ApplyFilters` itself to clear, since it is also invoked internally mid-batch.

### Owned and vaulted get separate visual channels
- **Owned**: keep the existing dim (opacity 0.55) **and** add an unintrusive green outline (`OwnedAccentBrush` `#5E9E6E`) plus a `✓` marker placed immediately *after* the gear name (mirroring the existing `⚠` ghost-part glyph). The `.owned` style therefore carries both opacity and border brush.
- **Vaulted**: remove the opacity setter from `Border.gear-vaulted` (and drop the now-inert class binding); vaulted is conveyed solely by the dedicated Vaulted section/header. This eliminates the owned↔vaulted collision: an owned-vaulted card is a dimmed, gold-outlined, ✓-marked card sitting under the Vaulted header.

### Move the base border brush out of the inline attribute
The card `Border` sets `BorderBrush="Gray"` inline. In Avalonia a local (inline) value beats a style-class setter, so a `.owned { BorderBrush }` override would be silently ignored. The base gray border must move into a `Border` style (or base class) so the `.owned` gold override wins.

### Factor the duplicated card template into a shared `DataTemplate`
The Active and Vaulted card templates are duplicated. The new card chrome (checkbox/`⋮` swap, ✓ marker, gold outline) must land in both. Extract a single shared `DataTemplate` resource keyed on `PrimeGearCardViewModel` and reference it from both `ItemsControl`s to prevent drift.

## Risks / Trade-offs

- **Bulk = N store writes** → Acceptable for V1 (a few dozen at most); reuses proven single-row writes. A batch store method is a possible later optimization, not needed now.
- **Vaulted no longer dimmed may look like active gear** → Mitigated by the always-present Vaulted section header and section grouping; this is the user's explicit preference.
- **Group-mode click target vs. inner controls** → A single transparent overlay intercepts all clicks in group mode, so part checkboxes and `⋮` cannot be triggered accidentally; the overlay is collapsed (and non-hit-testable) when group mode is off, so normal interaction is unaffected.
- **Inline→style border move could shift the default look** → Visual regression check that non-owned cards still show the gray border at the same thickness.
- **Bulk Mark with Show owned off fades many cards at once** → This is intended feedback; the fade animation already exists and is reused unchanged.

## Open Questions

None — all design decisions were resolved during exploration.
