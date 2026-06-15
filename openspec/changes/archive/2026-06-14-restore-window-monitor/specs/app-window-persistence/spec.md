## MODIFIED Requirements

### Requirement: Window Size Saved on Close

The application SHALL save the main window's current width, height, and screen position (X, Y) to persistent user settings when the window is closed.

> **Implementation note:** `Window.Width` / `Window.Height` are styled-property "desired" values that stay `NaN` after the user drag-resizes the window. The actual live dimensions must be read from `Window.ClientSize` at close time.

> **Implementation note:** The window position must be read from `Window.Position` (a `PixelPoint`) at close time.

> **Implementation note:** The save is performed synchronously (blocking) inside the `Closing` event handler using `Task.Run(...).GetAwaiter().GetResult()` so that the write completes before the process exits.

#### Scenario: Window size and position saved on normal close

- **WHEN** the user closes the main window
- **THEN** the current window width, height (client area), and screen position (X, Y) are written to the user settings store

### Requirement: Window Size and Position Restored on Launch

The application SHALL restore the previously saved window size and screen position on startup. If no saved size exists the application SHALL use the built-in default dimensions (900 × 600). If no saved position exists, or if the saved position is not on any currently connected monitor, the application SHALL let the OS position the window.

#### Scenario: Saved size and position are restored

- **WHEN** the application starts and previously saved window dimensions and position exist in user settings
- **AND** the saved position falls within the bounds of at least one currently connected screen
- **THEN** the main window opens at the saved width, height, and screen position

#### Scenario: No saved size falls back to default

- **WHEN** the application starts and no window size is present in user settings
- **THEN** the main window opens at 900 × 600

#### Scenario: Saved position is on a disconnected monitor

- **WHEN** the application starts and a previously saved position exists in user settings
- **AND** the saved position does not fall within the bounds of any currently connected screen
- **THEN** the main window opens at the saved size but at the OS-default position (position is not applied)
