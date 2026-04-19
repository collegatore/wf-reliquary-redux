## 1. Application Contracts and Use Case

- [x] 1.1 Add application models/options for drop-table source URL, stored-file status, provenance, and refresh result under `src/Application/WF.Reliquary.Application/DropTables/`.
- [x] 1.2 Add application ports for source download, raw file storage, and Last Update metadata extraction under `src/Application/WF.Reliquary.Application/Ports/`.
- [x] 1.3 Implement the application-layer refresh/status orchestration service without referencing infrastructure, presentation, or file-system APIs directly.
- [x] 1.4 Add NUnit unit tests for default URL behavior, status reporting with no file, successful refresh orchestration, failed refresh preservation, and Last Update propagation.

## 2. Infrastructure Implementations

- [x] 2.1 Implement an HTTP drop-table source client in `src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/` using `HttpClient`.
- [x] 2.2 Implement a local raw-file store with deterministic app-data path selection, test-overridable root path, raw HTML storage, and adjacent metadata JSON.
- [x] 2.3 Implement Last Update extraction from stored HTML, including behavior when the marker is missing.
- [x] 2.4 Add NUnit integration tests for file storage/provenance, metadata extraction, and HTTP client behavior using fake handlers or local test doubles rather than live network access.

## 3. Composition Boundary

- [x] 3.1 Create `src/Composition/WF.Reliquary.Composition/WF.Reliquary.Composition.csproj` and add it to `WF.Reliquary.Redux.sln`.
- [x] 3.2 Configure Composition to reference Application and Infrastructure, and configure Presentation to reference Composition without directly referencing Infrastructure.
- [x] 3.3 Add a composition factory that creates the configured drop-table refresh service using application-level user settings and the preparation-phase URL fallback.
- [x] 3.4 Extend project-reference boundary tests to validate Application independence, Presentation avoiding direct Infrastructure dependency, and Composition owning Infrastructure wiring.

## 4. Presentation UI

- [x] 4.1 Replace the placeholder main window content with a status field, operation result text, and a button that starts the drop-table download operation.
- [x] 4.2 Implement a testable presentation view model that loads stored-file status on startup, exposes Last Update/empty/unavailable/busy/error text, and prevents concurrent refreshes.
- [x] 4.3 Wire the Avalonia main window to the composed refresh service and view model.
- [x] 4.4 Add unit or integration tests for UI-facing view-model state transitions without requiring a live network request or launching the full UI.

## 5. Documentation and Scope Checks

- [x] 5.1 Update `README.md` with the default drop-table URL, user-editable app setting, local raw-file storage behavior, and current scope boundary.
- [x] 5.2 Update README validation tests to cover the new download/storage guidance.
- [x] 5.3 Review source changes to confirm no reward-table parser, SQLite schema, normalized import, item lookup, mission recommendation, or acquisition ranking behavior was added.

## 6. Verification

- [x] 6.1 Run `dotnet restore` from the repository root and resolve any restore issues.
- [x] 6.2 Run `dotnet build` from the repository root and resolve any build issues.
- [x] 6.3 Run `dotnet test` from the repository root and resolve any test failures.
- [x] 6.4 Run `openspec validate add-drop-table-download --type change --strict` and resolve any proposal/spec/task issues.
