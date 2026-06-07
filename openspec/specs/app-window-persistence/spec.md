# app-window-persistence Specification

## Requirements

### Requirement: Window Size Saved on Close

The application SHALL save the main window's current width and height to persistent user settings when the window is closed.

> **Implementation note:** `Window.Width` / `Window.Height` are styled-property "desired" values that stay `NaN` after the user drag-resizes the window. The actual live dimensions must be read from `Window.ClientSize` at close time.

> **Implementation note:** The save is performed synchronously (blocking) inside the `Closing` event handler using `Task.Run(...).GetAwaiter().GetResult()` so that the write completes before the process exits.

#### Scenario: Window size saved on normal close

- **WHEN** the user closes the main window
- **THEN** the current window width and height (client area) are written to the user settings store

### Requirement: Window Size Restored on Launch

The application SHALL restore the previously saved window size on startup. If no saved size exists the application SHALL use the built-in default dimensions (900 × 600).

> **Implementation note:** `IUserSettingsStore.LoadAsync` must be called via `Task.Run` on the UI thread to prevent a deadlock with Avalonia's `SynchronizationContext` when the settings file exists and `File.ReadAllTextAsync` posts its continuation back to the UI thread.

#### Scenario: Saved size is restored

- **WHEN** the application starts and a previously saved window size exists in user settings
- **THEN** the main window opens at the saved width and height

#### Scenario: No saved size falls back to default

- **WHEN** the application starts and no window size is present in user settings
- **THEN** the main window opens at 900 × 600
