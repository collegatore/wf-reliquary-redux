## REMOVED Requirements

### Requirement: Version Visible in the Window Title

**Reason**: The version is being moved out of the main window title into the new File → About dialog (see the `about-dialog` capability), to keep the title uncluttered and give the version a conventional, discoverable home.

**Migration**: No user data migration needed. Any code or documentation referencing the window title as `"{ApplicationInfo.Name} {ApplicationInfo.Version}"` should instead read `ApplicationInfo.Name` for the title and expect the version in the About dialog.

## ADDED Requirements

### Requirement: Application Name Visible in the Window Title

The main window title SHALL display the application name only (e.g. `WF Reliquary Redux`), without the version.

#### Scenario: Title shows only the application name

- **WHEN** the main window is shown
- **THEN** its title is exactly the application name, with no version suffix
