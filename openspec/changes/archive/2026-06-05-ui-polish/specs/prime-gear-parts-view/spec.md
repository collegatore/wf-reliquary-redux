## MODIFIED Requirements

### Requirement: Prime gear cards display
The Prime Gear tab SHALL display cards in two spatially separate sections: an **Active** section at the top containing non-vaulted gear cards, and a **Vaulted** section below containing vaulted gear cards. The **Show vaulted gear** checkbox SHALL appear in the wishlist-selector panel at the top of the tab. A visually distinct **"Vaulted"** section header SHALL appear between the Active and Vaulted card areas and SHALL only be visible when Show vaulted gear is checked. The Vaulted section SHALL be hidden by default. Each card SHALL show the gear name as its title and a checkbox list of component part labels. The card display set is the union of parts present in the current import and parts in the active wishlist that are absent from the current import (ghost items). The cards view SHALL reflect the current state of the database without requiring an application restart after a successful import.

#### Scenario: Card is shown for each prime gear entry
- **WHEN** the prime gear query returns N gear entries
- **THEN** N cards are visible across both sections of the Prime Gear tab, one per entry

#### Scenario: Non-vaulted cards appear in the Active section
- **WHEN** the Prime Gear tab is displayed
- **THEN** all non-vaulted gear cards appear in the Active section above the Vaulted header

#### Scenario: Vaulted cards appear in the Vaulted section
- **WHEN** the Prime Gear tab is displayed
- **THEN** all vaulted gear cards appear in the Vaulted section below the Vaulted header

#### Scenario: Vaulted section is hidden by default
- **WHEN** the application launches and the user navigates to the Prime Gear tab
- **THEN** the Vaulted section header and its cards are not visible

#### Scenario: Show vaulted gear checkbox is in the wishlist-selector panel
- **WHEN** the Prime Gear tab is displayed
- **THEN** the Show vaulted gear checkbox appears in the same panel row as the wishlist selector, not in a separate separator row between Active and Vaulted cards

#### Scenario: Vaulted header is only visible when Show vaulted gear is checked
- **WHEN** Show vaulted gear is unchecked
- **THEN** the Vaulted section header is not visible
- **WHEN** Show vaulted gear is checked
- **THEN** the Vaulted section header labelled "Vaulted" is visible above the vaulted cards
