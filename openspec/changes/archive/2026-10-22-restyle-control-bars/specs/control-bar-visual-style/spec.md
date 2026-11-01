# Spec Delta

## Purpose

Defines the shared visual language of the tab control bars (the Relics control panel and its Filters and Sorting rows, the missions separator panel, and the Prime Gear primary bar, filter row, and group-operations row), so their buttons, checkboxes, labels, and headings read consistently and unambiguously.

## ADDED Requirements

### Requirement: Control-bar buttons are outlined chips
Every button on a control bar SHALL be outlined and lighter than the bar, so it reads as clickable without hovering. Filters and Sorting (Relics), Filters and Group operations (Prime Gear), and the sort-direction button SHALL be separate pills, with related ones closer together than to other controls. The paired actions Select all / Select none and Mark owned / Unmark owned SHALL each be joined into one outlined control, with a single divider between the two buttons.

#### Scenario: Relics panel buttons are separate pills
- **WHEN** the Relics and Missions tab is displayed
- **THEN** the Filters and Sorting buttons each have their own outline, with a small gap between them

#### Scenario: Group-operations pairs are joined
- **WHEN** group operations is enabled on the Prime Gear tab
- **THEN** Select all and Select none form one outlined control split by a single divider, Mark owned and Unmark owned form a second, the two controls are separated by a gap, and the selection count is plain text

#### Scenario: Buttons are recognizable at rest
- **WHEN** a control bar is displayed and the pointer is not over any of its buttons
- **THEN** each button is visibly bounded by its own outline

### Requirement: Control bars use no free-standing separators
Control bars and their panel rows SHALL NOT contain free-standing vertical separator lines between controls. Grouping SHALL be conveyed by spacing between outlined controls.

#### Scenario: No separator lines in the Prime Gear filter row
- **WHEN** the Prime Gear filter panel is open
- **THEN** no vertical separator line appears between its checkboxes

#### Scenario: No separator after a heading
- **WHEN** the Relics control panel is displayed
- **THEN** no vertical separator line appears between the "Relics" heading and the Filters and Sorting buttons

### Requirement: Pane bars carry a section heading
A control bar that heads a pane of content SHALL begin with a section heading naming that pane: "Relics" on the Relics control panel and "Missions" on the missions separator panel. A section heading SHALL have no trailing colon, SHALL be rendered SemiBold and slightly larger than the bar's control text, and SHALL be set apart from the following controls by a gap. The Prime Gear primary bar SHALL NOT carry a section heading.

#### Scenario: Relics heading is styled as a heading
- **WHEN** the Relics and Missions tab is displayed
- **THEN** the Relics control panel begins with "Relics" (no colon) in SemiBold, larger than the Filters and Sorting text

#### Scenario: Missions heading in both layouts
- **WHEN** the Relics and Missions tab is displayed in either the stacked or the side-by-side layout
- **THEN** the missions separator panel begins with a "Missions" heading styled the same as the "Relics" heading

#### Scenario: Prime Gear bar has no heading
- **WHEN** the Prime Gear tab is displayed
- **THEN** its primary bar begins with the wishlist selector, with no section heading before it

### Requirement: Wishlist selector carries its label inside the control
The wishlist selector on the Prime Gear primary bar SHALL show a muted "Wishlist:" caption inside its dropdown button, before the active wishlist's name, which SHALL be shown in regular text. No separate "Wishlist" label SHALL appear outside the selector. Over-long names SHALL keep truncating with an ellipsis and a hover tooltip.

#### Scenario: Selector shows caption and name in one button
- **WHEN** the Prime Gear tab is displayed and the active wishlist is "Default"
- **THEN** a single dropdown button reads "Wishlist:" in muted text followed by "Default" in regular text, and no "Wishlist" label appears outside it

#### Scenario: Caption does not open anything separately
- **WHEN** the user clicks the "Wishlist:" caption inside the selector
- **THEN** the wishlist dropdown opens, exactly as when clicking the wishlist name

### Requirement: Control-bar dropdowns share the wishlist selector's field look
Dropdowns on a control bar (the relic sort-mode selector) SHALL have the same field look as the wishlist selector: the same background and outline at rest, and the same hover tint and hover outline while the pointer is over them.

#### Scenario: Sort-mode dropdown highlights like the wishlist selector
- **WHEN** the sorting panel is open and the pointer moves over the sort-mode dropdown
- **THEN** the dropdown shows the same hover tint and outline as the wishlist selector does on hover

### Requirement: Control-bar checkboxes are chips
Every checkbox on a control bar or panel row SHALL be a chip: the checkbox and its label SHALL form a single control inside an outlined pill, so that clicking or hovering the label acts on the checkbox. This SHALL apply to Show vaulted, Show all rewards, Show exact drop chances, Show vaulted gear, Show owned gear, and Wishlisted only.

#### Scenario: Clicking the label toggles the checkbox
- **WHEN** the user clicks the text "Show owned gear" in the Prime Gear filter panel
- **THEN** the Show owned gear checkbox toggles, with the same effect as clicking its box

#### Scenario: Each checkbox has its own pill
- **WHEN** the Prime Gear filter panel is open
- **THEN** Show vaulted gear, Show owned gear, and Wishlisted only each appear in their own outlined pill

### Requirement: Checked chips use the app's selection color
A checked chip SHALL be tinted with the same color used for a Prime gear card selected for group operations, so that "selected/on" looks the same across the app. An unchecked chip SHALL have no tint.

#### Scenario: Checked chip is tinted
- **WHEN** Show vaulted is checked in the Relics filter panel
- **THEN** its chip is tinted with the selection color, while the unchecked Show all rewards chip is not

#### Scenario: Tint follows the checked state
- **WHEN** the user unchecks a checked chip
- **THEN** the chip's tint is removed immediately
