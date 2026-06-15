## 1. DB Schema — source_files Table

- [x] 1.1 Add `SourceFileEntity` EF Core entity class: fields `Id`, `Content` (string), `SourceUrl`, `DownloadedAt`, `SourceLastUpdate`
- [x] 1.2 Register `SourceFileEntity` as `DbSet<SourceFileEntity>` in `ReliquaryDbContext` with table name `source_files`
- [x] 1.3 Configure `source_files` as a user-data table (exclude from the snapshot clear set in `SqliteDropTableImportStore`)
- [x] 1.4 Add EF Core migration for the `source_files` table

## 2. LocalDropTableSourceStore — DB Wiring

- [x] 2.1 Inject `ReliquaryDbContext` into `LocalDropTableSourceStore` constructor; update `ReliquaryServiceFactory` composition accordingly
- [x] 2.2 Replace HTML file write and JSON sidecar write (on save) with an `INSERT` into `source_files` (content + provenance)
- [x] 2.3 Replace HTML file read and JSON sidecar read (on load) with a projected query against `source_files ORDER BY id DESC LIMIT 1` — return content and provenance, avoiding loading the full `content` column when only provenance metadata is needed (use separate query or projection for status vs. load)
- [x] 2.4 Confirm `IDropTableSourceStore` contract still compiles cleanly — `LoadAsync` already returns content + provenance; no interface signature changes needed

## 3. Import Store Compatibility

- [x] 3.1 Verify `SqliteDropTableImportStore` receives HTML content via `IDropTableSourceStore.LoadAsync` (no direct file path reference) — no change needed if already the case
- [x] 3.2 Set `SourceFilePath` to `null` in new `import_metadata` rows (field is deprecated; no file path exists)

## 4. Cleanup

- [x] 4.1 Remove all HTML file and JSON sidecar read/write code from `LocalDropTableSourceStore`
- [x] 4.2 Remove filename-related constants from `DropTableSourceDefaults` that are no longer needed

## 5. Import from File — Application & Infrastructure

- [x] 5.1 Define `IDropTableLocalFileSourceWorkflow` port in Application: `RegisterFromFileAsync(string filePath, CancellationToken)` — reads file, extracts Last Update via `IDropTableMetadataReader`, inserts into `source_files`, returns success/failure result
- [x] 5.2 Implement the workflow in Infrastructure (or Application service): reads file content, delegates Last Update extraction and `source_files` insert to existing infrastructure
- [x] 5.3 Register the workflow in `ReliquaryServiceFactory`

## 6. Import from File — Presentation

- [x] 6.1 Add `ReadFromFileCommand` to the Drop Tables view model; wire it to `IDropTableLocalFileSourceWorkflow.RegisterFromFileAsync`
- [x] 6.2 In the command handler: open `StorageProvider.OpenFilePickerAsync()` filtered to `.html` files, pass the selected path to the workflow, display the result in the download status area
- [x] 6.3 Add "Read from file" button to the Drop Tables tab AXAML alongside "Download from URL"
- [x] 6.4 Rename the existing "Download drop tables" button label to "Download from URL" in AXAML

## 7. Export — Application & Infrastructure

- [x] 7.1 Define `IDropTableSourceExporter` port in Application: `ExportAllAsync(string destinationFolderPath, CancellationToken)` returning a result with file count or error message
- [x] 7.2 Implement `LocalDropTableSourceExporter` in Infrastructure: query all `source_files` rows (content + provenance), write each to `{folder}\warframe-drop-tables-{lastUpdate}.html` (fallback to downloaded-at timestamp when `source_last_update` is null)
- [x] 7.3 Register `IDropTableSourceExporter` → `LocalDropTableSourceExporter` in `ReliquaryServiceFactory`

## 8. Export — Presentation

- [x] 8.1 Add `ExportSourcesCommand` and `HasStoredSources` bool to the Drop Tables view model; wire `HasStoredSources` from the `source_files` row count on load and after each successful acquisition (download or file import)
- [x] 8.2 Add Export button to the Drop Tables tab AXAML; bind `IsEnabled` to `HasStoredSources`
- [x] 8.3 In the command handler: open `StorageProvider.OpenFolderPickerAsync()`, pass the selected path to `IDropTableSourceExporter.ExportAllAsync`, display the result message

## 9. Testing

- [ ] 9.1 Run `dotnet test` — existing tests should pass (provenance round-trip, import orchestration)
- [ ] 9.2 Click "Download from URL" — verify "Downloaded file version" updates and no HTML file is created on disk
- [ ] 9.3 Click "Read from file", select a local drop-table HTML — verify it appears in `source_files` and the downloaded version status updates
- [ ] 9.4 Click the Import button — verify parse/import succeeds and "Imported file version" updates
- [ ] 9.5 Click Export with stored sources present — verify one date-stamped HTML file per stored version appears in the selected folder
- [ ] 9.6 Verify Export button is disabled when no sources are stored
