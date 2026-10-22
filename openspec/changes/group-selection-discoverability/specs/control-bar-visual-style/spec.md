# Spec Delta

## MODIFIED Requirements

### Requirement: Control-bar buttons are outlined chips
Every button on a control bar SHALL be outlined and lighter than the bar, so it reads as clickable without hovering. Filters and Sorting (Relics), Filters and Group operations (Prime Gear), and the sort-direction button SHALL be separate pills, with related ones closer together than to other controls. The paired actions Select all / Select none and Mark owned / Unmark owned SHALL each be joined into one outlined control, with a single divider between the two buttons. While Unmark owned is hidden (see `prime-gear-group-operations`), Mark owned SHALL stand alone as a separate pill.

#### Scenario: Relics panel buttons are separate pills
- **WHEN** the Relics and Missions tab is displayed
- **THEN** the Filters and Sorting buttons each have their own outline, with a small gap between them

#### Scenario: Group-operations pairs are joined
- **WHEN** group operations is enabled on the Prime Gear tab with Show owned gear checked
- **THEN** Select all and Select none form one outlined control split by a single divider, Mark owned and Unmark owned form a second, the two controls are separated by a gap, and the selection status is plain text

#### Scenario: Mark owned stands alone when Unmark owned is hidden
- **WHEN** group operations is enabled with Show owned gear unchecked
- **THEN** Mark owned is a separate pill with its own complete outline and rounded corners on both sides

#### Scenario: Buttons are recognizable at rest
- **WHEN** a control bar is displayed and the pointer is not over any of its buttons
- **THEN** each button is visibly bounded by its own outline

## ADDED Requirements

### Requirement: Inactive bar buttons are greyed out
A control-bar button whose action is unavailable SHALL be greyed out as a whole (text, outline, and fill) and SHALL show no hover feedback.

#### Scenario: Inactive button ignores hover
- **WHEN** the pointer moves over an inactive Mark owned button
- **THEN** the button stays greyed out and shows no hover tint or hover outline
