# ui-iconography Specification

## Purpose

Defines the app's in-UI icons: vector shapes that replace text and emoji glyphs for actions, so action markers look the same on every machine and follow the app's colors and font size.

## Requirements

### Requirement: Icons are drawn as vector shapes
Icons SHALL be drawn as vector shapes that ship with the application. They SHALL NOT depend on installed fonts or on the system emoji font, so an icon looks the same on every machine.

#### Scenario: Icon looks the same without emoji fonts
- **WHEN** the application runs on a machine whose emoji font differs from the developer's
- **THEN** the Recruiting Chat message action in Table mode shows the same icon as on any other machine

### Requirement: Icons follow the surrounding text color
An icon SHALL take the color of the text of the control it sits in, so it follows that control's hover, disabled and palette colors without separate rules. Coloring an icon differently (for example, the destructive hover color) SHALL be done through the control's text color.

#### Scenario: Disabled action greys its icon
- **WHEN** an action with an icon is disabled
- **THEN** its icon is drawn in the same muted color as its disabled label

### Requirement: Icons scale with the font size
Icons SHALL be sized in proportion to the app's font size setting, so they keep the same size relative to the text next to them at Small, Medium, Large and Custom sizes.

#### Scenario: Icons grow with Large font size
- **WHEN** the user switches the font size from Medium to Large
- **THEN** the icons in the selector actions menus and the Table mode actions grow along with the text

### Requirement: Icon-only actions name themselves on hover
An action shown as an icon without a label SHALL show a tooltip naming the action when the pointer rests on it.

#### Scenario: Table mode action shows its name
- **WHEN** the user rests the pointer on the rewards action of a relic row in Table mode
- **THEN** a tooltip names the action
