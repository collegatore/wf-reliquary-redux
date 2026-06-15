## Why

The status bar and error messages refer to the drop table source metadata field as "Last Update," but the value actually represents the version identifier of the drop table data file (e.g., a date-stamped version string). Renaming it to "Drop Tables version" makes the label more accurate and meaningful to users.

## What Changes

- The status bar text "Last Update: {value}" becomes "Drop Tables version: {value}"
- The fallback text "Last Update unavailable" becomes "Drop Tables version unavailable"
- Error messages in workflow validation that reference "Last Update value" are updated to match

## Capabilities

### New Capabilities
<!-- None — this is a pure label/copy change -->

### Modified Capabilities
- `drop-table-source-files`: The display label for the `lastUpdate` metadata field is changing from "Last Update" to "Drop Tables version"

## Impact

- `src/Presentation/WF.Reliquary.Presentation/MainWindowViewModel.cs` — `LastUpdateText` helper and its callers
- `src/Application/WF.Reliquary.Application/DropTables/DropTableLocalFileSourceWorkflow.cs` — validation error message
- `src/Application/WF.Reliquary.Application/DropTables/DropTableSourceRefreshService.cs` — validation error message
