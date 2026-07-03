## 1. Application-Layer Code Rename

- [x] 1.1 Rename the folder `src/Application/WF.Reliquary.Application/Lfg/` to `RecruitingMessage/` and update the namespace declaration in each file from `WF.Reliquary.Application.Lfg` to `WF.Reliquary.Application.RecruitingMessage`
- [x] 1.2 Rename `LfgMessageBuilder.cs` → `RecruitingMessageBuilder.cs` and the class `LfgMessageBuilder` → `RecruitingMessageBuilder`
- [x] 1.3 Rename `LfgMessageTemplate.cs` → `RecruitingMessageTemplate.cs` and the class `LfgMessageTemplate` → `RecruitingMessageTemplate`
- [x] 1.4 Rename `LfgOption.cs` → `RecruitingMessageOption.cs` and the class `LfgOption` → `RecruitingMessageOption`
- [x] 1.5 Rename `LfgOptionGroup.cs` → `RecruitingMessageOptionGroup.cs` and the class `LfgOptionGroup` → `RecruitingMessageOptionGroup`
- [x] 1.6 Rename `LfgPatternParser.cs` → `RecruitingMessagePatternParser.cs` and the class `LfgPatternParser` → `RecruitingMessagePatternParser`
- [x] 1.7 In `src/Application/WF.Reliquary.Application/Settings/UserSettings.cs`, update the `using` to the new namespace and rename the property `LfgTemplate` → `RecruitingMessageTemplate` and its type reference

## 2. Infrastructure-Layer Code Rename

- [x] 2.1 In `LocalUserSettingsStore.cs`, update `using WF.Reliquary.Application.Lfg` to `WF.Reliquary.Application.RecruitingMessage`
- [x] 2.2 Rename private DTO `LfgTemplateFile` → `RecruitingMessageTemplateFile`, `LfgGroupFile` → `RecruitingMessageGroupFile`, `LfgOptionFile` → `RecruitingMessageOptionFile`
- [x] 2.3 Rename the property `SettingsFile.LfgTemplate` → `SettingsFile.RecruitingMessageTemplate`
- [x] 2.4 Update all references to the renamed types and property within `LocalUserSettingsStore.cs`

## 3. Presentation-Layer Code Rename

- [x] 3.1 Rename `LfgMessageDialog.axaml` → `RecruitingMessageDialog.axaml` and `LfgMessageDialog.axaml.cs` → `RecruitingMessageDialog.axaml.cs`; update `x:Class` attribute and code-behind class name to `RecruitingMessageDialog`
- [x] 3.2 Rename `LfgMessageDialogViewModel.cs` → `RecruitingMessageDialogViewModel.cs` and the class to `RecruitingMessageDialogViewModel`
- [x] 3.3 Rename `LfgTemplateEditorViewModel.cs` → `RecruitingMessageTemplateEditorViewModel.cs` and the class to `RecruitingMessageTemplateEditorViewModel`
- [x] 3.4 In `MainWindowShellViewModel.cs`, rename property `LfgEditor` → `RecruitingMessageEditor`, constructor parameter, and update the `LfgTemplateEditorViewModel` type reference
- [x] 3.5 In `MainWindow.axaml.cs`, update all `Lfg*` variable names and type references to the renamed types; rename `currentLfgTemplate` → `currentRecruitingMessageTemplate`, `lfgEditorVm` → `recruitingMessageEditorVm`
- [x] 3.6 In `SettingsViewModel.cs`, rename `GetCurrentLfgTemplate` → `GetCurrentRecruitingMessageTemplate`, `IsLfgTemplateDirty` → `IsRecruitingMessageTemplateDirty`, and update the `using` and type references
- [x] 3.7 In `WishlistRelicsViewModel.cs` and `WishlistRelicTableRow.cs`, update any `Lfg*` type references
- [x] 3.8 In `RelicsPanelView.axaml.cs`, update any `Lfg*` type references
- [x] 3.9 In `SettingsView.axaml`, update any `Lfg*` binding paths that reference `LfgEditor` → `RecruitingMessageEditor`

## 4. Test-File Rename

- [x] 4.1 Rename `LfgTemplateEditorViewModelTests.cs` → `RecruitingMessageTemplateEditorViewModelTests.cs` and update the class name
- [x] 4.2 Rename `LfgMessageBuilderTests.cs` → `RecruitingMessageBuilderTests.cs` and update the class name
- [x] 4.3 Rename `LfgPatternParserTests.cs` → `RecruitingMessagePatternParserTests.cs` and update the class name
- [x] 4.4 Update all `using WF.Reliquary.Application.Lfg` references in test files to `WF.Reliquary.Application.RecruitingMessage`

## 5. UI String Updates

- [x] 5.1 In `src/Presentation/WF.Reliquary.Presentation/RecruitingMessageDialog.axaml`, change the `Title` attribute from `"Compose LFG message"` to `"Compose Recruiting Chat message"`
- [x] 5.2 In `src/Presentation/WF.Reliquary.Presentation/RelicsPanelView.axaml`, change the `Header` attribute from `"Compose LFG message…"` to `"Compose Recruiting Chat message…"`
- [x] 5.3 In `src/Presentation/WF.Reliquary.Presentation/App.axaml`, change the `Header` attribute from `"Compose LFG message…"` to `"Compose Recruiting Chat message…"`
- [x] 5.4 In `src/Presentation/WF.Reliquary.Presentation/SettingsView.axaml`, change the section `TextBlock` text from `"LFG message constructor"` to `"Recruiting Chat message constructor"`

## 6. Spec Updates

- [x] 6.1 In `openspec/specs/lfg-message-constructor/spec.md`, update the file title from `# Spec: LFG Message Constructor` to `# Spec: Recruiting Chat Message Constructor`
- [x] 6.2 In `openspec/specs/lfg-message-constructor/spec.md`, rename the requirement heading `### Requirement: LFG message popup` to `### Requirement: Recruiting Chat message popup` and update the body text and scenario text that references "LFG message popup" to say "Recruiting Chat message popup"
- [x] 6.3 In `openspec/specs/lfg-message-constructor/spec.md`, update the "Relic card compose action" requirement body — change "opens the LFG message popup" to "opens the Recruiting Chat message popup"
- [x] 6.4 In `openspec/specs/lfg-message-constructor/spec.md`, update the "Template editor draft, save, and restore" requirement — change "LFG message section" to "Recruiting Chat message section"
- [x] 6.5 In `openspec/specs/relic-view-modes/spec.md`, update references to "LFG message composer" to "Recruiting Chat message composer"

## 7. Documentation Updates

- [x] 7.1 In `docs/technical-reference.md`, rename the section `## LFG Message Constructor` to `## Recruiting Chat Message Constructor` and update the description text that says "Compose LFG message…" and other "LFG message" references within the section
