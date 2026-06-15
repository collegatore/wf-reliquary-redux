## Why

The app currently restores its size on launch but always opens on the primary monitor. Users who work on a secondary monitor must move the window every session, which is disruptive.

## What Changes

- Save the main window's screen position (X, Y) to user settings when the app closes, alongside the existing size save.
- Restore the saved position on startup so the window reopens on whichever monitor it was last on.
- Validate the saved position against current screen geometry at launch; fall back to the system default position if the target monitor is no longer connected.

## Capabilities

### New Capabilities

_(none)_

### Modified Capabilities

- `app-window-persistence`: Extend requirements to include saving and restoring window position (X, Y) in addition to the existing size (width, height).

## Impact

- `UserSettings` model — add `WindowX` / `WindowY` fields
- `IUserSettingsStore` / `UserSettingsStore` — persist and load the new fields
- `MainWindow` close handler — capture `Window.Position` alongside `ClientSize`
- `MainWindow` startup — apply saved position after setting size; validate against available screens
