## 1. Update Status Bar Label

- [x] 1.1 In `MainWindowViewModel.cs`, change the `LastUpdateText` helper: replace `"Last Update unavailable"` with `"Drop Tables version unavailable"` and `$"Last Update: {lastUpdate}"` with `$"Drop Tables version: {lastUpdate}"`

## 2. Update Validation Error Messages

- [x] 2.1 In `DropTableLocalFileSourceWorkflow.cs`, replace the error message `"File does not contain a Last Update value."` with `"File does not contain a Drop Tables version value."`
- [x] 2.2 In `DropTableSourceRefreshService.cs`, replace the error message `"Downloaded drop-table file does not contain a Last Update value."` with `"Downloaded drop-table file does not contain a Drop Tables version value."`

## 3. Verify

- [x] 3.1 Search the repo for any remaining occurrences of `"Last Update"` in user-visible strings and confirm none were missed
- [x] 3.2 Build the solution and confirm no compile errors
