## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: Create new wishlist

The user SHALL be able to create a new named wishlist via a modal dialog. The confirmed name
MUST be valid: non-empty, unique within the active profile, and within the length limit. The
new wishlist SHALL belong to the active profile and SHALL become the active wishlist
immediately after creation.

#### Scenario: User creates a new wishlist

- **WHEN** the user selects "+ New list" and confirms the modal with a valid name
- **THEN** a new wishlist with that name is created in the active profile, it becomes the active
  wishlist, and it appears in the dropdown

#### Scenario: User cancels new wishlist creation

- **WHEN** the user selects "+ New list" and dismisses the modal without confirming
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
