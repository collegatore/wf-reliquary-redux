## 1. Shared name constants

- [x] 1.1 Add a shared constant for the maximum name length (50) usable by the modal and view models (e.g. a `NameRules` static or const on the dialog).
- [x] 1.2 Add a shared resource/constant for the selector + footer max display width so the footer and Settings profile selector stay in lockstep.

## 2. Modal validation (WishlistNameDialog)

- [x] 2.1 Extend the dialog API to accept the current name (for rename) and the set of taken names, plus the existing label/prefill (e.g. a `NameDialogRequest` or extra parameters on `ShowAsync`).
- [x] 2.2 Set `NameBox.MaxLength` to the length constant.
- [x] 2.3 Add an inline error `TextBlock` in `WishlistNameDialog.axaml` (hidden by default).
- [x] 2.4 Validate on text change and on OK: non-empty → within length → not a duplicate (trimmed, `OrdinalIgnoreCase`), excluding the current name; show the inline message and block OK while invalid; clear the message when corrected.

## 3. View model wiring (uniqueness inputs)

- [x] 3.1 In `WishlistViewModel`, build the taken-names set from `Wishlists` (all names for create; all except the active entry for rename) and pass it into the prompt.
- [x] 3.2 In `ProfileViewModel`, build the taken-names set from `Profiles` the same way and pass it into the prompt.
- [x] 3.3 Update `MainWindow.axaml.cs` wiring of both prompt delegates to the new dialog signature (wishlist + profile labels).

## 4. Truncation + hover in the UI

- [x] 4.1 In `MainWindow.axaml`, replace the wishlist selector button's string `Content` with a `TextBlock` using `TextTrimming="CharacterEllipsis"`, the shared `MaxWidth`, and `ToolTip.Tip` bound to `Wishlist.ActiveWishlistName`.
- [x] 4.2 In `SettingsView.axaml`, do the same for the profile selector button bound to `Profiles.ActiveProfileName`.
- [x] 4.3 In `MainWindow.axaml` footer, split into a static "Profile: " run plus a truncating name `TextBlock` (same `MaxWidth` + `ToolTip.Tip`), dropping the `StringFormat`.

## 5. Storage safety net (optional index + migration)

- [x] 5.1 Add EF Core unique indexes `profiles(name)` and `wishlists(profile_id, name)` in `ReliquaryDbContext.OnModelCreating` (only if existing data can be reconciled; otherwise document as deferred).
- [x] 5.2 Generate and review the EF Core migration; ensure it handles/guards against pre-existing duplicate names.

## 6. Tests & verification

- [x] 6.1 Extend `WishlistViewModelTests` for: duplicate-within-profile rejected, same name allowed across profiles, no-op self-rename allowed.
- [x] 6.2 Extend `ProfileViewModelTests` for: duplicate profile name rejected, no-op self-rename allowed.
- [x] 6.3 Add/extend tests for length-limit rejection and inline validation messaging where the dialog logic is testable.
- [x] 6.4 Run the app to verify: long names truncate with a hover tooltip in both selectors and the footer (footer width matches the Settings selector); duplicate/over-long names are blocked in the modal with an inline message.
