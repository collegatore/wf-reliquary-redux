## Why

The app currently loses window size on every restart and has no user-facing controls for visual preferences, forcing all configuration into scattered UI fragments (e.g., a standalone "Save URL" button on the Drop Tables tab). Consolidating settings into a dedicated tab and persisting layout state between launches reduces friction for regular users.

## What Changes

- Window size is saved to user settings on close and restored on next launch
- Font size becomes user-configurable in three steps: small, medium (current default), large; the chosen size applies app-wide
- The split between the Relics panel and Missions panel on the Relics & Missions tab is draggable; the chosen proportion is persisted
- A **Settings tab** is introduced, accessible via **File → Settings** in the application menu bar; it hosts font size, split proportion, and the drop-tables source URL field
- Settings tab has **Save** and **Back** buttons; pressing Back with unsaved changes prompts the user to confirm discarding them
- The standalone **Save URL** button on the Drop Tables tab is removed; **BREAKING**: `Drop-table source URL` section moves out of the Drop Tables tab entirely
- The top-level application menu bar is introduced (Windows/Avalonia native `Menu` + `MenuItem`)

## Capabilities

### New Capabilities

- `settings-tab`: Settings tab UI — font size selector, split proportion display/reset, drop-tables URL field — with Save/Back flow, unsaved-changes guard, and File-menu entry point
- `app-window-persistence`: Window width and height saved on close, restored on next launch (falls back to defaults when no saved value exists)
- `font-size-setting`: App-wide font size configurable as small / medium / large; selected size applied via a resource-key override at the root level; persisted in user settings
- `split-panel-persistence`: Relics/Missions split ratio is user-draggable and persisted; restored on next launch

### Modified Capabilities

- `drop-table-source-refresh`: The "Drop-table source URL" section and the Save URL button are removed from the Drop Tables tab UI; URL configuration moves to the Settings tab (owned by `settings-tab`)

## Impact

- `Presentation` project: new Settings view/viewmodel, updated Drop Tables view (remove URL section), root layout gains `Menu`, Relics & Missions view gets a `GridSplitter`
- `Application` or `Infrastructure` project: user settings persistence (window size, font size, split ratio, source URL already there); likely a new `IUserSettingsService` or extension of existing settings storage
- No database schema changes; settings stored in a separate user-scoped config file (e.g., JSON via `ISettingsService`)
- Existing tests for `drop-table-source-refresh` URL scenarios remain valid at the application layer; only the UI-layer scenarios change
