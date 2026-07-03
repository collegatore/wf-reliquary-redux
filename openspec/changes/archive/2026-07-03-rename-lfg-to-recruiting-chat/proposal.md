## Why

The term "LFG" (Looking for Group) is internal jargon that may not be immediately recognizable to all Warframe players. "Recruiting Chat message" is the in-game label for the channel where these messages are posted, making the feature's purpose self-evident to new users.

## What Changes

- All user-visible strings showing "LFG message" are renamed to "Recruiting Chat message"
- The Settings section heading "LFG message constructor" becomes "Recruiting Chat message constructor"
- The menu item "Compose LFG message…" becomes "Compose Recruiting Chat message…"
- The dialog title "Compose LFG message" becomes "Compose Recruiting Chat message"
- The spec and documentation references are updated to match the new terminology
- All code identifiers carrying the `Lfg` or `LfgMessage` prefix are renamed to the `RecruitingMessage` prefix: class names, file names, the `Application/Lfg/` namespace folder, and the `LfgTemplate` property in `UserSettings` and the settings-store DTOs

## Capabilities

### New Capabilities

_(none)_

### Modified Capabilities

- `lfg-message-constructor`: User-visible labels for the feature rename from "LFG message" to "Recruiting Chat message"; no behavior or requirement changes

## Impact

- UI string literals in `LfgMessageDialog.axaml`, `RelicsPanelView.axaml`, `App.axaml`, and `SettingsView.axaml`
- Application-layer files and namespace: `Application/Lfg/` → `Application/RecruitingMessage/`, all 5 class files renamed
- Presentation-layer files: `LfgMessageDialog.*`, `LfgMessageDialogViewModel.cs`, `LfgTemplateEditorViewModel.cs` renamed
- `UserSettings` record and `LocalUserSettingsStore` private DTOs: `LfgTemplate` property and `LfgTemplateFile`/`LfgGroupFile`/`LfgOptionFile` types renamed
- Test files: `LfgTemplateEditorViewModelTests.cs`, `LfgMessageBuilderTests.cs`, `LfgPatternParserTests.cs` renamed
- All call sites throughout the presentation and infrastructure layers updated
- `openspec/specs/lfg-message-constructor/spec.md` — heading and narrative text
- `docs/technical-reference.md` — section heading and description
