## Context

Profiles and wishlists share an almost identical UX: a selector button whose `Content` is bound
to the active entry's name, a flyout list, and a shared modal (`WishlistNameDialog`) reused for
both create and rename via a `Func<string?, Task<string?>>` prompt delegate. Today the modal
only trims and rejects empty input — there is no uniqueness or length validation — and the
selector button (`Content="{Binding ...ActiveProfileName}"` / `ActiveWishlistName`) and the
footer (`StringFormat='Profile: {0}'`) render the raw name with no width bound, so long names
overflow and there is no way to read a clipped value.

The view models (`ProfileViewModel`, `WishlistViewModel`) already hold the full in-memory list
of entries (`Profiles` / `Wishlists`), so the set of existing names — the input needed for a
uniqueness check — is available without a new query. Stores persist to SQLite via EF Core; the
schema currently has no unique index on profile/wishlist names.

## Goals / Non-Goals

**Goals:**
- Profile names unique globally; wishlist names unique within a profile (trimmed,
  case-insensitive).
- A single shared maximum name length for both.
- Modal validation with an inline message that keeps the dialog open until valid or cancelled.
- Selector buttons and the footer truncate over-long names with an ellipsis and a hover tooltip
  showing the full name; the footer matches the Settings profile selector's display width.

**Non-Goals:**
- No change to how profiles/wishlists are stored, scoped, seeded, or activated.
- No rename of the "Default" profile/wishlist seeding behavior.
- No new settings to configure the length cap (it is a fixed constant).

## Decisions

### Maximum name length = 50 characters
"Reasonably long" is interpreted as **50 characters** (trimmed). Long enough for descriptive
names ("Steel Path Farming — Alt Account"), short enough to keep selectors readable. Defined as
a single shared constant referenced by both validation and the modal's `TextBox.MaxLength`.
_Alternative: 30 (too tight for descriptive names); 100 (defeats the readability goal)._

### Validate in the modal, with names passed in by the view model
Extend the prompt contract so the modal receives the current name (for rename) and the set of
taken names. The simplest shape is a small request object (e.g. `NameDialogRequest { string?
Prefill; IReadOnlyCollection<string> TakenNames; string Label }`) or two extra delegate
parameters. The modal:
- sets `NameBox.MaxLength` to the length constant,
- on text change / on OK, validates non-empty → length → not in `TakenNames` (trimmed,
  `StringComparer.OrdinalIgnoreCase`), excluding the current name so an unchanged rename passes,
- shows an inline `TextBlock` error and disables/blocks OK until valid.

The view model builds `TakenNames` from its already-loaded list: for create, all names; for
rename, all names except the active entry's. This keeps the check in memory and avoids a store
round-trip. _Alternative: validate only in the store and surface thrown exceptions — worse UX
(dialog already closed) and more error-prone._

### Unique index as a safety net (migration)
Add EF Core unique indexes: `profiles(name)` and `wishlists(profile_id, name)` to guarantee
consistency even if a future code path bypasses the modal. Because SQLite's default index is
case-sensitive and the app rule is case-insensitive, either store names already-normalized or
accept that the DB index only catches exact-case duplicates while the modal enforces the
case-insensitive rule. Given existing data may contain duplicates (the feature shipped without
the constraint), the migration must first de-duplicate or the index creation will fail; the
modal-level check is the primary guard and the index is best-effort. _Decision: ship the
modal/view-model validation as the authoritative guard; add the index only if the migration can
safely reconcile existing rows — otherwise defer the index and rely on validation._

### Truncation + hover via TextBlock with TextTrimming
Replace the selector button's string `Content` with an explicit `TextBlock` child using
`TextTrimming="CharacterEllipsis"`, a `MaxWidth`, and `ToolTip.Tip` bound to the full name. The
footer splits its current single `TextBlock` into a static "Profile: " run plus a truncating
name `TextBlock` with the same `MaxWidth` and tooltip. A shared resource/constant for the max
width keeps the footer and Settings selector in lockstep ("same length"). Avalonia shows the
tooltip on hover regardless of whether the text is actually clipped; attaching it
unconditionally to the full name is acceptable and far simpler than detecting overflow. _The
StringFormat on the footer is dropped in favor of two side-by-side TextBlocks so only the name
truncates, not the "Profile:" label._

## Risks / Trade-offs

- **Existing duplicate names already in a user's DB** → The unique index migration could fail.
  Mitigation: make validation the authoritative guard; gate the index behind a de-dup step or
  defer it. Existing duplicates remain until the user renames one, which the modal now allows
  resolving.
- **Tooltip shows even when the name is not truncated** → Minor; the full name is identical to
  the visible text, so the tooltip is redundant but not misleading. Avoids brittle overflow
  detection.
- **Prompt-delegate signature change touches both view models and `MainWindow` wiring** →
  Contained; the change is mechanical and covered by existing `ProfileViewModelTests` /
  `WishlistViewModelTests`, which should be extended for the uniqueness/length paths.
- **Case-insensitive rule vs. case-sensitive SQLite index** → Possible divergence. Mitigation:
  treat the modal check as source of truth; document the index as a coarse backstop.
