## Context

The Settings tab currently treats font size selection the same as the URL field: changes enter dirty state and must be confirmed with Save. Since font size is previewed live and is inherently safe to auto-persist, this friction is unnecessary. The other three changes (custom font size, Ctrl+S shortcut, LFG reload-from-saved) are small UX improvements with no architectural risk.

Key existing facts:
- `SettingsViewModel` holds both the editable draft and the "last-saved" baseline for each field, computing `HasUnsavedChanges` by diffing them.
- Font size is applied immediately on selection (`ApplyFontSize`) but only becomes the "saved" baseline after `SaveAsync()`.
- `LfgTemplateEditorViewModel` already separates `savedTemplate` from the draft; it has a `RestoreDefaults` method but no "restore from last save" action.
- `UserFontSize` is a 3-value enum in Application; the presentation layer has a parallel `FontSizeOption` enum.

## Goals / Non-Goals

**Goals:**
- Font size selection auto-saves instantly and does not enter the unsaved-changes guard.
- A "Custom" font size option accepts an arbitrary numeric pixel value; it also auto-saves.
- Ctrl+S triggers the Settings Save action while the Settings tab is focused.
- "Reload from saved" in the LFG section reverts the draft to the last-persisted template.

**Non-Goals:**
- Changing the save behavior for URL, relics panel height, or any non-font field.
- Making the LFG template auto-save independently of the main Save button.
- Persisting Ctrl+S globally (outside the Settings tab) as a shortcut.

## Decisions

### D1 — Font size auto-save: load-then-save, touching only the font size field

**Decision:** When `EffectiveFontSizePt` changes, immediately call a private `SaveFontSizeNow()` helper that loads the currently-persisted `UserSettings` from the store, replaces only `FontSizePt`, and saves the result. Font size is removed from `HasUnsavedChanges` and from the discard-rollback path.

**Why load from the store rather than snapshot ViewModel state?** Snapshotting from ViewModel state would accidentally persist any other unsaved edits the user has made in the same session (e.g. a half-edited source URL). Loading from the store and updating only `FontSizePt` avoids touching anything the user hasn't explicitly chosen to save.

**Race safety:** Both `SaveFontSizeNow()` and the main `SaveAsync()` run on Avalonia's UI thread. Because `await` yields cooperatively, only one can hold the await-continuation slot at a time — they cannot truly interleave. The worst-case ordering (font-size save completes, then main Save overwrites) is still safe: `SaveAsync()` always builds its record from current ViewModel state, which includes the latest `fontSizePt`, so the final write is always consistent.

**Async handling:** `SaveFontSizeNow()` is `async Task`; the property setter uses `_ = SaveFontSizeNow()` (fire-and-forget on the UI thread). Save errors are non-fatal for font size (same tolerance as window-resize persistence).

### D2 — Custom font size: single `double FontSizePt` field; "Custom" is a derived presentation state

**Decision:** Replace the `UserFontSize FontSize` enum field on `UserSettings` with a single `double FontSizePt`. There is no `Custom` case in the Application-layer `UserFontSize` enum and no separate `CustomFontSizePt` field. "Custom" exists only in the presentation-layer `FontSizeOption` enum as a derived display state meaning "the current `FontSizePt` does not match any preset."

**ViewModel:** `fontSizePt` is the sole backing field in `SettingsViewModel`. `SelectedFontSize` is a derived computed property (no backing field of its own):
- Getter: returns `Small/Medium/Large` if `fontSizePt` matches the preset constant, else `Custom`.
- Setter: maps the preset to its `double` constant and assigns `EffectiveFontSizePt`; setting `Custom` is a no-op (the numeric value is already current).

`EffectiveFontSizePt` is the single writable property that the `NumericUpDown` binds to. It clamps the incoming value, updates `fontSizePt`, raises property-changed for itself and (if the option changed) for `SelectedFontSize`, applies the size live, and fires `SaveFontSizeNow()`.

**NumericUpDown always visible:** The numeric input is shown at all times (not conditionally on Custom). It displays the effective size for any selection — pressing up/down from a preset automatically moves to Custom because the resulting value no longer matches the preset.

