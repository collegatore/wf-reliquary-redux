## Why

The Drop Tables tab shows operation feedback (e.g. "Downloading source from URL…", "Import completed.") in a `TextBlock` at the bottom of the tab — but this feedback disappears as soon as the user navigates away. Moving operation status to a persistent footer gives users visibility into ongoing operations from any tab. A condensed import status in the footer also provides at-a-glance context when the tab is hidden.

## What Changes

- The `StatusText` property and its AXAML binding in the Drop Tables tab are **renamed** to `SourceStatusText` for clarity, with no behavior or wording change.
- The `ImportStatusText` and `StatusText` blocks **remain in the Drop Tables tab**.
- The `OperationStatusText` block is **removed from the tab body** and moved to the global footer.
- The global footer gains a **vertical separator** after the profile name, followed by a Drop Tables status region that shows:
  - While an operation is in progress (`IsBusy`): a spinning progress indicator + `OperationStatusText`.
  - While idle and the Drop Tables tab is **hidden**: condensed import status — "Source data not imported" or "Drop Tables version: `<last update>`".
  - While idle and the Drop Tables tab is **active**: the status region is empty (import status is already visible in the tab body).

## Capabilities

### New Capabilities

- `drop-tables-footer-status`: Operation feedback and condensed import status displayed in the global window footer.

### Modified Capabilities

*(none — source acquisition and import status requirements are unchanged; only the operation-feedback placement changes)*

## Impact

- **`MainWindow.axaml`** — footer row extended with separator + status region; `OperationStatusText` TextBlock removed from Drop Tables tab body; `StatusText` binding renamed to `SourceStatusText`.
- **`MainWindowViewModel.cs`** — expose `FooterImportStatusText` (condensed import status) for use in the footer binding.
- **`MainWindowShellViewModel.cs`** — expose a computed `ShowFooterImportStatus` bool (`!DropTables.IsBusy && SelectedTabIndex != DropTablesTabIndex`) for the AXAML visibility binding.
