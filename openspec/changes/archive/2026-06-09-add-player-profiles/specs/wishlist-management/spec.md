## MODIFIED Requirements

### Requirement: Default wishlist on first launch

The system SHALL create a wishlist named **Default** within the active profile and set it
as the active wishlist the first time a profile has no wishlists.

#### Scenario: First launch with no wishlists

- **WHEN** the application starts, a profile is active, and that profile has no wishlists
- **THEN** a wishlist named "Default" is created in that profile and selected as the active wishlist

### Requirement: Wishlist selector control

The Prime Gear tab SHALL display a wishlist selector row above the gear cards. The
selector SHALL show the name of the currently active wishlist and allow the user to switch
to another wishlist by selecting it from a dropdown. The selector SHALL list only the
wishlists belonging to the active profile.

#### Scenario: Selector shows active wishlist name

- **WHEN** the Prime Gear tab is displayed
- **THEN** the wishlist selector shows the name of the currently active wishlist

#### Scenario: User switches active wishlist

- **WHEN** the user opens the dropdown and selects a different wishlist
- **THEN** that wishlist becomes active, the selector updates its label, and the gear cards reload to reflect the new wishlist's contents

#### Scenario: Selector lists only the active profile's wishlists

- **WHEN** the user opens the wishlist selector dropdown
- **THEN** only wishlists belonging to the active profile are listed

### Requirement: Create new wishlist

The user SHALL be able to create a new named wishlist via a modal dialog. The new wishlist
SHALL belong to the active profile and SHALL become the active wishlist immediately after
creation.

#### Scenario: User creates a new wishlist

- **WHEN** the user selects "+ New list" and confirms the modal with a non-empty name
- **THEN** a new wishlist with that name is created in the active profile, it becomes the active wishlist, and it appears in the dropdown

#### Scenario: User cancels new wishlist creation

- **WHEN** the user selects "+ New list" and dismisses the modal without confirming
- **THEN** no wishlist is created and the active wishlist is unchanged

### Requirement: Last active wishlist persisted across sessions

The system SHALL remember the active wishlist per profile when the application is closed
and restore it on the next launch.

#### Scenario: App restores last active wishlist on restart

- **WHEN** the user closes the application with wishlist "Farm Set A" active in the current profile and then reopens it
- **THEN** "Farm Set A" is the active wishlist for that profile on the next launch

#### Scenario: Last wishlist was deleted before restart

- **WHEN** the previously active wishlist of the active profile no longer exists on next launch
- **THEN** the system activates any remaining wishlist in that profile without error

## ADDED Requirements

### Requirement: Wishlists scoped to a profile

Every wishlist SHALL belong to exactly one profile. Wishlist queries and creation SHALL be
scoped to the active profile, and deleting a profile SHALL permanently remove its
wishlists and their items.

#### Scenario: Wishlists are isolated between profiles

- **WHEN** the user switches from a profile containing wishlist "A" to a different profile
- **THEN** wishlist "A" is not listed in the selector for the second profile

#### Scenario: Deleting a profile removes its wishlists

- **WHEN** a profile is deleted
- **THEN** all wishlists belonging to that profile and their items are permanently removed
