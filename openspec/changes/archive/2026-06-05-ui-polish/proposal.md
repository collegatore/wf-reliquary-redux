## Why

The Drop Tables, Prime Gear, and Wishlist Relics tabs have accumulated small UX rough edges — redundant app title, unlabelled sections, misleadingly-named status text, and checkboxes placed where they don't belong. Addressing these now keeps the UI clean while the feature set is still small.

## What Changes

- **Drop Tables tab — remove app title**: the `TextBlock` binding `DropTables.Title` is removed; the tab header already identifies the context
- **Drop Tables tab — three labelled sections**: the tab content is restructured into "Drop-table source URL", "File download", and "File import" sections, each preceded by a header and a separator line
- **Drop Tables tab — Save URL disabled when unchanged**: the Save URL button is disabled when `SourceUrlText` equals the last saved URL; requires a new `CanSaveUrl` property on the ViewModel
- **Drop Tables tab — rename download status label**: "Last update" → "Downloaded file version"
- **Drop Tables tab — import status label**: new read-only status line shows "No drop-table file imported yet" or "Imported file version: `<value>`" drawn from `import_metadata`; requires a new `ImportStatusText` property on the ViewModel backed by a new `IImportMetadataReader` port
- **Prime Gear tab — move Show vaulted checkbox**: the `ShowVaulted` checkbox moves from the standalone separator panel into the existing wishlist-selector panel (same `Border` row); the standalone separator `Border` is removed
- **Prime Gear tab — Vaulted section header**: the vaulted `ItemsControl` gets a labelled separator header ("Vaulted") that is only visible when `ShowVaulted` is true
- **Wishlist Relics tab — Show all rewards checkbox**: adds a `ShowAllRewards` bool property (default `false`) to `WishlistRelicsViewModel`; when `false`, each relic card shows only wishlisted rewards; when `true`, all rewards are shown; the checkbox lives in the existing control panel `Border`

## Capabilities

### New Capabilities

<!-- None — all changes modify existing capabilities -->

### Modified Capabilities

- `drop-table-source-refresh`: URL section Save button disabled when unchanged; download status label renamed
- `drop-table-parsing-import`: new import status line displayed in the File import section
- `prime-gear-parts-view`: Show vaulted checkbox relocated; vaulted section gains a visible "Vaulted" label header
- `wishlist-relics-view`: new Show all rewards filter; relic cards show only wishlisted rewards by default

## Impact

- **`MainWindow.axaml`**: significant restructuring of the Drop Tables tab layout; minor changes to Prime Gear and Wishlist Relics tab panels
- **`MainWindowViewModel.cs`**: new `CanSaveUrl` computed property; new `ImportStatusText` property loaded from DB on init and after import
- **`WishlistRelicsViewModel.cs`**: new `ShowAllRewards` bool property; `RebuildCards` passes the flag to card construction
- **`WishlistRelicCardViewModel.cs`**: new filtered rewards view based on `ShowAllRewards`
- **New Application port**: `IImportMetadataReader` with a single `GetImportStatusAsync` method
- **New Infrastructure implementation**: `SqliteImportMetadataReader` reading from `import_metadata` via EF
- **`ReliquaryServiceFactory`**: wires the new reader into `MainWindowViewModel`
