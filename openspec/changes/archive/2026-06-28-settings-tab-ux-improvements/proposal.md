## Why

The Settings tab has minor friction points: font size changes require a Save click even though they are instantly previewed and low-risk, there is no keyboard shortcut for Save, and the LFG template editor offers "Restore defaults" but no way to revert a draft to the last-saved state. The "Custom" font size option rounds out the preset selector for users who want a size between the fixed steps.

## What Changes

- Font size selection saves immediately and silently; it no longer contributes to the unsaved-changes dirty state or the leave-guard.
- A **Custom** option is added to the font size selector with a numeric pixel input; it auto-saves on change like the other font size options.
- A **Ctrl+S** keyboard shortcut triggers the Settings Save action; it is scoped to the Settings tab and inactive elsewhere.
- A **Reload from saved** button is added to the LFG message section; it reverts the draft template to the persisted template without affecting any other settings.

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `font-size-setting`: Adds a fourth "Custom" size option with a numeric input; font size selection becomes an auto-saved action that does not enter dirty state.
- `settings-tab`: Removes font size from the unsaved-changes guard; adds Ctrl+S Save shortcut scoped to the tab; adds "Reload from saved" to the LFG template editor section.
- `lfg-message-constructor`: Adds "Reload from saved" action to the template editor draft lifecycle.

## Impact

- `SettingsViewModel`: `SelectedFontSize` setter calls save immediately and updates `savedFontSize` in place; font size no longer included in `HasUnsavedChanges`; `TryDiscardAsync` no longer needs to revert font size.
- `SettingsView.axaml`: Add `Custom` radio + numeric `NumericUpDown` input to font size section; add Ctrl+S `KeyBinding`; add "Reload from saved" button to LFG editor actions row.
- `LfgEditorViewModel` (or equivalent): Add `ReloadFromSavedCommand` that restores the draft from the persisted template.
- `UserSettings` / `UserFontSize`: Add `Custom` case and a `CustomFontSizeValue` field.
- `ApplyFontSize` helper: Handle the custom numeric value.
- No breaking changes to persistence; `Custom` font size serialises its numeric value.
