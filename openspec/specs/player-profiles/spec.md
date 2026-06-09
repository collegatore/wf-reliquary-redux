# Player Profiles Specification

## Purpose

Players who run more than one Warframe account can track each account separately via
named **profiles**. A profile is a container one level above wishlists: wishlists are
scoped to a profile, and the active profile determines which wishlists are visible.
Profile management lives in the Settings tab.

## Requirements

### Requirement: Default profile on first launch

The system SHALL create a profile named **Default** and set it as the active profile the
first time the application opens and the `profiles` table is empty.

#### Scenario: First launch with no profiles

- **WHEN** the application starts and no profiles exist in the database
- **THEN** a profile named "Default" is created and selected as the active profile

#### Scenario: Existing wishlists assigned to Default on upgrade

- **WHEN** the application starts for the first time after the profiles feature is added and wishlists already exist with no profile
- **THEN** all existing wishlists are assigned to the Default profile and remain selectable

### Requirement: Profiles section in Settings

The Settings screen SHALL contain a **Profiles** section, visually separated from the
other settings sections, that displays the active profile and lets the user switch to
another profile. The Profiles section SHALL be the only place profile management actions
appear; the Prime Gear tab SHALL NOT host profile controls.

#### Scenario: Profiles section is shown in Settings

- **WHEN** the user opens the Settings screen
- **THEN** a Profiles section is visible, separated from the Font size and Source URL sections, showing the active profile's name

#### Scenario: User switches the active profile

- **WHEN** the user selects a different profile in the Profiles section
- **THEN** that profile becomes active and the wishlist selector and gear/relic/mission views reload to reflect the new profile's wishlists

### Requirement: Active profile shown in footer

The application SHALL display the active profile's name in a footer at the bottom of the
main window, labeled as `Profile: <name>`, visible regardless of which tab is selected.
The footer SHALL update immediately when the active profile changes or is renamed. The
profile name portion of the footer SHALL use the same bounded display width and hover-tooltip
behavior as the Settings profile selector: an over-long name is truncated with an ellipsis and
its full value is shown in a tooltip on hover.

#### Scenario: Footer shows the active profile name

- **WHEN** the main window is displayed
- **THEN** a footer at the bottom shows "Profile: " followed by the active profile's name

#### Scenario: Footer updates when the active profile changes

- **WHEN** the user switches to or renames the active profile
- **THEN** the footer label updates immediately to reflect the current profile name

#### Scenario: Long profile name in footer is truncated with a tooltip

- **WHEN** the active profile's name is too long to fit the footer's display width
- **THEN** the footer shows the name truncated with an ellipsis, matching the Settings selector
  width, and reveals the full name in a tooltip on hover

### Requirement: Profile management actions

The Profiles section SHALL provide three actions: create a new profile, rename the active
profile, and delete the active profile.

#### Scenario: All three management actions are present

- **WHEN** the user views the Profiles section in Settings
- **THEN** actions to create a new profile, rename the active profile, and delete the active profile are available

### Requirement: Create new profile

The user SHALL be able to create a new named profile via a modal dialog. The confirmed name
MUST be valid: non-empty, unique across profiles, and within the length limit. The new profile
SHALL become the active profile immediately after creation and SHALL start with its own
default wishlist.

#### Scenario: User creates a new profile

- **WHEN** the user creates a profile and confirms the modal with a valid name
- **THEN** a new profile with that name is created, it becomes the active profile, and it has a
  Default wishlist

#### Scenario: User cancels profile creation

- **WHEN** the user opens the create-profile modal and dismisses it without confirming
- **THEN** no profile is created and the active profile is unchanged

### Requirement: Rename profile

The user SHALL be able to rename the currently active profile via a modal dialog
pre-filled with the current name. The confirmed name MUST be valid: non-empty, unique across
the other profiles, and within the length limit.

#### Scenario: User renames the active profile

- **WHEN** the user chooses rename and confirms the modal with a valid new name
- **THEN** the active profile is renamed and the Profiles section label updates immediately

