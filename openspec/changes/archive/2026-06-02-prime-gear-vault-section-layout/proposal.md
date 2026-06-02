## Why

Vaulted and non-vaulted prime gear are actionable in very different ways: non-vaulted items can be actively farmed right now, while vaulted items cannot. Mixing them in a single flat card grid forces the user to visually filter, making the view harder to scan. The `IsVaulted` flag is already propagated through the data and view-model layers; only the presentation layout needs to change.

## What Changes

- The Prime Gear tab cards area is split into two distinct sections: **Active** (non-vaulted) at the top, **Vaulted** at the bottom.
- A visually distinct separator row sits between the two sections. It contains a checkbox labelled **Show vaulted** that toggles visibility of the Vaulted section.
- The Vaulted section is hidden by default when the checkbox is unchecked.
- `PrimeGearViewModel` exposes two separate card collections (`ActiveCards`, `VaultedCards`) and a `ShowVaulted` toggle property instead of the single flat `Cards` collection.

## Capabilities

### New Capabilities

<!-- None -->

### Modified Capabilities

- `prime-gear-parts-view`: The cards display requirement changes — cards are now split into two spatially separate sections by vault status, with a separator and show/hide toggle between them.

## Impact

- **Presentation (`PrimeGearViewModel`)**: Replace single `Cards` collection with `ActiveCards`, `VaultedCards`, and `ShowVaulted` bool property.
- **Presentation (`MainWindow.axaml`)**: Replace single `ItemsControl`/`WrapPanel` with a `StackPanel` containing the active section, a styled separator, and the vaulted section.
- **No changes** to application ports, infrastructure, composition, or tests.
