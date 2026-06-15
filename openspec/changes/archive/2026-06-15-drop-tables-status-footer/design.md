## Context

The Drop Tables tab currently has three status `TextBlock` elements:

- **`StatusText`** (Source acquisition section) — source-file state ("Source data not acquired." / "Source data ready. Drop Tables version: X.").
- **`ImportStatusText`** (Import section) — import state ("Source data not imported." / "Source data imported. Drop Tables version: X.").
- **`OperationStatusText`** (bottom of tab) — live operation feedback, set at the start of each command and updated on completion ("Downloading source from URL…", "Import completed. …", etc.).

Only `OperationStatusText` moves. The other two remain in the tab body where they provide contextual help for the nearby buttons.

The global footer (`MainWindow.axaml`, docked bottom) already exists, showing only the active profile name. `MainWindowShellViewModel` owns both `SelectedTabIndex` and the `DropTables` sub-VM, making it the right place for the "is the Drop Tables tab active?" logic.

## Goals / Non-Goals

**Goals:**
- Move `OperationStatusText` from the Drop Tables tab body to the global footer, visible from all tabs.
- Show a spinner alongside any in-progress operation text in the footer.
- Show condensed import status in the footer when the Drop Tables tab is hidden and no operation is running.
- Rename the `StatusText` label in the Source acquisition section for clarity (UI string only, no behavior change).

**Non-Goals:**
- No change to `StatusText` or `ImportStatusText` placement or behavior.
- No change to commands, application-layer logic, or data model.
- The footer status region is intentionally empty while idle and the Drop Tables tab is active (import status is already visible in the tab body — no need to repeat it in the footer).

## Decisions

### 1. Footer idle state: empty when Drop Tables tab is active, condensed import status when hidden

When the user is on the Drop Tables tab and no operation is running, the tab body already shows `ImportStatusText`. Repeating it in the footer would be redundant. The footer status region is therefore blank in that state.

When the user is on any other tab, there is no visible import status. The footer shows a condensed one-liner: "Source data not imported" or "Drop Tables version: `<last update>`".

*Alternative considered:* Always show condensed import status in footer. Rejected because it duplicates information already visible in the tab body when the tab is active.

### 2. `ShowFooterImportStatus` computed on `MainWindowShellViewModel`

The condition `!IsBusy && SelectedTabIndex != DropTablesTabIndex` spans two different VMs. `MainWindowShellViewModel` already owns `SelectedTabIndex` and holds a reference to `DropTables`, so the bool is computed there and exposed as a bindable property.

`MainWindowShellViewModel` subscribes to `DropTables.PropertyChanged` for `IsBusy` (in addition to the existing `HasImportedData` subscription) so `ShowFooterImportStatus` fires `PropertyChanged` correctly.

### 3. `FooterImportStatusText` on `MainWindowViewModel`

The condensed form ("Source data not imported" / "Drop Tables version: X") is derived from the same data already used to set `ImportStatusText`. Adding a sibling `FooterImportStatusText` property on `MainWindowViewModel` keeps string-building logic with the VM that owns the data.

It is updated in the same `RefreshImportStatusAsync` call that updates `ImportStatusText`.

### 4. Spinner via `IsIndeterminate` ProgressBar

Avalonia's `ProgressBar` with `IsIndeterminate="True"` provides a built-in animated control. It is styled small (fixed height/width) and placed to the left of the operation text. Visibility is bound to `DropTables.IsBusy`.

### 5. StatusText property rename (code only)

The `StatusText` property on `MainWindowViewModel` and its AXAML binding are renamed to `SourceStatusText` to distinguish it from `ImportStatusText` and `OperationStatusText` at a glance. No wording change to the displayed strings.

## Risks / Trade-offs

- **Footer width pressure** — The import success `OperationStatusText` currently appends relic/reward/mission counts, which would overflow the footer. Mitigation: after a successful import, the counts are moved into `ImportStatusText` (which stays in the Drop Tables tab body). `OperationStatusText` for a successful import becomes a brief "Import completed. Drop Tables version: X." line safe for footer display. `ImportStatusText` after a successful import becomes "Source data imported. Drop Tables version: X. Relics: N; relic rewards: N; mission sources: N; mission rewards: N." — the same detail users saw before, now in the tab where there is room for it.
- **`ShowFooterImportStatus` property change subscription** — Shell VM must re-raise `PropertyChanged` for `ShowFooterImportStatus` whenever `DropTables.IsBusy` or `SelectedTabIndex` changes. The existing pattern in `MainWindowShellViewModel` already does this for `ShowDropTablesTabHeader`; the same mechanism is used here.
