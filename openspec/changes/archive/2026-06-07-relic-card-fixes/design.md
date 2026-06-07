## Context

Relic cards in `MainWindow.axaml` are rendered via two view model types: `WishlistRelicTierGroupViewModel` (one per tier) and `WishlistRelicRewardViewModel` (one per reward item). The AXAML DataTemplate for each tier group hard-codes a `<Separator>` after every group, and the DataTemplate for each reward unconditionally applies the `wishlisted` CSS class based on `IsWishlisted`.

Both issues are pure presentation-layer bugs — no data layer or application logic changes are needed.

## Goals / Non-Goals

**Goals:**
- Bold reward text only when Show all rewards is ON (i.e., when the distinction between wishlisted and non-wishlisted items is visible).
- Suppress the tier-group separator for the last group in a card.

**Non-Goals:**
- Changing any business logic, data queries, or application-layer types.
- Altering the farming missions panel or any other part of the UI.

## Decisions

**Decision: Add `ShowWishlistedStyle` to `WishlistRelicRewardViewModel` rather than binding to a parent-level flag in AXAML**

Cards are fully rebuilt by `RebuildCards()` whenever `ShowAllRewards` changes, so passing the flag at construction time costs nothing. A computed property on the reward VM keeps the AXAML binding simple (`Classes.wishlisted="{Binding ShowWishlistedStyle}"`) and avoids needing `MultiBinding` or a converter. Alternative — passing `ShowAllRewards` down through a binding chain in AXAML — would complicate the template and require a converter or ancestor lookup.

**Decision: Add `IsLast` to `WishlistRelicTierGroupViewModel`, set by `WishlistRelicCardViewModel` after building the group list**

`ItemsControl` in Avalonia has no built-in "is last item" concept. The cleanest solution is to flag the last item explicitly on the VM. The setter is `internal` so only the card VM can mutate it. Alternative — using an `IValueConverter` with the items count — is more complex and less readable.

## Risks / Trade-offs

- [`IsLast` flag must be kept in sync] → `WishlistRelicCardViewModel` sets `IsLast` immediately after building `TierGroups`; since `TierGroups` is populated once at construction and never mutated, this is a one-time operation with no ongoing sync burden.
- [Separator visibility binding `IsVisible="{Binding !IsLast}"` requires Avalonia negation support] → Avalonia supports the `!` prefix on boolean bindings, so no converter is needed.

## Migration Plan

No migration required. Changes are isolated to the presentation layer and take effect at next build.
