## Context

The drop table source HTML file contains a metadata field that the app has historically called "Last Update." This value is extracted at import/download time and stored in the `source_files` table. It is surfaced to the user in the status bar and in validation error messages. The value is more accurately described as a version identifier for the drop table dataset, not a timestamp of when the app last refreshed.

## Goals / Non-Goals

**Goals:**
- Replace every user-visible string "Last Update" with "Drop Tables version" across status messages and error messages

**Non-Goals:**
- Renaming the `lastUpdate` C# property, database column, or any internal identifier — this is a display-only change
- Changing the value itself or how it is parsed/stored

## Decisions

**String-only change in the presentation and application layers**

All four occurrences are string literals in helper methods or inline error messages. No data model, database schema, or API surface changes. Each string is updated in place.

The private helper `LastUpdateText` in `MainWindowViewModel` is the single point for status-bar formatting; updating it covers both the "available" and "unavailable" cases and all callers automatically.

The two error messages in the application layer (`DropTableLocalFileSourceWorkflow` and `DropTableSourceRefreshService`) each contain an independent string literal and are updated individually.

## Risks / Trade-offs

- [Risk] Tests or snapshots that assert exact error message text will fail → search for test assertions on "Last Update value" before closing the change
