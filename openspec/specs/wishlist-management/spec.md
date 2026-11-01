# Wishlist Management Specification

## Purpose

Wishlists let the user track which prime parts they want to farm. Wishlists are scoped to
the active player profile: the selector lists only the active profile's wishlists, and the
active wishlist is remembered per profile. This spec covers wishlist creation, renaming,
deletion, selection, and persistence.

## Requirements

### Requirement: Default wishlist on first launch

The system SHALL create a wishlist named **Default** within the active profile and set it as the active wishlist the first time a profile has no wishlists.

#### Scenario: First launch with no wishlists

- **WHEN** the application starts, a profile is active, and that profile has no wishlists
- **THEN** a wishlist named "Default" is created in that profile and selected as the active wishlist

### Requirement: Wishlist selector control

The Prime Gear tab SHALL display a wishlist selector row above the gear cards. The selector SHALL show the name of the currently active wishlist and allow the user to switch to another wishlist by selecting it from a dropdown. The selector SHALL list only the wishlists belonging to the active profile, and SHALL list no actions (see the `selector-actions-menu` capability).

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

The user SHALL be able to create a new named wishlist via a modal dialog. The confirmed name
MUST be valid: non-empty, unique within the active profile, and within the length limit. The
new wishlist SHALL belong to the active profile and SHALL become the active wishlist
immediately after creation.

#### Scenario: User creates a new wishlist

- **WHEN** the user selects "New list" in the Wishlist actions menu and confirms the modal with a valid name
- **THEN** a new wishlist with that name is created in the active profile, it becomes the active
  wishlist, and it appears in the dropdown

#### Scenario: User cancels new wishlist creation

- **WHEN** the user selects "New list" in the Wishlist actions menu and dismisses the modal without confirming
- **THEN** no wishlist is created and the active wishlist is unchanged

### Requirement: Rename wishlist

The user SHALL be able to rename the currently active wishlist via a modal dialog pre-filled
with the current name. The confirmed name MUST be valid: non-empty, unique within the active
profile (excluding the wishlist being renamed), and within the length limit.

#### Scenario: User renames a wishlist

- **WHEN** the user selects "Rename this list…" and confirms the modal with a valid new name
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

- **WHEN** the user opens the Wishlist actions menu and only one wishlist exists
- **THEN** the "Delete this list" action is disabled and cannot be selected

### Requirement: Last active wishlist persisted across sessions

The system SHALL remember the active wishlist per profile when the application is closed and restore it on the next launch.

#### Scenario: App restores last active wishlist on restart

- **WHEN** the user closes the application with wishlist "Farm Set A" active in the current profile and then reopens it
- **THEN** "Farm Set A" is the active wishlist for that profile on the next launch

#### Scenario: Last wishlist was deleted before restart

- **WHEN** the previously active wishlist of the active profile no longer exists on next launch
- **THEN** the system activates any remaining wishlist in that profile without error

### Requirement: Wishlists scoped to a profile

Every wishlist SHALL belong to exactly one profile. Wishlist queries and creation SHALL be scoped to the active profile, and deleting a profile SHALL permanently remove its wishlists and their items.

#### Scenario: Wishlists are isolated between profiles

- **WHEN** the user switches from a profile containing wishlist "A" to a different profile
- **THEN** wishlist "A" is not listed in the selector for the second profile

#### Scenario: Deleting a profile removes its wishlists

- **WHEN** a profile is deleted
- **THEN** all wishlists belonging to that profile and their items are permanently removed

### Requirement: Unique wishlist names within a profile

Wishlist names SHALL be unique within their owning profile. Uniqueness SHALL be evaluated on
the trimmed name, case-insensitively. Two profiles MAY each have a wishlist with the same name;
the constraint applies only within a single profile. The system SHALL reject creating or
renaming a wishlist to a name already used by another wishlist in the same profile.

#### Scenario: Creating a wishlist with a name taken in the same profile is rejected

- **WHEN** the user confirms the create-wishlist modal with a name that, trimmed and compared
  case-insensitively, matches another wishlist in the active profile
- **THEN** no wishlist is created and the modal reports that the name is already in use

#### Scenario: Same name allowed in a different profile

- **WHEN** profile A has a wishlist named "Farm Set" and the user creates a wishlist named
  "Farm Set" while profile B is active
- **THEN** the wishlist is created in profile B

#### Scenario: Renaming to another wishlist's name in the same profile is rejected

- **WHEN** the user confirms the rename modal with a name matching a different wishlist in the
  active profile
- **THEN** the wishlist is not renamed and the modal reports that the name is already in use

#### Scenario: Renaming a wishlist to its own current name is allowed

- **WHEN** the user confirms the rename modal without changing the name (or changing only case
  or surrounding whitespace)
- **THEN** the confirmation is accepted and treated as a no-op rename

### Requirement: Wishlist name length limit

Wishlist names SHALL be limited to a maximum length (the same "reasonably long" cap applied to
profile names). The create/rename modal SHALL prevent entering or confirming a name longer than
this limit.

#### Scenario: Name at the limit is accepted

- **WHEN** the user enters a wishlist name whose trimmed length is at most the maximum
- **THEN** the modal accepts the name

#### Scenario: Over-long name cannot be confirmed

- **WHEN** the user attempts to enter or confirm a wishlist name longer than the maximum
- **THEN** the modal does not accept it and the input is constrained to the limit

### Requirement: Wishlist name modal validation feedback

The create/rename modal for wishlists SHALL validate the entered name against the non-empty,
uniqueness, and length rules, show an inline message when the name is invalid, and remain open
until the name is valid or the user cancels.

#### Scenario: Invalid name shows inline error and blocks confirmation

- **WHEN** the entered wishlist name is empty, a duplicate within the profile, or over-long
- **THEN** the modal shows an inline validation message and does not close on confirm

#### Scenario: Correcting the name clears the error

- **WHEN** the user edits an invalid name so it becomes valid
- **THEN** the inline message clears and the name can be confirmed

### Requirement: Wishlist selector truncates long names

The wishlist selector button SHALL display the active wishlist's name within a bounded width,
truncating an over-long name with an ellipsis and showing the full name in a tooltip on hover.

#### Scenario: Long wishlist name is truncated with a tooltip

- **WHEN** the active wishlist's name is too long to fit the selector button
- **THEN** the button shows a truncated name with an ellipsis and reveals the full name in a
  tooltip when hovered

### Requirement: Wishlist actions menu

The Wishlist selector SHALL be followed by an actions button, as defined by the `selector-actions-menu` capability, whose menu contains **Rename this list…**, **Delete this list**, and **New list**.

#### Scenario: All three actions are in the menu

- **WHEN** the user presses the Wishlist actions button
- **THEN** the menu shows "Rename this list…", "Delete this list", and "New list"