#### Scenario: User cancels rename

- **WHEN** the user opens the rename modal and dismisses it without confirming
- **THEN** the profile name is unchanged

### Requirement: Delete profile

The user SHALL be able to delete the currently active profile. When deleted, all of the
profile's wishlists and their items SHALL be permanently removed and the application SHALL
automatically activate another existing profile. Deletion SHALL be prevented when only one
profile remains.

#### Scenario: User deletes a profile when multiple exist

- **WHEN** the user deletes the active profile and at least one other profile exists
- **THEN** the profile and all its wishlists and items are permanently removed, and another profile becomes active

#### Scenario: Delete is disabled when only one profile remains

- **WHEN** only one profile exists
- **THEN** the delete action is disabled and cannot be selected

### Requirement: Active profile persisted across sessions

The system SHALL remember the active profile when the application is closed and restore it
on the next launch. Each profile SHALL remember its own last-active wishlist.

#### Scenario: App restores last active profile on restart

- **WHEN** the user closes the application with profile "Alt Account" active and then reopens it
- **THEN** "Alt Account" is the active profile on the next launch

#### Scenario: Per-profile active wishlist is restored on switch

- **WHEN** the user switches away from a profile and later switches back to it
- **THEN** the wishlist that was active in that profile is re-selected

#### Scenario: Last active profile was deleted before restart

- **WHEN** the previously active profile no longer exists on next launch
- **THEN** the system activates any remaining profile without error

### Requirement: Unique profile names

Profile names SHALL be unique across all profiles. Uniqueness SHALL be evaluated on the
trimmed name, case-insensitively, so that "Alt" and "alt " are treated as the same name.
The system SHALL reject creating or renaming a profile to a name already used by another
profile.

#### Scenario: Creating a profile with a taken name is rejected

- **WHEN** the user confirms the create-profile modal with a name that, trimmed and compared
  case-insensitively, matches an existing profile
- **THEN** no profile is created and the modal reports that the name is already in use

#### Scenario: Renaming to another profile's name is rejected

- **WHEN** the user confirms the rename modal with a name matching a different existing profile
- **THEN** the profile is not renamed and the modal reports that the name is already in use

#### Scenario: Renaming a profile to its own current name is allowed

- **WHEN** the user confirms the rename modal without changing the name (or changing only case
  or surrounding whitespace)
- **THEN** the confirmation is accepted and treated as a no-op rename

### Requirement: Profile name length limit

Profile names SHALL be limited to a maximum length (the same "reasonably long" cap applied to
wishlist names). The create/rename modal SHALL prevent entering or confirming a name longer
than this limit.

#### Scenario: Name at the limit is accepted

- **WHEN** the user enters a profile name whose trimmed length is at most the maximum
- **THEN** the modal accepts the name

#### Scenario: Over-long name cannot be confirmed

- **WHEN** the user attempts to enter or confirm a profile name longer than the maximum
- **THEN** the modal does not accept it and the input is constrained to the limit

### Requirement: Profile name modal validation feedback

The create/rename modal for profiles SHALL validate the entered name against the non-empty,
uniqueness, and length rules, show an inline message when the name is invalid, and remain open
until the name is valid or the user cancels.

#### Scenario: Invalid name shows inline error and blocks confirmation

- **WHEN** the entered profile name is empty, a duplicate, or over-long
- **THEN** the modal shows an inline validation message and does not close on confirm

#### Scenario: Correcting the name clears the error

- **WHEN** the user edits an invalid name so it becomes valid
- **THEN** the inline message clears and the name can be confirmed

### Requirement: Profile selector truncates long names

The profile selector button in the Settings Profiles section SHALL display the active
profile's name within a bounded width, truncating an over-long name with an ellipsis and
showing the full name in a tooltip on hover.

#### Scenario: Long profile name is truncated with a tooltip

- **WHEN** the active profile's name is too long to fit the selector button
- **THEN** the button shows a truncated name with an ellipsis and reveals the full name in a
  tooltip when hovered
