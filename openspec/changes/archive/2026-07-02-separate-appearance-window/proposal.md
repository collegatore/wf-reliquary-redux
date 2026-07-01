## Why

Font size, relic card width, and Relics/Missions layout orientation already apply live and auto-save the instant they're changed, but they live inside the Settings tab, which fully replaces whatever the user was looking at (Prime Gear, Relics & Missions, etc.). To see the effect of a change, the user has to leave the tab they were on, tweak the setting, then switch back — there's no way to watch the real content update while adjusting it.

## What Changes

- Add a new **File → Appearance** menu item that opens a modeless, single-instance Appearance window.
- Move the font size, relic card width, and Relics/Missions layout orientation controls out of the Settings tab into this new window (not mirrored — the Settings tab no longer shows them).
- Changes made in the Appearance window continue to apply live and auto-save exactly as they do today, via the existing shared `MainWindowShellViewModel` properties and `DynamicResource` bindings — no new preview surface or duplicated view is introduced. The main window, in whatever state the user left it, is the live preview.
- The Appearance window has a fixed size (not resizable) and opens positioned 50px left and 50px down from the main window's current position, every time — no size/position persistence.
- The Appearance window uses default Avalonia window-close/shutdown behavior; no special lifecycle handling.
- Settings tab keeps all other controls unchanged (relics panel height, drop-tables source URL, LFG template, Save/Back, unsaved-changes guard).
- Scoping guardrail (for future changes, not enforced in code): only settings that already apply live with no Save-button gate belong in the Appearance window; adding anything else there requires explicit approval first.

## Capabilities

### New Capabilities
- `appearance-window`: A modeless, single-instance window opened via File → Appearance, hosting the font size, relic card width, and layout orientation controls, so the user can watch the main window update live while adjusting them.

### Modified Capabilities
- `settings-tab`: Settings Tab Content no longer includes font size or relic card width controls; the File-menu requirement gains an adjacent Appearance entry (documented in `appearance-window` instead of duplicated here).
- `font-size-setting`: The font size selector moves from the Settings tab to the Appearance window; live-apply and auto-save behavior is unchanged.
- `relic-card-width-setting`: The relic card width control moves from the Settings tab to the Appearance window; live-apply, auto-save, clamping, and font-scaling behavior is unchanged.
- `relics-missions-layout-orientation`: The layout orientation toggle moves from Settings → Appearance to the Appearance window; live-apply and auto-save behavior is unchanged.

## Impact

- Presentation layer only (Avalonia UI): `SettingsView.axaml`/`.cs`, `SettingsViewModel.cs`, `MainWindow.axaml`/`.cs`, plus a new `AppearanceWindow.axaml`/`.cs` (+ view model, reusing or extracting the existing appearance-control bindings from `SettingsViewModel`).
- No changes to persistence (`IUserSettingsStore`/`LocalUserSettingsStore`), the `UserSettings` shape, Application layer, or Infrastructure layer — these settings already auto-save independently of the Settings tab's Save button, and the window's own bounds are neither resizable nor persisted.
- No database or schema impact.
