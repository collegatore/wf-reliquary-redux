# selector-actions-menu Specification

## Purpose

Defines the actions button that sits next to a selector (the Wishlist selector on the Prime Gear tab and the Profile selector in Settings) and the menu it opens, so that choosing an entry and managing entries are separate controls.

## Requirements

### Requirement: Actions button sits next to its selector
Each selector SHALL be followed directly by an actions button showing a vertical-dots ("more") icon. The button SHALL have the same height and outline as the selector and SHALL sit closer to the selector than to any other control. When the pointer rests on it, a tooltip SHALL name what it manages (e.g. "Wishlist actions").

#### Scenario: Wishlist actions button follows the selector
- **WHEN** the Prime Gear tab is displayed
- **THEN** an actions button with a vertical-dots icon sits right after the Wishlist selector, at the same height

#### Scenario: Actions button names itself on hover
- **WHEN** the user rests the pointer on the Profile actions button in Settings
- **THEN** a tooltip reads "Profile actions"

### Requirement: Actions menu lists rename, delete and new with icons
Pressing the actions button SHALL open a menu below it containing, in order: a rename action, a delete action, and a create action. Each SHALL show an icon before its label (pencil, trash can, plus), and no label SHALL start with a text symbol such as "+". Choosing an action SHALL close the menu and start that action.

#### Scenario: Menu shows three actions with icons
- **WHEN** the user presses the Wishlist actions button
- **THEN** a menu opens showing "Rename this list…", "Delete this list" and "New list", each with its icon

### Requirement: Selector lists only choices
A selector's dropdown SHALL list only its entries (wishlists or profiles). It SHALL NOT contain actions. Choosing an entry with the pointer or the keyboard SHALL make it active.

#### Scenario: Selector contains no actions
- **WHEN** the user opens the Wishlist selector
- **THEN** only wishlist names are listed

#### Scenario: Keyboard selects an entry
- **WHEN** the Wishlist selector has keyboard focus and the user opens it, moves down with the arrow key and presses Enter
- **THEN** the wishlist moved to becomes active

### Requirement: Unavailable actions are greyed out
An action that is unavailable SHALL be shown disabled in the menu, with both its icon and its label greyed out, and SHALL NOT respond to clicks.

#### Scenario: Delete is greyed out with one entry
- **WHEN** only one profile exists and the user opens the Profile actions menu
- **THEN** the delete action's icon and label are greyed out and clicking it does nothing

### Requirement: Delete is marked as destructive on hover
While the pointer is over an enabled delete action, its label and icon SHALL turn a destructive color. The destructive color SHALL be the same in every palette.

#### Scenario: Hovering delete shows the destructive color
- **WHEN** two wishlists exist and the user points at "Delete this list" in the Wishlist actions menu
- **THEN** the action's label and trash icon turn the destructive color
