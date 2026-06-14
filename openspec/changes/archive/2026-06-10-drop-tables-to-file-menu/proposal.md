## Why

The **Drop Tables** tab is an infrequently-used maintenance surface (download/import the source file), yet it currently occupies the first slot in the tab strip and is the tab shown on app start. Everyday use centers on **Prime Gear** and **Relics and Missions**. Surfacing a maintenance tab first adds noise and an extra click for the common path. Moving Drop Tables alongside Settings under the **File** menu declutters the strip and makes Prime Gear the immediate landing surface.

## What Changes

- The **Drop Tables** tab is removed from the always-visible tab strip and is reached via a new **File → Drop Tables** menu item (mirroring how **File → Settings** already works).
- On app start, **Prime Gear** is the selected tab. The Drop Tables tab is no longer shown on startup.
- Menu-accessed tabs (**Settings** and **Drop Tables**) hide their header in the tab strip while inactive, and show their header in the strip while active (so the user can see which menu tab they are on and can click back to a strip tab). This replaces Settings' current always-hidden header.
- The default tab strip therefore shows **Prime Gear** and **Relics and Missions**; **Drop Tables** and **Settings** headers appear only when their tab is the active one.

## Capabilities

### New Capabilities

- `main-window-navigation`: Defines the application's tab-shell navigation — which tab is selected on startup, which tabs live in the always-visible strip versus the File menu, and the shared rule that File-menu-accessed tabs hide their header while inactive and reveal it while active.

### Modified Capabilities

- `settings-tab`: The "Settings Tab Navigation via File Menu" requirement currently states the Settings tab header SHALL NOT be visible in the tab strip. This changes to: hidden while inactive, visible while the Settings tab is active.

## Impact

- **Presentation only.** No Application/Infrastructure/Composition changes.
- `MainWindow.axaml`: move the Drop Tables `TabItem` out of the default strip ordering; bind both the Settings and Drop Tables `TabItem` headers' visibility to whether that tab is active; add a **Drop Tables** item under the **File** `Menu`.
- `MainWindowShellViewModel`: add a navigate-to-Drop-Tables command; set the startup `SelectedTabIndex` to Prime Gear; adjust the `SettingsTabIndex` constant and any tab-index assumptions if the `TabItem` order changes.
- `MainWindow.axaml.cs`: the tab-selection guard logic references a hardcoded Settings tab index; keep it consistent with the new ordering.
- Tests: integration/presentation tests that assert the startup tab, File-menu contents, or tab header visibility.
- No persisted-state changes; no database or settings-format migration.
