## Context

The vault status flag (`IsVaulted`) is already propagated end-to-end: the SQLite reader queries a `gear_vault_status` view, `PrimeGearEntry` carries the flag, and `PrimeGearCardViewModel` exposes `IsVaulted`. The current `PrimeGearViewModel` holds a single flat `Cards` collection used by a single `WrapPanel` in the AXAML. The `.vaulted` style (reduced opacity) and a "Vaulted" label are already applied per-card.

This change restructures the presentation only — no changes below the view-model layer.

## Goals / Non-Goals

**Goals:**
- Split `PrimeGearViewModel.Cards` into `ActiveCards` (non-vaulted) and `VaultedCards`.
- Expose a `ShowVaulted` bool property (default `false`) on `PrimeGearViewModel` with `INotifyPropertyChanged`.
- Lay out the Prime Gear tab as: active cards section → separator → vaulted cards section (visibility bound to `ShowVaulted`).
- Make the separator visually distinct (background fill, label text, checkbox).

**Non-Goals:**
- Persisting the `ShowVaulted` preference across sessions.
- Sorting or filtering cards within each section beyond what the reader already returns.
- Any changes to infrastructure, application ports, or tests.

## Decisions

### Two collections on the view-model

`ActiveCards` and `VaultedCards` are populated by splitting entries on `IsVaulted` during `LoadAsync`. Both are `ObservableCollection<PrimeGearCardViewModel>` so the existing `ReloadAsync` (clear + reload) pattern continues to work unchanged.

**Alternative considered**: Keep a single `Cards` collection and filter in AXAML with a predicate converter. Rejected — Avalonia's `ItemsControl` does not have built-in filtering; a converter approach would require a custom `IValueConverter` that re-evaluates on every item, and the predicate would need to be re-applied on `ShowVaulted` change. Two collections are simpler and explicit.

### Separator as a styled `Border` in a `StackPanel`

The Prime Gear tab content becomes a vertical `StackPanel` with three children:
1. `ItemsControl` + `WrapPanel` for active cards.
2. A `Border` separator containing a `StackPanel` with a label and `CheckBox`.
3. `ItemsControl` + `WrapPanel` for vaulted cards, with `IsVisible` bound to `ShowVaulted`.

The separator `Border` uses a distinct background (e.g. subtle tinted fill) and padding to stand apart from the card areas.

**Alternative considered**: `Expander` control for the vaulted section. The `Expander` header would serve as the separator, but its default styling does not match the "visually distinct separator row" requirement and is harder to customise in Avalonia without a full control template override.

### `ShowVaulted` default

Defaults to `false` — vaulted section hidden on launch. This keeps the primary farming view uncluttered; users opt in to see vaulted gear.

## Risks / Trade-offs

- [Risk] On reload (`ReloadAsync`), clearing both collections and repopulating may briefly flash an empty state → Acceptable; reloads only happen after a successful import, not during normal navigation.
- [Risk] If all gear is vaulted, the active section is empty and the UI shows only the separator and hidden vaulted section → Acceptable; an empty active section with the separator is honest and correct.