**Persistence:** `LocalUserSettingsStore` writes both `FontSizePt` (primary) and the legacy `FontSize` enum (derived via `PtToLegacyFontSize` for backward compat) on every save. On load it prefers `FontSizePt > 0`; if absent (old file) it converts the `FontSize` enum via `LegacyFontSizeToPt`. The `UserFontSize` enum remains in Application but is used only inside `LocalUserSettingsStore`'s `SettingsFile` DTO.

**Post-mortem (implemented then reverted):** The original implementation kept `UserFontSize FontSize` in `SettingsFile` as a legacy field — dual-writing it on every save and falling back to it on load if `FontSizePt` was absent — to avoid breaking existing `user-settings.json` files. This was a backward-compatibility shim in exactly the sense the pre-V1 persistence rule forbids: non-schema formats evolve in place, with no migration or compatibility code. The shim was removed in the same session: `UserFontSize.cs` deleted, `LegacyFontSizeToPt`/`PtToLegacyFontSize` helpers removed, `SettingsFile` reduced to `double FontSizePt`. Existing settings files with only the old `fontSize` enum field will load with `FontSizePt` defaulting to `0`, which the load path treats as missing and replaces with `UserSettings.Default.FontSizePt` (14.0 pt). In corrected implementation `LocalUserSettingsStore` writes `FontSizePt` directly as a `double` field in `SettingsFile`. `UserFontSize` and the `FontSize` enum field are removed entirely.

**Input range:** `NumericUpDown` clamped to 8–32 pt. `EffectiveFontSizePt` setter also clamps in code.

**Post-apply correction:** The font-size section heading (`TextBlock FontWeight="SemiBold" Text="Font size"`) and the "pt" label next to the `NumericUpDown` are `TextBlock`s and therefore inherit the global `ContentFontSize` style — they scaled with the setting while the `RadioButton` labels did not, creating visual inconsistency. Both were pinned to `FontSize="{StaticResource FontSizeMedium}"`.

### D3 — Ctrl+S shortcut: scoped to SettingsView via InputBindings

**Decision:** Add a `<KeyBinding Gesture="Ctrl+S" Command="{Binding Settings.SaveCommand}" />` inside `<UserControl.KeyBindings>` in `SettingsView.axaml`.

**Avalonia scoping:** `KeyBinding` on a `UserControl` fires only when the control (or a descendant) holds keyboard focus. Since the Settings panel is only visible and focused while that tab is active, this naturally limits the shortcut to that context without any runtime checks.

### D4 — Reload from saved: new command on LfgTemplateEditorViewModel

**Decision:** Add `ReloadFromSavedCommand` (a `RelayCommand`) to `LfgTemplateEditorViewModel`. It resets `patternText` to `savedTemplate.Pattern`, clears and re-populates `Groups` from `savedTemplate.Groups`, clears parse errors/warnings, then calls `NotifySaved(savedTemplate)` to clear dirty state.

**Why `NotifySaved(savedTemplate)` is safe here:** `LfgTemplateEditorViewModel.OnSaved` is wired in `MainWindow` to `t => currentLfgTemplate = t` — it just keeps the popup's active-template reference in sync. After reload-from-saved, `savedTemplate` *is* the correct active template for the popup, so this update is idempotent and correct. Nothing is written to disk by `OnSaved`. The `SettingsViewModel.OnSaved` callback (which clears the LFG editor's dirty state via `LfgEditor.NotifySaved`) is a separate callback on a different VM and is not involved here.

This is the exact symmetric of `RestoreDefaults` (which loads from the in-code default rather than from `savedTemplate`), so implementation is straightforward.

## Risks / Trade-offs

- **Fire-and-forget font-size save could silently fail** → Non-critical: font size will simply not be persisted until next explicit Save. Same pattern already used for window size. An error would appear in the app log via the existing error-reporting surface.
- **Custom font size extremes could break layout** → Mitigated by clamping `NumericUpDown` to 8–32 pt. Card widths already scale proportionally in `ApplyFontSize`, so the layout degrades gracefully.
- **Ctrl+S conflicts with text-box system shortcuts** → No conflict: text boxes do not use Ctrl+S natively in Avalonia; the binding fires the Save command when focus is within the Settings panel.
