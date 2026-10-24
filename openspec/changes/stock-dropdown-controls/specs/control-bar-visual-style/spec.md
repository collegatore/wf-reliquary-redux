# Spec Delta

## MODIFIED Requirements

### Requirement: Wishlist selector carries its label inside the control
The wishlist selector on the Prime Gear primary bar SHALL show a muted "Wishlist:" caption inside its closed field, before the active wishlist's name, which SHALL be shown in regular text. The caption SHALL NOT appear on the entries of the open list. No separate "Wishlist" label SHALL appear outside the selector. Over-long names SHALL keep truncating with an ellipsis and a hover tooltip.

#### Scenario: Selector shows caption and name in one button
- **WHEN** the Prime Gear tab is displayed and the active wishlist is "Default"
- **THEN** the selector's field reads "Wishlist:" in muted text followed by "Default" in regular text, and no "Wishlist" label appears outside it

#### Scenario: Caption does not open anything separately
- **WHEN** the user clicks the "Wishlist:" caption inside the selector
- **THEN** the wishlist dropdown opens, exactly as when clicking the wishlist name

#### Scenario: Open list shows names only
- **WHEN** the user opens the Wishlist selector
- **THEN** the listed entries show wishlist names without the "Wishlist:" caption

## ADDED Requirements

### Requirement: Dropdown lists open left-aligned under their field
Every dropdown in the application SHALL open its list just below its field, separated by a small gap, with the list's left edge aligned to the field's left edge. A list wider than its field SHALL extend to the right, never centered on the field. Over-long entry names SHALL truncate with an ellipsis and show the full name in a hover tooltip.

#### Scenario: Wide list starts at the field's left edge
- **WHEN** a wishlist's name is longer than the Wishlist selector field and the user opens the selector
- **THEN** the list opens below the field with a small gap, starting at the field's left edge, and the long name is truncated with an ellipsis
