# about-dialog Specification

## Purpose

Gives the application a conventional File → About entry point that surfaces the application name, current version, and a link to the project's README.

## Requirements

### Requirement: About Menu Item

The File menu SHALL contain an **About** item, positioned after Drop Tables, Settings, and Appearance.

#### Scenario: About item is present in the File menu

- **WHEN** the user opens the File menu
- **THEN** an About item is shown after the Drop Tables, Settings, and Appearance items

### Requirement: About Dialog Shows Name and Version

Selecting the File → About menu item SHALL open a modal dialog, owned by the main window, showing the application name and current version.

#### Scenario: About dialog opens from the File menu

- **WHEN** the user selects File → About
- **THEN** a modal dialog opens, blocking interaction with the main window until closed, displaying the application name and the current application version

#### Scenario: Dialog can be closed

- **WHEN** the About dialog is open and the user closes it (e.g. via a Close button or the window's close control)
- **THEN** the dialog closes and interaction with the main window resumes

### Requirement: About Dialog Links to the README

The About dialog SHALL contain a hyperlink to the project's README on its GitHub repository. Activating the hyperlink SHALL open it in the user's default browser.

#### Scenario: README hyperlink opens in the default browser

- **WHEN** the user clicks the README hyperlink in the About dialog
- **THEN** the system opens the repository's README URL in the user's default browser
