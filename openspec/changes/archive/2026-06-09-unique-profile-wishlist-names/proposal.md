## Why

Profiles and wishlists currently accept any non-empty name, so a user can create two
profiles (or two wishlists in the same profile) with identical names and cannot tell them
apart in the selectors. Names are also unbounded, and a long name silently overflows or
stretches the dropdown button and footer with no way to read the full value. Enforcing
uniqueness and a sensible length cap, plus truncating with a hover tooltip, makes the
selectors and footer reliable and readable.

## What Changes

- Profile names SHALL be unique across all profiles; wishlist names SHALL be unique within
  their owning profile. Comparison is case-insensitive on the trimmed name.
- A maximum name length SHALL be enforced for both profile and wishlist names (a single
  shared, "reasonably long" cap).
- The create/rename modal SHALL validate the entered name against the uniqueness and length
  rules and surface an inline error, keeping the dialog open until the name is valid or the
  user cancels.
- The wishlist selector button, the profile selector button (Settings), and the active
  profile footer SHALL truncate an over-long name with an ellipsis and show the full name in
  a hover tooltip. The footer name SHALL use the same display width and hover behavior as the
  Settings profile selector.

## Capabilities

### New Capabilities

(none)

### Modified Capabilities

- `player-profiles`: profile names become unique and length-capped; the create/rename modal
  validates the name; the Settings selector and the footer truncate long names with a hover
  tooltip, the footer matching the selector's display width.
- `wishlist-management`: wishlist names become unique within their profile and length-capped;
  the create/rename modal validates the name; the selector truncates long names with a hover
  tooltip.

## Impact

- Application ports: `IProfileStore` / `IWishlistStore` gain name-availability checks (or
  uniqueness is enforced in the view models that already hold the loaded name lists).
- Presentation: `WishlistNameDialog` gains validation (length + taken-name rejection with an
  inline message); `ProfileViewModel` / `WishlistViewModel` pass the existing names and the
  current name (for rename) into the prompt.
- XAML: `MainWindow.axaml` (wishlist selector button + footer) and `SettingsView.axaml`
  (profile selector button) gain bounded width, `TextTrimming`, and a tooltip bound to the
  full name.
- Storage: optional unique indexes on `profiles(name)` and `wishlists(profile_id, name)` as a
  safety net, requiring an EF Core migration; existing duplicate names (if any) must be
  reconciled or the index made consistent with the case-insensitive rule.
