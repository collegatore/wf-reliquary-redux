# Spec: Recruiting Chat Message Constructor

## Purpose

Enables users to compose Looking-for-Group (LFG) chat messages for specific relics. A template-driven engine substitutes system placeholders (relic identity) and user-selected options (participation style, refinement level) into a pattern, producing a ready-to-paste message. The template is configurable in Settings and persisted with user settings.

## Requirements

### Requirement: Relic card compose action

Each relic card on the Relics and Missions tab SHALL present an overflow menu in its top-right corner, shown the same way as the prime gear cards' menu (a `⋮` button opening a menu flyout). The menu SHALL contain a compose item that opens the Recruiting Chat message popup pre-filled for that relic. The relic identity passed to the popup SHALL be the relic's full display name (including the `Relic` suffix, e.g. `Axi A1 Relic`).

#### Scenario: Overflow menu visible on each card

- **WHEN** the Relics and Missions tab renders relic cards
- **THEN** every relic card shows a `⋮` overflow-menu button in its top-right corner, consistent with the prime gear cards

#### Scenario: Opening the popup for a relic

- **WHEN** the user opens a relic card's overflow menu and selects the compose item
- **THEN** the Recruiting Chat message popup opens as a modal child of the main window
- **AND** the popup is initialized with that relic's full display name as the `{relic_name}` value

### Requirement: Recruiting Chat message popup

The popup SHALL display the active template pattern before the selectors, so the user can see the structure they are filling in. The popup SHALL render one selector per custom option group that has more than one option, each pre-selecting that group's default option. Groups that have exactly one option SHALL be omitted from the selector list (the single value is substituted silently). The popup SHALL show a live preview of the constructed message that updates whenever a selection changes. The popup SHALL provide a copy-to-clipboard action that places the previewed message on the system clipboard.

#### Scenario: Pattern shown above selectors

- **WHEN** the popup opens
- **THEN** the active template pattern string is shown above the group selectors

#### Scenario: Selectors reflect multi-option groups only

- **WHEN** the popup opens with a template containing custom option groups
- **THEN** the popup shows one selector per group that has more than one option, listing each option by its descriptor
- **AND** each such selector pre-selects its group's default option
- **AND** groups with exactly one option produce no visible selector (the single value is substituted automatically)

#### Scenario: Live preview updates with selections

- **WHEN** the user changes a selection in any group selector
- **THEN** the preview text updates to the message produced by substituting the current selections and the relic name into the pattern

#### Scenario: Default preview for a relic

- **WHEN** the popup opens for relic `Axi A1 Relic` with the default template
- **THEN** the preview reads `LF [Axi A1 Relic] rad`

#### Scenario: Copy to clipboard

- **WHEN** the user activates the copy action
- **THEN** the current preview text is placed on the system clipboard

### Requirement: Placeholder grammar and system placeholders

The pattern SHALL use `{name}` placeholders. The engine SHALL recognize `{relic_name}` and `{relic_reward}` as system placeholders; any other placeholder name SHALL be treated as a custom option group. `{relic_name}` SHALL resolve to the relic's full display name. `{relic_reward}` SHALL be reserved (recognized as a system placeholder and never treated as a custom group) but SHALL have no selector or substituted value in this version.

#### Scenario: Relic name resolves from the card

- **WHEN** a pattern containing `{relic_name}` is used to construct a message for relic `Axi A1 Relic`
- **THEN** `{relic_name}` is replaced with `Axi A1 Relic`

#### Scenario: relic_reward is reserved, not a custom group

- **WHEN** a pattern contains `{relic_reward}`
- **THEN** the parser does not create a custom option group for it
- **AND** no selector for it appears in the editor or popup

#### Scenario: Unknown name becomes a custom group

- **WHEN** a pattern contains a placeholder whose name is not a system placeholder
- **THEN** the engine treats it as a custom option group

### Requirement: Option groups and options

A custom option group SHALL be identified by its placeholder name and SHALL contain a list of options. Each option SHALL have a descriptor (human-readable label) and a value (the text substituted into the message). A group SHALL track which of its options is the default. When a message is constructed, each custom placeholder SHALL be replaced by the value of the option selected for its group.

#### Scenario: Option substitutes its value, not its descriptor

- **WHEN** the participation group's `Looking for group` option (value `LF`) is selected
- **THEN** the `{participation}` placeholder is replaced with `LF` in the constructed message

#### Scenario: Default option is pre-selected

- **WHEN** a popup selector is created for an option group
- **THEN** the option marked as that group's default is initially selected

### Requirement: Pattern validation

The pattern parser SHALL validate the pattern structurally and reject it with a positioned, human-readable error message when any of the following occur: an unclosed placeholder (a `{` with no matching `}`), a nested placeholder (a `{` encountered before the current placeholder is closed), an empty placeholder (`{}`), an unexpected `}` with no open placeholder, or an invalid character in a placeholder name. The parser SHALL also enforce the semantic rule that a valid pattern MUST contain `{relic_name}`.

