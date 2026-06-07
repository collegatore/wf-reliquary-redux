## MODIFIED Requirements

### Requirement: Drop Table Source URL UI Configuration

The Avalonia UI SHALL show the configured drop-table source URL and SHALL allow the user to edit and save that URL as part of the unified application-level user settings via the Settings tab. The Settings tab's Save button persists the URL along with all other settings. There is no separate Save URL button on the Drop Tables tab.

#### Scenario: User views configured URL

- **WHEN** the user opens the Settings tab
- **THEN** the source URL field displays the saved application setting or the default source URL when no setting exists

#### Scenario: User saves configured URL

- **WHEN** the user enters a valid absolute URL in the Settings tab and clicks Save
- **THEN** the application persists the URL for future refresh operations

#### Scenario: Invalid URL is rejected on save

- **WHEN** the user enters a value that is not an absolute URL in the Settings tab and clicks Save
- **THEN** the application rejects the value, keeps the previously configured URL, and notifies the user of the validation error

## REMOVED Requirements

### Requirement: Drop Tables tab layout

**Reason**: The "Drop-table source URL" section moves to the Settings tab. The Drop Tables tab now contains only the "File download" and "File import" sections; the three-section layout requirement no longer applies.

**Migration**: Remove the "Drop-table source URL" section from the Drop Tables tab. The field now lives in the Settings tab under the `settings-tab` capability.

### Requirement: Drop Table Source URL UI Configuration (old)

**Reason**: Superseded by the MODIFIED requirement above. The standalone Save URL button and its enabled/disabled state are removed; URL editing is now part of the Settings tab Save flow.

**Migration**: Remove the Save URL button and its `CanSaveUrl` / `SaveSourceUrlCommand` bindings from the Drop Tables tab view and view-model. URL state is owned by `SettingsViewModel`.
