## 1. Data Model

- [x] 1.1 Add `WindowX` (`int?`) and `WindowY` (`int?`) fields to `UserSettings` record with default `null`
- [x] 1.2 Update `UserSettings.Default` to leave `WindowX` and `WindowY` as `null`

## 2. Settings Persistence

- [x] 2.1 Verify `LocalUserSettingsStore` serializes/deserializes the new nullable int fields correctly (System.Text.Json handles missing keys as `null` for nullable types — confirm no explicit mapping needed)

## 3. Save Position on Close

- [x] 3.1 In `MainWindow.OnWindowClosing`, read `this.Position` (`PixelPoint`) after reading `ClientSize`
- [x] 3.2 Include `WindowX = position.X` and `WindowY = position.Y` in the `store.SaveAsync` `with`-expression

## 4. Restore Position on Launch

- [x] 4.1 In `MainWindow` startup (after loading `initialSettings`), check if `WindowX` and `WindowY` are non-null
- [x] 4.2 If non-null, validate the saved point against `this.Screens.All` — apply `this.Position = new PixelPoint(x, y)` only if at least one screen's `Bounds.Contains(point)` is true
- [x] 4.3 If the saved point is on no connected screen, skip setting position (let OS decide)

## 5. Testing

- [x] 5.1 Run the app, move the window to a secondary monitor, close, and reopen — verify it reopens on the same monitor
- [x] 5.2 Verify reopening after no saved position (fresh settings) still works at default size and OS position