#### Scenario: Unclosed placeholder rejected

- **WHEN** a pattern contains a `{` with no matching `}`
- **THEN** validation fails with a message identifying an unclosed placeholder and its position

#### Scenario: Nested placeholder rejected

- **WHEN** a pattern contains a `{` before a previously opened placeholder is closed
- **THEN** validation fails with a message identifying nested placeholders and the position

#### Scenario: Empty placeholder rejected

- **WHEN** a pattern contains `{}`
- **THEN** validation fails with a message identifying an empty placeholder and its position

#### Scenario: Stray closing brace rejected

- **WHEN** a pattern contains a `}` with no open placeholder
- **THEN** validation fails with a message identifying an unexpected `}` and its position

#### Scenario: Missing relic_name rejected

- **WHEN** a structurally valid pattern does not contain `{relic_name}`
- **THEN** validation fails with a message stating the pattern must contain `{relic_name}`

#### Scenario: Valid pattern accepted

- **WHEN** the pattern `{participation} [{relic_name}] {refinement}` is validated
- **THEN** validation succeeds and yields the placeholder names in order of appearance

### Requirement: Parse reconciles option groups

When the user requests a parse of an edited pattern, the engine SHALL reconcile the draft option groups against the placeholders in the new pattern using three-tier lookup in order: (1) keep the current draft group if its placeholder still appears; (2) restore from the saved (persisted) group if the placeholder was absent from the draft but present in saved config; (3) create an empty group if no saved group exists either. Groups whose placeholder no longer appears in the new pattern SHALL be dropped from the draft. The resulting groups SHALL be ordered by the placeholders' order of appearance in the pattern. If the pattern fails validation, the parse SHALL make no change to the draft groups and SHALL surface the validation error.

#### Scenario: Existing draft group preserved

- **WHEN** the user parses a new pattern that still contains a placeholder present in the current draft
- **THEN** that placeholder's draft option group keeps its current options and default

#### Scenario: Re-added placeholder restored from saved config

- **WHEN** the user parses a pattern that re-introduces a placeholder that was removed from the draft but whose group still exists in the persisted configuration
- **THEN** the group is restored from saved config with its saved options and default

#### Scenario: Brand-new placeholder gets an empty group

- **WHEN** the user parses a pattern that introduces a custom placeholder not present in either the draft or the saved config
- **THEN** a new option group is created for it with no options

#### Scenario: Removed placeholder dropped

- **WHEN** the user parses a pattern that no longer contains a previously present custom placeholder
- **THEN** that placeholder's option group is removed from the draft

#### Scenario: Failed parse leaves draft unchanged

- **WHEN** the user parses a pattern that fails validation
- **THEN** the draft option groups are left unchanged
- **AND** the validation error message is shown

### Requirement: Template editor draft, save, and restore

The Settings template editor SHALL maintain edits as a draft that is not propagated to the persisted configuration until the user saves. There is no separate Save button for the Recruiting Chat message editor; the Recruiting Chat message template is persisted together with all other settings when the user presses the main Settings Save button. The default field for each option group SHALL be selected via a dropdown (not a free-text field) whose items are the group's current options. Restoring defaults SHALL reset the draft to the in-code default template without persisting until saved. The Recruiting Chat message editor's save action SHALL be blocked while any custom option group has no options (empty group), whereas a parse SHALL surface an empty group as a non-blocking warning in Settings; the Settings Save button reflects the combined dirty state of all settings including the Recruiting Chat message editor.

A **Reload from saved** action SHALL be available in the Recruiting Chat message section. Activating it SHALL reset the draft pattern and option groups to the last-persisted template (the template on disk at the time the Settings tab was opened, or at the time of the last Save). This action SHALL NOT persist anything; it only reverts the in-memory draft.

#### Scenario: Edits are not persisted before save

- **WHEN** the user edits the pattern or option groups but does not save
- **THEN** the persisted configuration and the popup behavior are unaffected

#### Scenario: Save persists the template via main Settings Save

- **WHEN** the user presses the main Settings Save button with a valid LFG template
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

### Requirement: Template persistence and defaults

The active template (pattern and option groups) SHALL be persisted in the user settings file. The default template SHALL be defined in application code. When the settings file contains no stored template, loading SHALL yield the in-code default template.

#### Scenario: Default template when none stored

- **WHEN** the settings file contains no Recruiting Chat message template
- **THEN** loading yields the in-code default: pattern `{participation} [{relic_name}] {refinement}`, a `participation` group (`Hosting`→`H`, `Looking for group`→`LF`, default `LF`), and a `refinement` group (`Intact`→`inta`, `Exceptional`→`exceptional`, `Flawless`→`flawless`, `Radiant`→`rad`, default `rad`)

#### Scenario: Stored template round-trips

- **WHEN** a template is saved and then loaded
- **THEN** the loaded pattern and option groups equal what was saved
