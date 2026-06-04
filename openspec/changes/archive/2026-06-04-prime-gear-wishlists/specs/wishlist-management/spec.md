## ADDED Requirements

### Requirement: Default wishlist on first launch
The system SHALL create a wishlist named **Default** and set it as the active wishlist the first time the application opens and the `wishlists` table is empty.

#### Scenario: First launch with no wishlists
- **WHEN** the application starts and no wishlists exist in the database
- **THEN** a wishlist named "Default" is created and selected as the active wishlist

### Requirement: Wishlist selector control
The Prime Gear tab SHALL display a wishlist selector row above the gear cards. The selector SHALL show the name of the currently active wishlist and allow the user to switch to another wishlist by selecting it from a dropdown.

#### Scenario: Selector shows active wishlist name
- **WHEN** the Prime Gear tab is displayed
- **THEN** the wishlist selector shows the name of the currently active wishlist

#### Scenario: User switches active wishlist
- **WHEN** the user opens the dropdown and selects a different wishlist
- **THEN** that wishlist becomes active, the selector updates its label, and the gear cards reload to reflect the new wishlist's contents

### Requirement: Wishlist management actions in dropdown
The wishlist selector dropdown SHALL include a visually separated bottom section containing three actions: **Rename this list…**, **Delete this list**, and **+ New list**.

#### Scenario: Bottom section is separated from list entries
- **WHEN** the user opens the wishlist selector dropdown
- **THEN** a visual divider separates the list of wishlist names from the management actions

#### Scenario: All three management actions are present
- **WHEN** the user opens the wishlist selector dropdown
- **THEN** the actions "Rename this list…", "Delete this list", and "+ New list" are visible in the bottom section

### Requirement: Create new wishlist
The user SHALL be able to create a new named wishlist via a modal dialog. The new wishlist SHALL become the active wishlist immediately after creation.

#### Scenario: User creates a new wishlist
- **WHEN** the user selects "+ New list" and confirms the modal with a non-empty name
- **THEN** a new wishlist with that name is created, it becomes the active wishlist, and it appears in the dropdown

#### Scenario: User cancels new wishlist creation
- **WHEN** the user selects "+ New list" and dismisses the modal without confirming
- **THEN** no wishlist is created and the active wishlist is unchanged

### Requirement: Rename wishlist
The user SHALL be able to rename the currently active wishlist via a modal dialog pre-filled with the current name.

#### Scenario: User renames a wishlist
- **WHEN** the user selects "Rename this list…" and confirms the modal with a new non-empty name
- **THEN** the active wishlist is renamed and the selector label updates immediately

#### Scenario: User cancels rename
- **WHEN** the user selects "Rename this list…" and dismisses the modal without confirming
- **THEN** the wishlist name is unchanged

### Requirement: Delete wishlist
The user SHALL be able to delete the currently active wishlist. When deleted, the application SHALL automatically activate another existing wishlist. Deletion SHALL be prevented when only one wishlist remains.

#### Scenario: User deletes a wishlist when multiple exist
- **WHEN** the user selects "Delete this list" and at least one other wishlist exists
- **THEN** the active wishlist and all its items are permanently removed, and another wishlist becomes active

#### Scenario: Delete is disabled when only one wishlist remains
- **WHEN** the user opens the dropdown and only one wishlist exists
- **THEN** the "Delete this list" action is disabled and cannot be selected

### Requirement: Last active wishlist persisted across sessions
The system SHALL remember the active wishlist when the application is closed and restore it on the next launch.

#### Scenario: App restores last active wishlist on restart
- **WHEN** the user closes the application with wishlist "Farm Set A" active and then reopens it
- **THEN** "Farm Set A" is the active wishlist on the next launch

#### Scenario: Last wishlist was deleted before restart
- **WHEN** the previously active wishlist no longer exists on next launch
- **THEN** the system activates any remaining wishlist without error
