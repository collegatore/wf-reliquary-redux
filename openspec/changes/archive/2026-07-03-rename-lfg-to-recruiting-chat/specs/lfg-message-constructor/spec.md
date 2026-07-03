## RENAMED Requirements

### Requirement: LFG message popup
FROM: LFG message popup
TO: Recruiting Chat message popup

## MODIFIED Requirements

### Requirement: Relic card compose action

Each relic card on the Relics and Missions tab SHALL present an overflow menu in its top-right corner, shown the same way as the prime gear cards' menu (a `⋮` button opening a menu flyout). The menu SHALL contain a compose item that opens the Recruiting Chat message popup pre-filled for that relic. The relic identity passed to the popup SHALL be the relic's full display name (including the `Relic` suffix, e.g. `Axi A1 Relic`).

#### Scenario: Overflow menu visible on each card

- **WHEN** the Relics and Missions tab renders relic cards
- **THEN** every relic card shows a `⋮` overflow-menu button in its top-right corner, consistent with the prime gear cards

#### Scenario: Opening the popup for a relic

- **WHEN** the user opens a relic card's overflow menu and selects the compose item
- **THEN** the Recruiting Chat message popup opens as a modal child of the main window
- **AND** the popup is initialized with that relic's full display name as the `{relic_name}` value

### Requirement: Template editor draft, save, and restore

The Settings template editor SHALL maintain edits as a draft that is not propagated to the persisted configuration until the user saves. There is no separate Save button for the LFG editor; the LFG template is persisted together with all other settings when the user presses the main Settings Save button. The default field for each option group SHALL be selected via a dropdown (not a free-text field) whose items are the group's current options. Restoring defaults SHALL reset the draft to the in-code default template without persisting until saved. The LFG editor's save action SHALL be blocked while any custom option group has no options (empty group), whereas a parse SHALL surface an empty group as a non-blocking warning in Settings; the Settings Save button reflects the combined dirty state of all settings including the LFG editor.

A **Reload from saved** action SHALL be available in the Recruiting Chat message section. Activating it SHALL reset the draft pattern and option groups to the last-persisted template (the template on disk at the time the Settings tab was opened, or at the time of the last Save). This action SHALL NOT persist anything; it only reverts the in-memory draft.

#### Scenario: Edits are not persisted before save

- **WHEN** the user edits the pattern or option groups but does not save
- **THEN** the persisted configuration and the popup behavior are unaffected

#### Scenario: Save persists the template via main Settings Save

- **WHEN** the user presses the main Settings Save button with a valid Recruiting Chat message template
- **THEN** the pattern and option groups are written to the settings file together with the other settings
- **AND** subsequent popups use the saved template
- **AND** the Settings Save button disables and the leave-guard considers the page clean

#### Scenario: Default value selected via dropdown

- **WHEN** the editor shows an option group
- **THEN** the default value field is a dropdown listing the group's current options
- **AND** selecting an option from the dropdown sets it as the group default

#### Scenario: Restore defaults resets the draft

- **WHEN** the user activates Restore defaults
- **THEN** the draft pattern and option groups become the in-code default template
- **AND** the change is not persisted until the user presses the main Settings Save button

#### Scenario: Reload from saved resets the draft to persisted template

- **WHEN** the user activates Reload from saved
- **THEN** the draft pattern and option groups become the last-persisted template
- **AND** the Recruiting Chat message editor no longer has unsaved changes (the draft matches the saved state)
- **AND** the change is not persisted (nothing is written to storage)

#### Scenario: Reload from saved clears parse errors

- **WHEN** the user activates Reload from saved while a parse error or warning is displayed
- **THEN** the error and warning messages are cleared

#### Scenario: Empty group warns on parse but blocks save

- **WHEN** the draft contains a custom option group with no options
- **THEN** a parse surfaces a non-blocking warning identifying the empty group
- **AND** save is blocked until the group has at least one option
