# Spec Delta

## MODIFIED Requirements

### Requirement: Profile management actions

The Profiles section SHALL provide three actions: create a new profile, rename the active
profile, and delete the active profile. They SHALL be offered in an actions menu next to the
Profile selector, as defined by the `selector-actions-menu` capability, labeled
**Rename this profile…**, **Delete this profile**, and **New profile**. The Profile selector's
own dropdown SHALL list only profiles.

#### Scenario: All three management actions are present

- **WHEN** the user views the Profiles section in Settings
- **THEN** actions to create a new profile, rename the active profile, and delete the active profile are available

#### Scenario: Actions are in the menu, not the selector

- **WHEN** the user opens the Profile selector, and then presses the Profile actions button
- **THEN** the selector lists only profile names, and the menu shows "Rename this profile…", "Delete this profile", and "New profile"
