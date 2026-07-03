## 1. Delete Dead Infrastructure Class

- [x] 1.1 Delete `src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/LocalDropTableSourceSettingsStore.cs`

## 2. Shrink the Workflow Interface and Implementation

- [x] 2.1 Remove `GetSettingsAsync` and `SaveSettingsAsync` from `IDropTableSourceRefreshWorkflow` (`src/Application/WF.Reliquary.Application/DropTables/IDropTableSourceRefreshWorkflow.cs`)
- [x] 2.2 Remove the implementations of `GetSettingsAsync` and `SaveSettingsAsync` from `DropTableSourceRefreshService` (`src/Application/WF.Reliquary.Application/DropTables/DropTableSourceRefreshService.cs`)

## 3. Simplify the Composition Factory

- [x] 3.1 In `ReliquaryServiceFactory.CreateDropTableSourceRefreshService`, remove the `?? new LocalUserSettingsStore()` fallback and replace the guard expression with a direct cast of the supplied `userSettingsStore` to `IDropTableSourceSettingsStore` (`src/Composition/WF.Reliquary.Composition/ReliquaryServiceFactory.cs`)

## 4. Remove Orphaned Tests

- [x] 4.1 Delete the two test methods that exercise `LocalDropTableSourceSettingsStore` directly from `DropTableInfrastructureTests.cs`: `LocalSettingsStore_SavesUserEditableSourceUrl` (line 127) and `LocalSettingsStore_UsesDefaultUrlWhenSettingsDoNotExist` (line 140) (`tests/Integration/WF.Reliquary.IntegrationTests/DropTableInfrastructureTests.cs`)
- [x] 4.2 Delete the `SaveSettingsAsync_PersistsUserEditableSourceUrl` test method (line 138) from `DropTableSourceRefreshServiceTests.cs` (`tests/Unit/WF.Reliquary.UnitTests/DropTableSourceRefreshServiceTests.cs`)
- [x] 4.3 Remove the `GetSettingsAsync` mock setup line from `MainWindowViewModelTests.CreateWorkflow` (line 173) — `workflow.Setup(workflow => workflow.GetSettingsAsync(...)).ReturnsAsync(...)` (`tests/Integration/WF.Reliquary.IntegrationTests/MainWindowViewModelTests.cs`)

## 5. Unify StoredDropTableSource with DropTableSourceContent

- [x] 5.1 Delete `src/Application/WF.Reliquary.Application/DropTables/StoredDropTableSource.cs`
- [x] 5.2 Delete `src/Application/WF.Reliquary.Application/DropTables/DropTableSourceProvenance.cs`
- [x] 5.3 Change `IDropTableSourceStore.LoadAsync` return type from `StoredDropTableSource?` to `DropTableSourceContent?` (`src/Application/WF.Reliquary.Application/Ports/IDropTableSourceStore.cs`)
- [x] 5.4 Update `LocalDropTableSourceStore.LoadAsync` to return a `DropTableSourceContent` instead of `StoredDropTableSource` — map entity fields as `new DropTableSourceContent(new Uri(entity.SourceUrl), entity.Content, DateTimeOffset.Parse(...), entity.SourceLastUpdate)` (`src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/LocalDropTableSourceStore.cs`)
- [x] 5.5 Change `IDropTableParser.ParseAsync` (and `HtmlDropTableParser.Parse` / `ParseAsync`) parameter type from `StoredDropTableSource` to `DropTableSourceContent`; update all field access from `source.Provenance.SourceUrl/DownloadedAt/LastUpdate` to `source.SourceUrl/DownloadedAt/LastUpdate` (`src/Application/WF.Reliquary.Application/Ports/IDropTableParser.cs`, `src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/HtmlDropTableParser.cs`)
- [x] 5.6 Update `DropTableImportService`: change `stored.Provenance.LastUpdate` to `stored.LastUpdate` (`src/Application/WF.Reliquary.Application/DropTables/DropTableImportService.cs`)
- [x] 5.7 Update `DropTableSourceRefreshService.GetStatusAsync`: change `stored.Provenance.DownloadedAt` → `stored.DownloadedAt` and `stored.Provenance.LastUpdate` → `stored.LastUpdate` (`src/Application/WF.Reliquary.Application/DropTables/DropTableSourceRefreshService.cs`)
- [x] 5.8 Update the six test files that construct `StoredDropTableSource` / `DropTableSourceProvenance` — replace `new StoredDropTableSource(html, new DropTableSourceProvenance(url, at, ver))` with `new DropTableSourceContent(url, html, at, ver)` in: `DropTableSourceRefreshServiceTests.cs` (lines 32–34, 108–110), `DropTableImportServiceTests.cs` (lines 134–143), `DropTableImportInfrastructureTests.cs` (lines 240–248), `WishlistFarmingMissionsBenchmarkTests.cs` (lines 81–86), `WishlistFarmingMissionsDistilledTableBenchmarkTests.cs` (lines 234–239), `WishlistFarmingMissionsDedupeTests.cs` (lines 164–167)

## 6. Remove Dead source_file_path DB Column

- [x] 6.1 Remove `SourceFilePath` property from `ImportMetadataEntity` and its mapping from `ReliquaryDbContext` (`src/Infrastructure/WF.Reliquary.Infrastructure/Storage/Entities/SnapshotEntities.cs`, `src/Infrastructure/WF.Reliquary.Infrastructure/Storage/ReliquaryDbContext.cs`)
- [x] 6.2 Remove `SourceFilePath = null` from `SqliteDropTableImportStore.ReplaceAsync` (`src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/SqliteDropTableImportStore.cs`)
- [x] 6.3 Add an EF Core migration that drops the `source_file_path` column from `import_metadata`: run `dotnet ef migrations add RemoveSourceFilePath --project src/Infrastructure/WF.Reliquary.Infrastructure --startup-project src/Presentation/WF.Reliquary.Presentation` from the repo root

## 7. Verify

- [x] 7.1 Run `dotnet build` from the repository root and confirm zero errors
- [x] 7.2 Run `dotnet test` from the repository root and confirm all tests pass
