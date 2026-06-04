## ADDED Requirements

### Requirement: Checked state reflects active wishlist membership
A part checkbox SHALL be checked if and only if that part is present in the currently active wishlist. Checking a part SHALL add it to the active wishlist immediately. Unchecking SHALL remove it immediately. This state SHALL persist across application restarts and drop-table re-imports.

#### Scenario: Part in active wishlist appears checked
- **WHEN** the Prime Gear tab loads with an active wishlist that contains "Ash Prime Neuroptics"
- **THEN** the Neuroptics checkbox on the Ash Prime card is checked

#### Scenario: Part not in active wishlist appears unchecked
- **WHEN** the Prime Gear tab loads with an active wishlist that does not contain "Ash Prime Blueprint"
- **THEN** the Blueprint checkbox on the Ash Prime card is unchecked

#### Scenario: Checking a part persists across restart
- **WHEN** the user checks "Ash Prime Neuroptics" and then restarts the application
- **THEN** the Neuroptics checkbox is still checked on next launch

#### Scenario: Checking a part adds it to the active wishlist immediately
- **WHEN** the user checks a part checkbox
- **THEN** that part is saved to the active wishlist without any additional user action

#### Scenario: Unchecking a part removes it from the active wishlist immediately
- **WHEN** the user unchecks a previously checked part checkbox
- **THEN** that part is removed from the active wishlist without any additional user action

#### Scenario: Switching wishlist reloads checkbox states
- **WHEN** the user switches to a different wishlist
- **THEN** all part checkboxes update to reflect that wishlist's contents

### Requirement: Part identity is stable across imports
Wishlist items SHALL be stored using the part's `normalized_name` as the identifier, not the database integer ID, so that wishlist contents survive drop-table re-imports.

#### Scenario: Checked part remains checked after re-import
- **WHEN** the user has "Ash Prime Neuroptics" checked and then triggers a drop-table import
- **THEN** the Neuroptics checkbox is still checked after the import completes

### Requirement: Ghost item display
If a part is in the active wishlist but is absent from the current drop-table import, it SHALL still appear on the relevant gear card with a distinct visual marker and a tooltip explaining the situation. Ghost items SHALL remain in the wishlist until the user manually unchecks them.

#### Scenario: Ghost part appears on gear card with marker
- **WHEN** a part is in the active wishlist but "Ash Prime Neuroptics" does not appear in the current import
- **THEN** an "Ash Prime Neuroptics" row is still visible on the Ash Prime card with a distinct visual marker

#### Scenario: Ghost part shows explanatory tooltip
- **WHEN** the user hovers over a ghost part row
- **THEN** a tooltip reads "This part is not present in the current drop table import"

#### Scenario: Ghost part is not auto-removed on import
- **WHEN** a drop-table import runs and a wishlisted part is absent from the new import
- **THEN** the part remains in the wishlist and continues to appear as a ghost item

#### Scenario: User can manually remove a ghost part
- **WHEN** the user unchecks a ghost part
- **THEN** it is removed from the active wishlist and disappears from the gear card

#### Scenario: Ghost part resolves after import brings it back
- **WHEN** a subsequent drop-table import includes a part that was previously a ghost
- **THEN** the part is no longer marked as a ghost and appears as a normal checked item
