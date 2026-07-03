## Context

The relic cards panel on the Relics and Missions tab renders `WishlistRelicCardViewModel` items in a `WrapPanel`, so every card in a row takes the height of the tallest card, wasting vertical space. The card layout was governed by a "verbatim reward names" rule (docs/card-display-design.md) derived from a ~5–10s in-game reward-match use case. Field use showed that use case does not occur on this tab — the player cannot know which relics were cracked, so matching happens on the wishlist screen. This change re-frames the tab as a planning/overview surface and introduces three user-selectable view modes: **Verbatim** (current card, preserved for comparison), **Compact** (clipped wording, one line per reward), and **Table** (one row per relic with expandable reward rows).

Relevant precedents in the codebase:
- `UserSettings` record with defaulted parameters and the `RelicsMissionsOrientation` enum (Application layer) for persisted UI choices.
- The stacked / side-by-side layout switch in `RelicsMissionsView.axaml`: sibling subtrees toggled by `IsVisible` bindings.
- LFG compose dialog as the modal-window precedent; `OpenLfgCompose` is already wired per relic card.
- `RelicCardWidth` dynamic resource scaling card width with the font size.

## Goals / Non-Goals

**Goals:**
- Three view modes selectable from the Appearance window (alongside the layout orientation control), persisted across sessions, default **Verbatim** (current behavior preserved on upgrade). The modeless window's live preview supports the side-by-side mode comparison this change exists for. *(Later revised: after the comparison, the user switched the default to **Compact** — including the legacy-settings fallback.)*
- Compact card composition with presentation-layer word clipping and trailing `(C)/(U)/(R)` tier markers. *(Later revised to leading, bold, unparenthesized markers — see Decision 3's follow-up.)*
- Table mode built on Avalonia's official `TreeDataGrid`: relic rows → expandable child reward rows, wishlisted-tier markers, rewards-modal and LFG pictogram actions.
- Rewrite `docs/card-display-design.md`: governing use case becomes planning/overview; the reward-match use case is recorded as living on the wishlist screen; Verbatim rationale framed as historical.

**Non-Goals:**
- Mission cards keep their current layout and controls (out of scope).
- No masonry/column layout for card modes — the tallest-card row-height waste in Verbatim/Compact card modes is accepted; Table mode is the density answer.
- No refinement concept / drop-chance display (tier letter markers only).
- No changes to domain or stored reward names — clipping is presentation-only.

## Decisions

### 1. View mode as an Application-layer enum persisted in UserSettings

`RelicViewMode { Verbatim, Compact, Table }` lives in the Application layer next to `RelicsMissionsOrientation`, added to the `UserSettings` record as a defaulted parameter (`RelicViewMode.Verbatim`) so existing settings files deserialize unchanged. `LocalUserSettingsStore` round-trips it like the orientation.

The selector lives in the **Appearance window**, following the layout-orientation precedent end to end: a control in `AppearanceWindow.axaml` bound through `AppearanceViewModel`, which persists via the settings store and pushes the value to `WishlistRelicsViewModel.ViewMode` for the main window's live preview. The modeless window makes mode comparison convenient — flip the selector while watching the relics panel change.

*Alternatives considered*: a selector in the Relics control bar — rejected in favor of consistency (orientation and card width already live in Appearance) and control-bar simplicity; presentation-local persistence (like splitter widths in code-behind) — rejected; view mode is a first-class user choice, matching the orientation precedent.

### 2. Mode switching via sibling subtrees toggled by IsVisible

The relics panel content becomes three siblings — Verbatim cards `ItemsControl`, Compact cards `ItemsControl`, and the `TreeDataGrid` — toggled by `IsViewModeVerbatim` / `IsViewModeCompact` / `IsViewModeTable` bindings. This is the established idiom (stacked vs side-by-side uses exactly this).

Because the relics panel (control bar + scroll content) is currently duplicated across the stacked and side-by-side subtrees, tripling its content would create six copies. **Extract the relics panel into a shared `UserControl` (control bar + mode subtrees) used by both layout subtrees.** This removes the existing duplication rather than multiplying it.

*Alternative considered*: a `DataTemplate` selector switching card templates in one ItemsControl — workable for the two card modes but not for Table (different control entirely), so the sibling-subtree approach is used uniformly.

### 3. Compact composition reuses the card grammar minus the tier rows

Compact cards keep the card border, header (clipped name + ⋮ flyout), `RelicCardWidth` binding, and vaulted styling. The body is a flat list of reward lines, each with the tier-colored accent bar in the left gutter (preserving the established color channel) and a trailing `(C)`/`(U)`/`(R)` letter marker. `ShowAllRewards` and dimming behave exactly as in Verbatim mode.

To support this, `WishlistRelicRewardViewModel` gains the tier (currently only the group knows it) plus computed presentation properties (`CompactName`, `TierLetter`); `WishlistRelicCardViewModel` exposes a flat `Rewards` list alongside `TierGroups`. Domain reward names are untouched.

**Follow-up revisions (post-implementation):** the tier marker moved from a trailing suffix to a leading prefix, then dropped its parentheses in favor of **bold** weight (`Forma (C)` → `(C) Forma` → **C** Forma); and the per-line accent bars were removed entirely — per-line bars added color noise without the grouping job they do in Verbatim, so Compact's tier identity now rests on the letter marker alone and the freed left gutter is reclaimed. Table child rows use the same bold leading marker.

### 4. Word-clipping rule (precise, testable)

Applied to display strings only:
- Card header / table relic name: strip trailing `" Relic"`.
- Reward names: remove the word `" Prime"`; strip trailing `" Blueprint"` **unless** the original name ends with `"Prime Blueprint"` (a main blueprint), in which case `Blueprint` is retained.
  - `Revenant Prime Blueprint` → `Revenant Blueprint`
  - `Revenant Prime Neuroptics Blueprint` → `Revenant Neuroptics`
  - `Forma Blueprint` → `Forma` (no `Prime`, so trailing `Blueprint` is clipped)

The rule is a pure string transform in the presentation layer, unit-testable in isolation. If `Forma` reads oddly in practice, the rule has one obvious extension point (keep `Blueprint` when clipping would leave a single word) — deferred until observed.

### 5. Table mode on Avalonia.Controls.TreeDataGrid

Hierarchical mode maps 1:1 to the need (relic rows expanding into reward rows). Cost accepted knowingly: theme include in `App.axaml` (`avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml`), and a `HierarchicalTreeDataGridSource<T>` built in code rather than pure-XAML bindings.

**Deviation discovered during implementation:** on the Avalonia 12 line (the app is on 12.0.5), the *official* `Avalonia.Controls.TreeDataGrid` package became a paid Avalonia Accelerate product — it fails the build with `AVLIC0001` (no license key), and there is no free official 12.x build. Per user decision, Table mode uses the free **community fork** `Caly.Avalonia.Controls.TreeDataGrid` (`12.0.1.3`, by BobLd), which keeps the original assembly name, namespaces, API, and theme avares path unchanged — so the structure below is unaffected. Tradeoff: a third-party, less-vetted dependency in exchange for avoiding the commercial license. If the fork is later abandoned, the alternatives are a paid license or re-architecting Table mode on free built-in controls (`TreeView` / nested `ItemsControl` + `Expander`).

Structure:
- Row VM per relic: clipped name, wishlisted-tier markers string (tiers containing ≥1 wishlisted reward, C→U→R order, e.g. `(C, R)`; later revised to unparenthesized with a count prefix when a tier has >1 wishlisted reward, e.g. `C, 2 R`), vaulted flag, child reward rows.
- Child reward row VM: compact reward name + leading tier letter marker. Expansion shows only **wishlisted** rewards — non-wishlisted rewards are omitted from expansion, not dimmed (revised after initial implementation; see the row-actions/expansion follow-up below).
- Columns: hierarchical expander column with the relic name; markers column; two `TemplateColumn` pictogram cells (⊞ rewards modal, 💬 LFG compose) — pictogram cells are empty on child rows.
- Ordering matches the card modes (era Lith→Meso→Neo→Axi, then alphabetical); `ShowVaulted` filters rows; vaulted rows get the reduced-opacity treatment.

**Follow-up revision (post-implementation):** the "expansion shows all rewards, dimmed" behavior above was the initial implementation but felt noisy in practice; the user requested expansion be filtered to wishlisted rewards only, and Compact mode's tier letter marker be moved from a trailing suffix to a leading prefix (`(C) Forma` instead of `Forma (C)`). Both changed accordingly; `RewardTableRowViewModel` no longer has a dimmed state since only wishlisted rewards are ever rows.

### 6. Rewards modal reuses the Verbatim card

The ⊞ pictogram opens a dialog window (LFG-dialog precedent) hosting a card bound to a `WishlistRelicCardViewModel` constructed in all-rewards mode for that relic, rendered by the app-scoped Verbatim card `DataTemplate` (resolved implicitly by type). The Verbatim card styles (tier bars, tier label, vaulted/dimming) live in `MainWindow.axaml`'s `Window.Styles`, which do not reach a separate dialog window, so the modal restates them in its own `Window.Styles`. The modal has no in-content Close button; only the window's native close control dismisses it (revised after initial implementation, which had a redundant Close button alongside the window chrome's close).

*Revised after initial implementation:* the modal originally used a window-scoped copy of the card template that additionally highlighted wishlisted reward lines with a background wash (`WishlistHighlightBrush`). The highlight read as parasitic noise and was removed; with it gone the copy was identical to the app-scoped template, so the copy was deleted and the modal now hosts the shared template directly. Dimming of non-wishlisted rewards is the only wishlist cue, matching the main panel.

### 7. Show all rewards visibility

The checkbox (and its label) is hidden — not disabled — while Table mode is active, driven by the same `IsViewModeTable` binding. Its checked state is retained so switching back restores the previous card-mode behavior.

## Risks / Trade-offs

- **[TreeDataGrid version/theme mismatch with the app's Avalonia version]** → pin the package version matching the Avalonia minor in the csproj; verify the theme include renders under the app's Fluent theme and font-size settings before building on it (do this as the first Table-mode task).
- **[TreeDataGrid styling diverges from app look (row heights, selection chrome, font scaling)]** → constrain scope: no selection semantics needed; style only what the spec requires (dimming, vaulted opacity, pictogram cells); accept stock chrome otherwise.
- **[Clipping rule surprises on unusual reward names (e.g. `Forma Blueprint`, `2X Forma Blueprint`)]** → rule is centralized and unit-tested; scenarios pin the known cases; extend the rule only when a real name misbehaves.
- **[Six-way duplication of the relics panel]** → mitigated by extracting the shared relics panel UserControl (Decision 2); this touches the side-by-side width wiring in code-behind, which must be re-verified after extraction.
- **[Verbatim mode remains the default, so density win is opt-in]** → intentional at design time: the user explicitly wanted side-by-side comparison before choosing a preference. *(Resolved: after comparison the user chose Compact, and the default — including the legacy-settings fallback — was switched to Compact.)*

## Open Questions

- None blocking. The exact TreeDataGrid package version and any theme-interop quirks are resolved empirically in the first Table-mode task.
