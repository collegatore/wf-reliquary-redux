## Context

The app already saves and restores window size (`WindowWidth`, `WindowHeight`) via `UserSettings`, `LocalUserSettingsStore`, and `MainWindow`. The window always opens at the OS-default position (primary monitor, centered), ignoring wherever the user had placed it.

Avalonia exposes `Window.Position` (`PixelPoint`) for reading/setting the top-left corner of the window in screen coordinates, and `Window.Screens.All` (`IReadOnlyList<Screen>`) where each `Screen` has a `Bounds` (`PixelRect`) covering its pixel area.

## Goals / Non-Goals

**Goals:**
- Save window position (X, Y) on close alongside the existing size save
- Restore the saved position on launch when the target monitor is still available
- Gracefully fall back to OS-default position when the monitor is gone

**Non-Goals:**
- Saving/restoring position for any window other than the main window
- Snapping the window back onto the nearest screen edge when partially off-screen (not a reported pain point)
- Per-profile window positions

## Decisions

### Add `WindowX` / `WindowY` as nullable ints to `UserSettings`

`PixelPoint` coordinates are integers. Nullable (`int?`) lets existing settings files deserialize without those fields and naturally express "no saved position" without a sentinel value.

`UserSettings` uses a positional record with `with`-expression updates — add the two fields with defaults of `null`.

**Alternative considered:** Store as `double` to match the existing width/height fields. Rejected — screen coordinates are always integers in Avalonia and widening to double would require a cast on every use.

### Read position from `Window.Position` at close time

`Window.Position` is a live `PixelPoint` that reflects the actual top-left corner of the window frame. It is available during the `Closing` event, consistent with how `ClientSize` is already read there.

### Validate saved position against `Screens.All` at startup

Before applying the saved position, iterate `this.Screens.All` and check whether any screen's `Bounds.Contains(new PixelPoint(x, y))` returns true. If none match, skip applying the position so the OS places the window normally.

This is the simplest guard against a disconnected or reconfigured monitor. It uses only the Avalonia screen API already available on `Window`.

**Alternative considered:** Allow the position to be applied unconditionally and let the OS clip it. Rejected — Avalonia on Windows can place the window fully off-screen in this case, making it inaccessible without keyboard tricks.

## Risks / Trade-offs

- **Multi-monitor scaling differences** — if the user moves the window to a 4K monitor, saves, then opens on a machine with only a 1080p monitor that happens to have overlapping coordinates, the window may open in an unexpected but valid location. Accepted: the fallback only activates when no screen contains the saved point, which covers the common "monitor unplugged" case.
- **JSON deserialization of new fields** — existing `user-settings.json` files lack `WindowX`/`WindowY`. `System.Text.Json` will deserialize missing nullable fields as `null`, which is the correct "no saved position" state. No migration needed.

## Migration Plan

No data migration required. The new fields default to `null`; existing settings files continue to deserialize correctly. On first close after the update, position is added to the file automatically.
