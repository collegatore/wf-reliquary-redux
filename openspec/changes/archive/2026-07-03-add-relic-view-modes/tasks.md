## 1. Settings & view-mode state

- [x] 1.1 Add `RelicViewMode` enum (Verbatim / Compact / Table) in the Application layer next to `RelicsMissionsOrientation`
- [x] 1.2 Add `RelicViewMode ViewMode = RelicViewMode.Verbatim` to the `UserSettings` record; round-trip it in `LocalUserSettingsStore` (existing settings files must still deserialize, falling back to Verbatim)
- [x] 1.3 Add `ViewMode` property + `IsViewModeVerbatim`/`IsViewModeCompact`/`IsViewModeTable` derived properties to `WishlistRelicsViewModel`
- [x] 1.4 Add the relic view mode selector to `AppearanceWindow.axaml` / `AppearanceViewModel`, following the layout-orientation precedent end to end: persist via the settings store and push the value to `WishlistRelicsViewModel.ViewMode` (live preview)
- [x] 1.5 Integration tests: mode persistence round-trip, legacy-settings fallback to Verbatim, and AppearanceViewModel → WishlistRelicsViewModel propagation

## 2. Compact wording transform

- [x] 2.1 Implement the clipping transform (presentation layer, pure function): strip trailing " Relic" from relic names; remove " Prime" from reward names; strip trailing " Blueprint" unless the original ends with "Prime Blueprint"
- [x] 2.2 Unit tests pinning the spec scenarios: "Axi A15 Relic"→"Axi A15", "Revenant Prime Neuroptics Blueprint"→"Revenant Neuroptics", "Revenant Prime Blueprint"→"Revenant Blueprint", "Forma Blueprint"→"Forma"
- [x] 2.3 Extend `WishlistRelicRewardViewModel` with tier, `CompactName`, and `TierLetter`; expose a flat tier-ordered `Rewards` list on `WishlistRelicCardViewModel` alongside `TierGroups`

## 3. Relics panel extraction & control-bar behavior

- [x] 3.1 Extract the relics panel (control bar + cards scroll content) from the stacked and side-by-side subtrees of `RelicsMissionsView.axaml` into a shared UserControl; re-verify side-by-side width wiring in code-behind after extraction
- [x] 3.2 Hide the "Show all rewards" checkbox (and label) in the control bar while Table mode is active, retaining its checked state
- [x] 3.3 Integration tests: Show all rewards visibility toggles with Table mode and its checked state survives the Table-mode round-trip

## 4. Compact card mode

- [x] 4.1 Add the Compact card `DataTemplate` (card border, clipped header + ⋮ flyout, `RelicCardWidth` binding, vaulted class; body = flat reward lines with per-line tier accent bar and trailing tier letter, dimming as in Verbatim) and render it as a sibling subtree toggled by `IsViewModeCompact`
- [x] 4.2 Integration tests: compact card composition (no tier label rows, tier letters present, Show all rewards dimming applies)

## 5. Table mode (TreeDataGrid)

- [x] 5.1 Add the `Avalonia.Controls.TreeDataGrid` package pinned to the app's Avalonia minor version; include its theme in `App.axaml`; smoke-verify it renders under the app theme and font-size settings before building on it
  - Deviation: official 12.x TreeDataGrid is now a paid Avalonia Accelerate product (`AVLIC0001`); per user decision, used the free community fork `Caly.Avalonia.Controls.TreeDataGrid` 12.0.1.3 (same assembly name / API / theme path). See design.md Decision 5.
  - Smoke-verified via build (theme include compiles, `<TreeDataGrid>` resolves, source/columns build). Visual render under the running app not yet checked (app-launch pending user approval).
- [x] 5.2 Add table row view models: relic row (clipped name, wishlisted-tier markers "(C, R)" in C→U→R order, vaulted flag, expandable child reward rows) and reward row (compact name + tier letter, dimmed when not wishlisted); rows ordered era→alphabetical; `ShowVaulted` filter applies
- [x] 5.3 Build the `HierarchicalTreeDataGridSource` with expander/name column, markers column, and two pictogram `TemplateColumn`s (empty cells on child rows); style vaulted-row opacity and reward dimming; render as the `IsViewModeTable` subtree
- [x] 5.4 Integration tests: row composition and ordering, wishlisted-tier markers, expansion shows all rewards with dimming, vaulted filtering/styling

## 6. Row actions

- [x] 6.1 Rewards modal: dialog window (LFG-dialog precedent) hosting the Verbatim card template bound to an all-rewards `WishlistRelicCardViewModel` for the row's relic; wire the ⊞ pictogram
- [x] 6.2 Wire the 💬 pictogram to the existing `OpenLfgCompose` for the row's relic
- [x] 6.3 Integration tests: pictogram commands open the right dialog for the right relic; closing the modal leaves table state untouched

## 7. Documentation

- [x] 7.1 Rewrite `docs/card-display-design.md`: governing use case becomes planning/overview; record that the reward-match use case lives on the wishlist screen; frame Verbatim mode's rationale as historical/preserved-for-comparison; document the three modes, the clipping rule, and the TreeDataGrid decision
- [x] 7.2 Update `docs/technical-reference.md` if it enumerates settings or tab controls affected by this change
