## Why

The project has researched the official Warframe drop-table source, but the
application cannot yet refresh or preserve that source at runtime. Adding a
download-and-store workflow creates the first practical data-ingestion step
while keeping parsing and database import for later changes.

## What Changes

- Add a configurable drop-table source URL with a default matching the
  preparation-phase URL:
  `https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html`.
- Add application-layer behavior to request a download, store the raw HTML file,
  and report file metadata including the source page's Last Update value when
  available.
- Add infrastructure behavior for HTTP download, local file storage, metadata
  extraction, and configuration defaults.
- Update the Avalonia shell with a button that starts the download operation.
- Update the UI with a status field that shows the stored file's Last Update
  value if a file exists, plus useful empty/loading/error states.
- Add NUnit coverage for URL defaults, application orchestration, metadata
  extraction, storage behavior, and UI-facing status behavior.
- Add README guidance for where the downloaded file is stored and how to
  configure the source URL.

## Capabilities

### New Capabilities

- `drop-table-source-refresh`: Runtime workflow for downloading the configured
  Warframe drop-table HTML source, storing the raw file locally, and surfacing
  stored-file status in the UI.

### Modified Capabilities

- None.

## Impact

- Touches application-layer ports/services for source refresh orchestration and
  status reporting.
- Touches infrastructure for HTTP retrieval, local app-data/file storage,
  metadata extraction, and default configuration.
- Touches presentation to add a download button and status field to the current
  Avalonia shell.
- Extends unit and integration tests.
- Does not implement drop-table parsing, SQLite schema creation, normalized
  import, item lookup, mission recommendation, or acquisition ranking.
