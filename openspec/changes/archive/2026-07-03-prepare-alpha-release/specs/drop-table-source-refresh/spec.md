## MODIFIED Requirements

### Requirement: Configured Drop Table Source URL

The system SHALL use a user-editable application-level Warframe drop-table
source URL and SHALL default to `https://www.warframe.com/droptables`
when no user setting is saved. The download operation SHALL follow HTTP
redirects, since the canonical address redirects to DE's current CDN location
for the drop-table HTML.

#### Scenario: Default source URL is used
- **WHEN** the application starts without a configured source URL override
- **THEN** the drop-table refresh workflow uses `https://www.warframe.com/droptables`
  as the source URL

#### Scenario: Default source redirect is followed
- **WHEN** a download is requested against the default source URL and the server
  responds with an HTTP redirect to the current drop-table asset location
- **THEN** the download follows the redirect and stores the drop-table HTML it
  resolves to

#### Scenario: Source URL is edited by user
- **WHEN** the user saves a different absolute source URL in the application UI
- **THEN** the drop-table refresh workflow uses the configured URL for the next
  download operation

#### Scenario: Invalid source URL is entered
- **WHEN** the user attempts to save a source URL that is not absolute
- **THEN** the application rejects the value and keeps the previously configured
  URL
