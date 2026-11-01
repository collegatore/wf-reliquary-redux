# control-bar-visual-style Specification

## Purpose

Defines the shared visual language of the tab control bars (the Relics control panel, the missions separator panel, and the Prime Gear primary bar and group-operations row), so their buttons, icon-only menu buttons, checkboxes, labels, headings, and non-default markers read consistently and unambiguously.

## Requirements

### Requirement: Pane bars carry a section heading
A control bar that heads a pane of content SHALL begin with a section heading naming that pane: "Relics" on the Relics control panel and "Missions" on the missions separator panel. A section heading SHALL have no trailing colon, SHALL be rendered SemiBold and slightly larger than the bar's control text, and SHALL be set apart from the following controls by a gap. The Prime Gear primary bar SHALL NOT carry a section heading.

#### Scenario: Relics heading is styled as a heading
- **WHEN** the Relics and Missions tab is displayed
- **THEN** the Relics control panel begins with "Relics" (no colon) in SemiBold, larger than the bar's regular text size

#### Scenario: Missions heading in both layouts
- **WHEN** the Relics and Missions tab is displayed in either the stacked or the side-by-side layout
- **THEN** the missions separator panel begins with a "Missions" heading styled the same as the "Relics" heading

#### Scenario: Prime Gear bar has no heading
- **WHEN** the Prime Gear tab is displayed
- **THEN** its primary bar begins with the wishlist selector, with no section heading before it

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

### Requirement: Control-bar checkboxes are chips
Every checkbox on a control bar SHALL be a chip: the checkbox and its label SHALL form a single control inside an outlined pill, so that clicking or hovering the label acts on the checkbox. This SHALL apply to Show exact drop chances. Filter and display options are menu entries, not bar checkboxes.

#### Scenario: Clicking the label toggles the checkbox
- **WHEN** the user clicks the text "Show exact drop chances" on the missions bar
- **THEN** the checkbox toggles, with the same effect as clicking its box

#### Scenario: Each checkbox has its own pill
- **WHEN** the missions separator panel is displayed
- **THEN** Show exact drop chances appears in its own outlined pill

### Requirement: Checked chips use the app's selection color
A checked chip SHALL be tinted with the same color used for a Prime gear card selected for group operations, so that "selected/on" looks the same across the app. An unchecked chip SHALL have no tint.

#### Scenario: Checked chip is tinted
- **WHEN** Show exact drop chances is checked on the missions bar
- **THEN** its chip is tinted with the selection color

#### Scenario: Tint follows the checked state
- **WHEN** the user unchecks a checked chip
- **THEN** the chip's tint is removed immediately

### Requirement: Inactive bar buttons are greyed out
A control-bar button whose action is unavailable SHALL be greyed out as a whole (text, outline, and fill) and SHALL show no hover feedback.

#### Scenario: Inactive button ignores hover
- **WHEN** the pointer moves over an inactive Mark owned button
- **THEN** the button stays greyed out and shows no hover tint or hover outline

### Requirement: Dropdown lists open left-aligned under their field
Every dropdown in the application SHALL open its list just below its field, separated by a small gap, with the list's left edge aligned to the field's left edge. A list wider than its field SHALL extend to the right, never centered on the field. Over-long entry names SHALL truncate with an ellipsis and show the full name in a hover tooltip.

#### Scenario: Wide list starts at the field's left edge
- **WHEN** a wishlist's name is longer than the Wishlist selector field and the user opens the selector
- **THEN** the list opens below the field with a small gap, starting at the field's left edge, and the long name is truncated with an ellipsis

### Requirement: Control-bar buttons are outlined pills
Every button on a control bar SHALL be outlined and lighter than the bar, so it reads as clickable without hovering. Related controls SHALL sit closer together than unrelated ones: on the Relics bar the sort button and its direction button form one close pair, apart from the Filters and Display buttons; on the Prime Gear bar the Filters and Group operations buttons are separate pills.

#### Scenario: Relics bar controls are separate pills
- **WHEN** the Relics and Missions tab is displayed
- **THEN** the Filters button, the sort button with its direction button, and the Display button each have their own outline, with the sort pair closer together than the rest

#### Scenario: Buttons are recognizable at rest
- **WHEN** a control bar is displayed and the pointer is not over any of its buttons
- **THEN** each button is visibly bounded by its own outline

### Requirement: Paired group actions are always joined
The paired actions Select all / Select none and Mark owned / Unmark owned SHALL each always be joined into one outlined control, with a single divider between the two buttons, whatever the Filters menu hides.

#### Scenario: Group-operations pairs are always joined
- **WHEN** group operations is enabled on the Prime Gear tab, with or without Owned hidden
- **THEN** Select all and Select none form one outlined control split by a single divider, Mark owned and Unmark owned form a second, the two controls are separated by a gap, and the selection status is plain text

### Requirement: Control bars group controls by spacing
Control bars SHALL NOT contain free-standing vertical separator lines between controls. Grouping SHALL be conveyed by spacing between outlined controls.

#### Scenario: No separator lines on the Relics bar
- **WHEN** the Relics control bar is displayed
- **THEN** no vertical separator line appears between the "Relics" heading and the controls after it, nor between those controls

### Requirement: Bar menu buttons are icon-only
The Filters, Sort, and Display menu buttons SHALL show only their icon (a funnel for Filters, a double arrow for Sort, an eye for Display), with the same height and outline as the bar's other controls. Pressing one SHALL open its menu just below it, left-aligned with it, in the shared menu look. While the pointer rests on one, a tooltip SHALL appear after a short delay naming the control and describing its current state.

#### Scenario: Filters button is a funnel icon
- **WHEN** the Prime Gear tab is displayed
- **THEN** the Filters control is a pill containing only a funnel icon, as tall as the wishlist selector next to it

#### Scenario: Tooltip names an icon-only button
- **WHEN** nothing is hidden and the user rests the pointer on the Filters button
- **THEN** after a short delay a tooltip names the Filters control and states that nothing is hidden

### Requirement: A non-default state looks like a checked chip
While its state differs from its default, the Filters, Sort, or Display button SHALL take the checked-chip look: the palette's selection wash behind an icon in the palette's selection accent, kept on hover. Otherwise it SHALL look like any other bar button. Each default is defined by the button's own capability, and the look SHALL update as soon as the state changes.

#### Scenario: Marker appears with the change
- **WHEN** the user checks a Filters entry, then unchecks it again
- **THEN** the Filters button takes the selection wash with an accent funnel at once, and returns to the plain look at once

#### Scenario: Marker follows the palette
- **WHEN** the Stone palette is active and a Prime Gear filter is checked
- **THEN** the Filters button shows Stone's selection wash with the funnel in Stone's selection accent
