## 1. Application — domain models

- [x] 1.1 Add `LfgOption` record (`Descriptor`, `Value`) under `Application/Lfg/`
- [x] 1.2 Add `LfgOptionGroup` record (`Placeholder`, options list, default option value/identity)
- [x] 1.3 Add `LfgMessageTemplate` record (`Pattern`, `Groups`) with a static `Default` (pattern `{participation} [{relic_name}] {refinement}`; `participation`: Hosting→H, Looking for group→LF, default LF; `refinement`: Intact→inta, Exceptional→exceptional, Flawless→flawless, Radiant→rad, default rad)

## 2. Application — pattern engine

- [x] 2.1 Implement `LfgPatternParser.Validate`: single forward scan reporting unclosed / nested / empty / stray-`}` / invalid-name-char with positions; returns placeholder names in order on success
- [x] 2.2 Add the semantic rule to validation: pattern must contain `{relic_name}`
- [x] 2.3 Define the system-name set `{ relic_name, relic_reward }`; classify non-system placeholders as custom groups
- [x] 2.4 Implement `LfgPatternParser.Reconcile(newPattern, currentGroups, savedGroups)`: validate; for each custom placeholder (in appearance order) keep the current draft group if present, else restore from saved group if available, else create an empty group; drop groups whose placeholder vanished; on failure return the error and leave the input groups unchanged
- [x] 2.5 Implement `LfgMessageBuilder`: substitute `{relic_name}` with the supplied relic name and each custom placeholder with its selected option value via `Regex.Replace`; empty group → empty string (defensive)

## 3. Application — settings contract

- [x] 3.1 Extend `UserSettings` with an `LfgMessageTemplate` field; default to `LfgMessageTemplate.Default`
- [x] 3.2 Confirm `IUserSettingsStore` carries the template through load/save (no signature change needed if it flows via `UserSettings`)

## 4. Infrastructure — persistence

- [x] 4.1 Add a nested template object to `LocalUserSettingsStore.SettingsFile` (placeholder + groups + options), serialized with the existing `JsonSerializerOptions`
- [x] 4.2 Map to/from `LfgMessageTemplate` in `LoadAsync`/`SaveAsync`; a missing/empty stored template yields `LfgMessageTemplate.Default`

## 5. Composition

- [x] 5.1 Wire any new construction needed for the editor/dialog in `ReliquaryServiceFactory` (template load/save flows through existing `IUserSettingsStore`)

## 6. Presentation — LFG message popup

- [x] 6.1 Add `LfgMessageDialog` (modal `Window`) + `LfgMessageDialog.axaml`, mirroring `WishlistNameDialog`, with `ShowAsync(owner, template, relicName)`
- [x] 6.2 Add `LfgMessageDialogViewModel`: build one selector per custom group (pre-select default), expose live `Preview` recomputed via `LfgMessageBuilder` on selection change
- [x] 6.3 Add a copy-to-clipboard command using the `CopyLogPath`-style clipboard delegate injected from `MainWindow`

## 7. Presentation — relic card compose action

- [x] 7.1 Add a top-right `⋮` overflow menu to the relic-card `DataTemplate` in `MainWindow.axaml`, mirroring the prime gear card menu (`ColumnDefinitions="*,Auto"` header, transparent borderless `⋮` button, `MenuFlyout Placement="BottomEdgeAlignedRight"`), with a "Compose LFG message…" `MenuItem`
- [x] 7.2 Have `WishlistRelicCardViewModel` expose the compose command/request carrying its relic name; `MainWindow` handles it to open `LfgMessageDialog` with the active template

## 8. Presentation — Settings template editor

- [x] 8.1 Add an `LfgTemplateEditorViewModel` holding the template as a draft with a saved baseline (mirror `SettingsViewModel`'s draft/saved/dirty pattern)
- [x] 8.2 Add a Settings sub-section in `SettingsView.axaml`: pattern text box, Parse button, dynamic per-group editors (options with descriptor + value, default ComboBox, add/remove), validation/warning text, Restore defaults (no separate LFG Save button — save is via main Settings Save)
- [x] 8.3 Wire Parse → `Reconcile`, showing validation errors and the empty-group warning; block `LfgTemplateEditorViewModel.SaveCommand` while any custom group is empty
- [x] 8.4 Wire LFG template through main Settings Save: `SettingsViewModel` receives `GetCurrentLfgTemplate` and `IsLfgTemplateDirty` delegates; `MainWindowShellViewModel.OnSettingsSaved` calls `LfgEditor.NotifySaved` after save to clear dirty state; Restore defaults → set draft to `LfgMessageTemplate.Default` (persisted only on main Settings Save)
- [x] 8.5 Hide single-option groups from the popup (`LfgMessageDialogViewModel.Selectors` exposes only groups with >1 option; `allSelectors` retains all for substitution)
- [x] 8.6 Show template pattern in popup above selectors
- [x] 8.7 Replace computed `DraftMatchesSaved()` dirty check with explicit `isDirty` flag in `LfgTemplateEditorViewModel`; add null guard on `SelectedDefaultOption.set` to prevent Avalonia ComboBox initialization write-backs from triggering spurious dirty state

## 9. Tests

- [x] 9.1 Unit: `LfgPatternParser.Validate` — valid pattern; unclosed; nested; empty; stray `}`; invalid char; missing `{relic_name}` (assert positions/messages)
- [x] 9.2 Unit: `LfgPatternParser.Reconcile` — preserve matching draft group; restore from saved when draft dropped it; empty group when absent from both; drop removed; appearance ordering; failed-parse leaves groups unchanged
- [x] 9.3 Unit: `LfgMessageBuilder` — default template for `Axi A1 Relic` yields `LF [Axi A1 Relic] rad`; option value (not descriptor) substituted; empty group → empty string
- [x] 9.4 Unit: `LfgMessageTemplate.Default` shape (pattern, groups, defaults)
- [x] 9.5 Integration: `LocalUserSettingsStore` template round-trip; missing template loads `Default`
- [x] 9.6 Integration: editor viewmodel — Parse reconciles draft, empty group blocks Save, Save persists, Restore defaults resets draft without persisting

## 10. Docs

- [x] 10.1 Note the LFG message constructor in the README/technical-reference as appropriate, including the pre-V1 settings-format note (existing `user-settings.json` picks up defaults; no migration)
