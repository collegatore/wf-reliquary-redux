## Context

WF Reliquary Redux currently has a runnable Avalonia shell, an application
project with placeholder metadata and an application-owned storage health
contract, and an infrastructure project with a SQLite placeholder. The
preparation phase preserved and analyzed the official Warframe drop-table page
from:

`https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html`

The baseline explicitly leaves runtime source refresh undefined. This change
introduces the first runtime ingestion operation: download the configured raw
HTML source, store it locally, and surface the stored file's Last Update status
in the UI. It does not parse reward tables or persist normalized rows.

## Goals / Non-Goals

**Goals:**

- Define a default drop-table source URL matching the preparation-phase URL.
- Allow the source URL to be edited by the user inside the app without
  rebuilding.
- Download the raw HTML source on demand.
- Store the downloaded HTML file and lightweight metadata locally.
- Extract the source page's Last Update value from the stored file when present.
- Add an Avalonia button that starts the download operation.
- Add a UI status field that shows Last Update from the stored file if it
  exists, plus empty, busy, success, and error states.
- Preserve application/infrastructure/presentation separation.
- Cover orchestration, metadata extraction, storage, and UI-facing state with
  NUnit tests.

**Non-Goals:**

- Do not implement drop-table reward parsing.
- Do not create SQLite schema, migrations, or normalized import rows.
- Do not implement item lookup, mission recommendation, ranking, or wishlist
  behavior.
- Do not add background refresh scheduling.
- Do not require credentials or authenticated network access.

## Decisions

### Decision: Application owns the refresh use case and ports

Add application-layer models and service contracts for:

- `DropTableSourceSettings`: user-editable source URL.
- `DropTableSourceOptions`: default source URL and storage file name.
- `DropTableSourceStatus`: whether a local file exists, source URL, stored file
  path, downloaded timestamp, and provenance Last Update text.
- `DropTableRefreshResult`: operation result, updated status, and error message
  if the refresh fails.
- `IDropTableSourceClient`: downloads source content from a URL.
- `IDropTableSourceStore`: stores and loads the raw file and provenance
  metadata.
- `IDropTableMetadataReader`: extracts Last Update text from raw HTML.
- `IDropTableSourceSettingsStore`: stores and loads user-editable app-level
  source settings.
- `DropTableSourceRefreshService`: orchestrates settings, status loading, and
  refresh.

Rationale: the application layer defines the user-facing workflow and remains
independent from HTTP, file-system, and Avalonia details.

Alternative considered: place download logic directly in the Avalonia view
model. This was rejected because it mixes UI and infrastructure concerns.

### Decision: Infrastructure implements HTTP, file storage, and metadata extraction

Infrastructure should provide:

- an HTTP client adapter based on `HttpClient`;
- a local file store rooted in an app-data location, overridable for tests;
- metadata extraction that reads the downloaded HTML and finds the source
  page's Last Update marker.

The raw HTML file should be named predictably, such as `warframe-drop-tables.html`.
Metadata can be stored in a small adjacent JSON file, such as
`warframe-drop-tables.metadata.json`, to preserve source URL, download time, and
Last Update without requiring a database. The refresh service should parse Last
Update before saving; if parsing fails, the operation fails and the existing
stored file is preserved.

Rationale: raw-file refresh is infrastructure work, and JSON metadata keeps this
feature lightweight while SQLite schema remains out of scope.

Alternative considered: store the downloaded file as an embedded resource or in
the repository. This was rejected because the operation is runtime refresh, not
design-artifact preservation.

### Decision: Add a composition project instead of direct presentation-to-infrastructure reference

Add `src/Composition/WF.Reliquary.Composition` to wire application services to
infrastructure implementations. Presentation may reference Application and
Composition. Composition may reference Application and Infrastructure.

Rationale: the baseline allows Presentation to reference Application and keeps
Application independent. A composition project lets the executable obtain real
implementations without Presentation directly referencing Infrastructure.

Alternative considered: let Presentation reference Infrastructure directly as
the desktop composition root. This was rejected because the current baseline
explicitly says Presentation should not reference Infrastructure directly unless
a future specification changes the composition model.

### Decision: Use a simple Avalonia view model with async command state and URL editing

Replace the placeholder-only main window content with:

- a status field that displays `Last Update: <value>` when a stored file exists
  and the value is present;
- an empty state such as `No drop-table file downloaded yet`;
- a textbox showing the configured source URL;
- a save button that persists a valid absolute source URL to app-level user
  settings;
- a button to download/update the drop-table file;
- busy state while the operation is running;
- success/error status text after completion.

Rationale: this is a small first UI workflow. A full MVVM framework is not yet
needed, but the view model should still be testable without launching Avalonia.

Alternative considered: keep the operation command-line only. This was rejected
because the requested feature explicitly includes UI.

### Decision: Configuration uses user-editable application settings with a code default fallback

Use a local application settings JSON file in the user app-data area to store
the configured source URL. The UI owns editing and saving that setting. When the
settings file is absent or invalid, the application falls back to the
preparation-phase URL default.

Rationale: the URL is an application-level user preference, so it belongs in
user-editable app settings rather than an environment variable. A code fallback
guarantees the app works without setup.

Alternative considered: hard-code the URL only. This was rejected because the
request calls for a configured URL.

Alternative considered: read an environment variable. This was rejected because
the source URL should be editable by the user in the app, not hidden in process
environment setup.

## Risks / Trade-offs

- Network failures or source downtime -> return a failed refresh result and
  keep the previous stored file/status visible.
- Source markup changes around Last Update -> refresh fails without replacing
  the previous stored file, making source-format drift visible.
- App-data path varies by platform -> centralize path selection in
  infrastructure and keep it overridable in tests.
- Composition project adds one more project -> keeps layer boundaries explicit
  and avoids direct Presentation-to-Infrastructure coupling.
- UI async state can become flaky if not testable -> keep view-model state
  transitions in plain C# and cover them with unit tests.

## Migration Plan

1. Add application contracts, models, and refresh service.
2. Add infrastructure HTTP, file-store, and metadata implementations.
3. Add composition project and wire it into the solution.
4. Update Presentation to use the composed refresh service.
5. Add tests across application, infrastructure, integration, and UI-facing view
   model behavior.
6. Update README with app-level URL settings and local storage guidance.

Existing users have no migration burden because no runtime data file exists yet.

## Open Questions

- The exact app-data folder name can be finalized during implementation, but it
  should be deterministic and documented.
- If the official source changes the visible Last Update label, future parsing
  work may need a more robust HTML parser. This feature can start with focused
  extraction covered by tests.
