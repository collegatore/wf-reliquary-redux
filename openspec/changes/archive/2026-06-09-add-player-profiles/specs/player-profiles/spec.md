## ADDED Requirements

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
The footer SHALL update immediately when the active profile changes or is renamed.

#### Scenario: Footer shows the active profile name

- **WHEN** the main window is displayed
- **THEN** a footer at the bottom shows "Profile: " followed by the active profile's name

#### Scenario: Footer updates when the active profile changes

- **WHEN** the user switches to or renames the active profile
- **THEN** the footer label updates immediately to reflect the current profile name

### Requirement: Profile management actions

The Profiles section SHALL provide three actions: create a new profile, rename the active
profile, and delete the active profile.

#### Scenario: All three management actions are present

- **WHEN** the user views the Profiles section in Settings
- **THEN** actions to create a new profile, rename the active profile, and delete the active profile are available

### Requirement: Create new profile

The user SHALL be able to create a new named profile via a modal dialog. The new profile
SHALL become the active profile immediately after creation and SHALL start with its own
default wishlist.

#### Scenario: User creates a new profile

- **WHEN** the user creates a profile and confirms the modal with a non-empty name
- **THEN** a new profile with that name is created, it becomes the active profile, and it has a Default wishlist

#### Scenario: User cancels profile creation

- **WHEN** the user opens the create-profile modal and dismisses it without confirming
- **THEN** no profile is created and the active profile is unchanged

### Requirement: Rename profile

The user SHALL be able to rename the currently active profile via a modal dialog
pre-filled with the current name.

#### Scenario: User renames the active profile

- **WHEN** the user chooses rename and confirms the modal with a new non-empty name
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
