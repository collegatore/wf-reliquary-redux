## Context

The main window is a single `TabControl` (`MainWindow.axaml`) bound to `MainWindowShellViewModel.SelectedTabIndex`. Today the `TabItem` declaration order is **Drop Tables (0), Prime Gear (1), Relics and Missions (2), Settings (3)**. Settings is already a "menu-accessed" tab: it is reached via **File → Settings**, and its header is permanently hidden using `<TextBlock IsVisible="False" Text="Settings" />` as the header content. A discard-guard in `MainWindow.axaml.cs` (`OnTabSelectionChanged`) and `MainWindowShellViewModel.NavigateToSettings` both hardcode the Settings index as `3`.

This change makes **Drop Tables** behave like Settings (File-menu accessed, header hidden when inactive), makes **Prime Gear** the startup tab, and upgrades the header rule for both menu tabs from *always hidden* to *hidden while inactive, visible while active*.

This is a Presentation-only change. No Application/Infrastructure/Composition code, no persisted state, no database or settings-format migration.

## Goals / Non-Goals

**Goals:**
- Prime Gear is the selected tab on startup; Drop Tables is never the startup tab.
- Drop Tables is reachable only via **File → Drop Tables**; Settings remains reachable via **File → Settings**.
- Settings and Drop Tables headers are hidden in the strip while inactive and appear while active.
- Keep the existing Settings discard-guard behavior intact.

**Non-Goals:**
- No change to the Drop Tables tab's *content* (download/import sections, status lines) — only where/how it is surfaced.
- No keyboard shortcut for Drop Tables (Settings keeps Ctrl+,). Out of scope unless requested.
- No change to the Relics/Missions splitter, profiles footer, or any view model below the shell.

## Decisions

### Decision 1: Reorder `TabItem`s so Prime Gear is first and the two menu tabs are last

New declaration order: **Prime Gear (0), Relics and Missions (1), Drop Tables (2), Settings (3)**.

Rationale:
- The startup default falls out for free: `MainWindowShellViewModel.selectedTabIndex` already defaults to `0`, which now points at Prime Gear — no startup-selection plumbing needed and no flash of the Drop Tables tab during binding.
- `SettingsTabIndex` stays `3`, so neither hardcoded constant (`MainWindowShellViewModel` and `OnTabSelectionChanged`) needs to change.
- Both menu-accessed tabs sit at the right end of the strip, so when surfaced their headers appear adjacently on the right rather than Drop Tables jumping to the leftmost slot.

Alternative considered: leave Drop Tables at index 0 and set the startup `SelectedTabIndex` to Prime Gear's index explicitly. Rejected — it requires extra startup plumbing, risks a one-frame flash of Drop Tables, and scatters surfaced headers to opposite ends of the strip.

### Decision 2: Bind each menu tab's header visibility to its own `IsSelected`

Replace the always-hidden Settings header and apply the same pattern to Drop Tables: the header `TextBlock`'s `IsVisible` binds to the owning `TabItem`'s `IsSelected` via a `RelativeSource` `AncestorType=TabItem` binding (or an equivalent `TabItem:selected` style). When the tab is selected its label renders in the strip; otherwise it collapses.

Rationale: the active/inactive header rule is intrinsically per-tab selection state, so binding to `IsSelected` keeps it declarative with no view-model flag. Strip tabs (Prime Gear, Relics and Missions) keep plain string headers and are unaffected.

Alternative considered: a view-model boolean per menu tab. Rejected as redundant — `IsSelected` already carries exactly this state.

### Decision 3: Add `NavigateToDropTablesCommand` mirroring `NavigateToSettingsCommand`

`MainWindowShellViewModel` gains a `NavigateToDropTablesCommand` that sets `SelectedTabIndex` to the Drop Tables index (`2`). A `File → Drop Tables` `MenuItem` binds to it, sitting beside the existing `Settings` item. Drop Tables has no unsaved-changes guard (only Settings does), so the command is a plain index switch — no `previousTabIndex`/discard handling required.

## Risks / Trade-offs

- **Residual empty header sliver while inactive** → The header `TextBlock` collapsing still leaves the `TabItem`'s header container in the strip layout (pre-existing behavior for Settings today). Accepted as-is to match current behavior; if it proves visible, a `TabItem:not(:selected)` style can zero the header padding. Not pursued unless observed.
- **Hardcoded index assumptions drift** → Indices are still hardcoded in two places. Mitigation: the reorder keeps `SettingsTabIndex = 3` so the existing constants remain correct, and the index map comment in `MainWindowShellViewModel` is updated to the new order. Verified by the existing Settings-guard integration tests plus new startup/visibility tests.
- **Startup tab assumption in tests** → Any test asserting the old startup tab (Drop Tables) will fail. Mitigation: update those tests as part of the change; add a test asserting Prime Gear is selected on load.
