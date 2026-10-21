# Spec Delta

## MODIFIED Requirements

### Requirement: Sorting button toggles a collapsible sorting panel
The Relics control panel SHALL contain a **Sorting** button, styled as an outlined button chip grouped with the Filters button, as defined by the `control-bar-visual-style` capability. Pressing it SHALL toggle a collapsible sorting panel rendered as its own row directly below the Relics control panel, using the same background tint and corner styling as the control panel. The sorting panel SHALL be closed by default when the application starts. Closing the panel SHALL NOT reset the active sort mode or direction. The sorting panel SHALL be available in all three relic view modes (Verbatim, Compact, Table). The Sorting and Filters panels MAY be open at the same time; each toggles independently and they stack as separate rows.

#### Scenario: Sorting button opens the panel
- **WHEN** the user presses the Sorting button while the sorting panel is closed
- **THEN** the sorting panel appears below the Relics control panel, showing the sort-mode dropdown and the direction button

#### Scenario: Sorting button closes the panel without resetting the sort
- **WHEN** the user selects Wishlist score sorting and then presses the Sorting button to close the panel
- **THEN** the panel hides and the relics remain sorted by Wishlist score

#### Scenario: Panel is closed on startup
- **WHEN** the application starts
- **THEN** the sorting panel is not visible until the Sorting button is pressed

#### Scenario: Panel is available in every view mode
- **WHEN** the user switches between Verbatim, Compact, and Table view modes
- **THEN** the Sorting button and (when open) the sorting panel remain visible and functional
