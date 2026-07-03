## MODIFIED Requirements

### Requirement: Window Size and Position Restored on Launch

The application SHALL restore the previously saved window size and screen position on startup. If no saved size exists the application SHALL use the built-in default dimensions (1280 × 800). If no saved position exists, or if the saved position is not on any currently connected monitor, the application SHALL let the OS position the window.

> **Implementation note:** `IUserSettingsStore.LoadAsync` must be called via `Task.Run` on the UI thread to prevent a deadlock with Avalonia's `SynchronizationContext` when the settings file exists and `File.ReadAllTextAsync` posts its continuation back to the UI thread.

#### Scenario: Saved size and position are restored

- **WHEN** the application starts and previously saved window dimensions and position exist in user settings
- **AND** the saved position falls within the bounds of at least one currently connected screen
- **THEN** the main window opens at the saved width, height, and screen position

#### Scenario: No saved size falls back to default

- **WHEN** the application starts and no window size is present in user settings
- **THEN** the main window opens at 1280 × 800

#### Scenario: Saved position is on a disconnected monitor

- **WHEN** the application starts and a previously saved position exists in user settings
- **AND** the saved position does not fall within the bounds of any currently connected screen
- **THEN** the main window opens at the saved size but at the OS-default position (position is not applied)
