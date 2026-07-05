# first-start-setup Specification

## Purpose
TBD - created by openspec-sync-specs for change automated-first-start-setup. Update Purpose after archive.

## Requirements

### Requirement: First-Start Setup Gate

The system SHALL offer an automated setup routine (download the drop-table source from the configured URL, then import it) only when no drop-table source has ever been saved. This condition SHALL be evaluated during application startup, after Drop Tables status has loaded, using the same signal as the existing "any stored sources" status (`HasStoredSources`) rather than whether data has been imported. Because sources are only ever added or replaced and never deleted, this gate SHALL become permanently satisfied (and the offer permanently suppressed) once any source has been saved by any means — this routine, a manual "Download from URL", or "Read from file".

#### Scenario: No source exists on startup
- **WHEN** the application starts and no row exists in `source_files`
- **THEN** the application presents the first-start setup consent modal

#### Scenario: A source already exists on startup
- **WHEN** the application starts and at least one row exists in `source_files`, regardless of whether any data has been imported
- **THEN** the application does not present the first-start setup consent modal

#### Scenario: Gate stays satisfied after a prior manual download
- **WHEN** a user previously used "Download from URL" or "Read from file" to save a source, without ever approving the automated routine
- **THEN** the first-start setup consent modal does not appear on subsequent launches

### Requirement: First-Start Setup Consent Modal

When the first-start setup gate is met, the system SHALL present a modal window that explains a file will be downloaded from the web, displays the effective source URL as currently configured (not a hardcoded default), and states that the user may instead download the file themselves using a browser. The modal SHALL offer exactly two actions: Approve and Deny. The automated setup routine SHALL NOT run unless the user activates Approve.

#### Scenario: Modal displays the effective configured URL
- **WHEN** the first-start setup consent modal is shown and the user has previously saved a non-default source URL in Settings
- **THEN** the modal displays that saved URL, not the compiled-in default

#### Scenario: Modal displays the default URL when unconfigured
- **WHEN** the first-start setup consent modal is shown and no source URL override has been saved
- **THEN** the modal displays the default source URL

#### Scenario: User denies the offer
- **WHEN** the user activates Deny on the consent modal
- **THEN** the modal closes, no download or import is attempted, and no preference is persisted

#### Scenario: Denial is not remembered across launches
- **WHEN** the user has denied the offer on a previous launch and no source has been saved since
- **THEN** the consent modal is presented again on the next launch

### Requirement: Automated Download-Then-Import Routine

When the user activates Approve, the system SHALL run, within the same modal window, a download step followed by an import step: the download step invokes the existing drop-table source refresh operation against the configured URL; if and only if the download step succeeds, the import step invokes the existing drop-table import operation against the newly saved source. The modal SHALL display each step's status as it progresses through pending, running, and a terminal succeeded or failed state, and SHALL NOT allow the user to abandon the run via the window's native close control.

#### Scenario: Both steps succeed
- **WHEN** the user approves and the download step succeeds with a parseable Last Update value
- **THEN** the import step runs against the newly saved source and, on success, both steps show a succeeded state

#### Scenario: Download step fails
- **WHEN** the user approves and the download step fails
- **THEN** the import step does not run, the download step shows a failed state, and the failure is also reported through the application's existing error-reporting channel

#### Scenario: Download succeeds but import fails
- **WHEN** the user approves, the download step succeeds, and the subsequent import step fails
- **THEN** the downloaded source remains saved, the import step shows a failed state, the failure is also reported through the application's existing error-reporting channel, and the user must resolve the failure manually later from the Drop Tables tab

#### Scenario: Native close control is inert during the run
- **WHEN** the download or import step is in progress, or either step has failed or succeeded but the user has not yet activated Close
- **THEN** activating the window's native close (X) control does not close the window

### Requirement: First-Start Setup Reload Parity

A successful download step SHALL refresh the Drop Tables tab's own acquisition and import display, exactly as a manual "Download from URL" action does. A successful import step SHALL refresh the same set of dependent views that a manual import already refreshes (at minimum Prime Gear), exactly as a manual "Import" action does. A failed step SHALL NOT trigger the reload associated with it.

#### Scenario: Successful download refreshes the Drop Tables tab's own display
- **WHEN** the download step succeeds
- **THEN** the Drop Tables tab's acquisition and import sections reflect the newly saved source, without requiring the user to navigate away and back

#### Scenario: Successful import refreshes dependent views
- **WHEN** the import step succeeds
- **THEN** Prime Gear (and the other views a manual import already refreshes) reload to reflect the newly imported data

#### Scenario: A failed step does not trigger its reload
- **WHEN** the download step fails, or the download step succeeds but the import step fails
- **THEN** only the reloads associated with the steps that actually succeeded occur

### Requirement: First-Start Setup Completion

Once both the download and import steps have reached a terminal state (succeeded or failed), the system SHALL present a Close action in the modal. The window SHALL only be dismissible through this Close action; the native close (X) control SHALL remain inert even after both steps have settled.

#### Scenario: Close becomes available after both steps settle
- **WHEN** the download and import steps have both reached a terminal state
- **THEN** the modal presents an enabled Close action

#### Scenario: Close action dismisses the modal
- **WHEN** the user activates the Close action
- **THEN** the modal closes and normal application use continues

#### Scenario: Native close control remains inert after completion
- **WHEN** both steps have reached a terminal state and the user has not yet activated Close
- **THEN** activating the window's native close (X) control does not close the window
