## Why

The relic cards on the Relics and Missions tab waste space: every card in a WrapPanel row takes the height of the tallest card, and the verbatim reward names force wide cards and multi-row tier groups. Field use showed the design's governing use case — matching a reward name against the in-game reward screen in ~5–10 seconds — does not actually happen on this tab: the player cannot know which relics were cracked, so reward matching happens on the wishlist screen instead. The relics panel's real job is **planning and scope overview**, where density and a noise-free layout matter more than verbatim fidelity.

## What Changes

- Add a user-selectable **relic view mode** with three options: **Verbatim**, **Compact**, and **Table**, chosen in the **Appearance window** (File → Appearance) alongside the layout orientation control, persisted across sessions (same pattern as the layout orientation setting). The modeless Appearance window's live preview lets modes be compared side by side against the main window.
- **Verbatim** mode preserves the current card exactly as-is (tier groups, accent bars, verbatim names) so modes can be compared side by side.
- **Compact** mode renders cards with clipped wording and no per-tier label rows:
  - "Relic" clipped from the card header; "Prime" clipped from part names; "Blueprint" clipped from component blueprints but kept on main blueprints ("Revenant Prime Blueprint" → "Revenant Blueprint"; "Revenant Prime Neuroptics Blueprint" → "Revenant Neuroptics").
  - Tier conveyed by a trailing letter marker `(C)`/`(U)`/`(R)` on each reward line (letter, not drop chance — chances depend on refinement, which has no concept in the app).
- **Table** mode renders one row per relic using the Avalonia **TreeDataGrid** control: clipped relic name, wishlisted-tier markers (e.g. "(C, R)"), and two pictogram actions — one opening a modal with the Verbatim card in all-rewards mode, one opening the existing LFG message composer. Rows expand into child reward rows in compact wording.
- The **Show all rewards** checkbox continues to apply in Verbatim and Compact modes and is hidden in Table mode.
- **Revoke the verbatim-names design rule**: `docs/card-display-design.md` is rewritten — the governing use case for this tab becomes planning/overview; the reward-match use case is recorded as living on the wishlist screen; Verbatim mode's rationale is framed as historical/preserved-for-comparison.
- Mission cards are **out of scope**; relics panel only.

## Capabilities

### New Capabilities

- `relic-view-modes`: the three-way view mode selector (Verbatim / Compact / Table), its persistence, the Compact card composition with word-clipping rules and tier letter markers, and the Table mode (TreeDataGrid rows, expansion into reward rows, wishlisted-tier markers, rewards modal and LFG pictogram actions).

### Modified Capabilities

- `wishlist-relics-view`: the "reward names rendered verbatim" requirement is scoped to Verbatim mode only (no longer a tab-wide rule); the Show all rewards checkbox is hidden while Table mode is active; the existing card-rendering requirements are scoped as describing Verbatim mode.
- `appearance-window`: the window's content gains the relic view mode selector (Verbatim / Compact / Table) alongside the existing font size, card width, and orientation controls; the existing live-preview behavior covers it.

## Impact

- **Presentation**: `RelicsMissionsView.axaml` (+ code-behind) — per-mode content (card templates / TreeDataGrid), Show-all-rewards visibility; `AppearanceWindow.axaml` / `AppearanceViewModel.cs` — the view mode selector; `WishlistRelicsViewModel.cs` — view mode state, compact name transforms (presentation-layer computed properties; domain reward names untouched), table row/child view models; a new rewards modal (precedent: LFG dialog).
- **Settings**: `UserSettings.cs`, `LocalUserSettingsStore.cs` — persist the selected view mode.
- **Dependencies**: new NuGet package `Avalonia.Controls.TreeDataGrid` (official Avalonia package); its theme must be included in `App.axaml`.
- **Docs**: `docs/card-display-design.md` rewritten (governing use case, mode catalog, historical framing of verbatim rationale).
- **Tests**: integration tests for mode persistence, compact name clipping rules, table row composition, and control-bar visibility behavior.
