# Spec Delta

## MODIFIED Requirements

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

### Requirement: Delete wishlist

The user SHALL be able to delete the currently active wishlist. When deleted, the application SHALL automatically activate another existing wishlist. Deletion SHALL be prevented when only one wishlist remains.

#### Scenario: User deletes a wishlist when multiple exist

- **WHEN** the user selects "Delete this list" and at least one other wishlist exists
- **THEN** the active wishlist and all its items are permanently removed, and another wishlist becomes active

#### Scenario: Delete is disabled when only one wishlist remains

- **WHEN** the user opens the Wishlist actions menu and only one wishlist exists
- **THEN** the "Delete this list" action is disabled and cannot be selected

## ADDED Requirements

### Requirement: Wishlist actions menu

The Wishlist selector SHALL be followed by an actions button, as defined by the `selector-actions-menu` capability, whose menu contains **Rename this list…**, **Delete this list**, and **New list**.

#### Scenario: All three actions are in the menu

- **WHEN** the user presses the Wishlist actions button
- **THEN** the menu shows "Rename this list…", "Delete this list", and "New list"

## REMOVED Requirements

### Requirement: Wishlist management actions in dropdown

**Reason**: The actions move out of the selector's dropdown into a separate actions menu, so the selector can be a stock dropdown that lists only wishlists.
**Migration**: See "Wishlist actions menu" in this capability and the `selector-actions-menu` capability.
