## 1. Import Status Port and Infrastructure

- [x] 1.1 Add `IImportMetadataReader` port to Application (`Ports/IImportMetadataReader.cs`) with a single `Task<string?> GetImportedVersionAsync(CancellationToken ct = default)` method returning `source_last_update` or `null`
- [x] 1.2 Create `SqliteImportMetadataReader` in Infrastructure (`DropTables/SqliteImportMetadataReader.cs`) using EF `ReliquaryDbContext` to query `ImportMetadata` for id=1
- [x] 1.3 Wire `SqliteImportMetadataReader` into `ReliquaryServiceFactory.CreateMainWindowViewModel()` (or create a dedicated factory method) and inject into `MainWindowViewModel`

## 2. MainWindowViewModel — CanSaveUrl and ImportStatusText

- [x] 2.1 Add private `savedSourceUrl` field to `MainWindowViewModel`; populate it in `LoadStatusAsync` from the settings call
- [x] 2.2 Add `bool CanSaveUrl` property (returns `!string.Equals(SourceUrlText, savedSourceUrl, StringComparison.Ordinal)`); raise `PropertyChanged` for `CanSaveUrl` whenever `SourceUrlText` or `savedSourceUrl` changes
- [x] 2.3 Accept `IImportMetadataReader` in `MainWindowViewModel` constructor; add `string ImportStatusText` property
- [x] 2.4 Load import status in `LoadStatusAsync`; refresh `ImportStatusText` after a successful import in `ImportAsync`
- [x] 2.5 `ImportStatusText` formats as `"No drop-table file imported yet"` when reader returns `null`, or `"Imported file version: <value>"` otherwise

## 3. Drop Tables tab AXAML restructure

- [x] 3.1 Remove the `TextBlock` binding `DropTables.Title` from the Drop Tables tab
- [x] 3.2 Replace the flat `Grid` with a `StackPanel` containing three section blocks; each block: section-header `TextBlock` + `Separator` + content
- [x] 3.3 **Section 1 — Drop-table source URL**: header "Drop-table source URL", URL `TextBox`, Save URL `Button` with `IsEnabled="{Binding DropTables.CanSaveUrl}"`
- [x] 3.4 **Section 2 — File download**: header "File download", status `TextBlock` bound to `DropTables.StatusText` (label updated to "Downloaded file version" in ViewModel), Download button
- [x] 3.5 **Section 3 — File import**: header "File import", status `TextBlock` bound to `DropTables.ImportStatusText`, Import button
- [x] 3.6 Move `OperationStatusText` `TextBlock` to the bottom of the outer `StackPanel` (below all three sections)

## 4. Prime Gear tab AXAML changes

- [x] 4.1 Move the `ShowVaulted` `CheckBox` + label into the wishlist-selector `Border > StackPanel` (append after the wishlist `Button`)
- [x] 4.2 Remove the standalone separator `Border` (the one currently containing the Show vaulted checkbox between Active and Vaulted cards)
- [x] 4.3 Add a new `Border` labelled "Vaulted" (same visual style as the wishlist panel) between the Active `ItemsControl` and the Vaulted `ItemsControl`, with `IsVisible="{Binding PrimeGear.ShowVaulted}"`

## 5. WishlistRelicsViewModel and card changes

- [x] 5.1 Add `bool ShowAllRewards { get; set; }` to `WishlistRelicsViewModel` (default `false`); setter calls `RebuildCards()`
- [x] 5.2 Update `WishlistRelicCardViewModel` constructor to accept a `bool showAllRewards` parameter; when `false`, filter the rewards list passed to `TierGroups` to only those where `IsWishlisted = true`
- [x] 5.3 Pass `ShowAllRewards` from `WishlistRelicsViewModel.RebuildCards()` into each `WishlistRelicCardViewModel`

## 6. Wishlist Relics tab AXAML changes

- [x] 6.1 Add a `CheckBox` + label "Show all rewards" to the control panel `Border`, bound to `WishlistRelics.ShowAllRewards`

## 7. Verification

- [x] 7.1 Build solution and confirm zero errors
- [ ] 7.2 Verify Drop Tables tab: three sections visible, app title gone, Save URL disabled when URL unchanged, import status shows correctly before and after an import
- [ ] 7.3 Verify Prime Gear tab: Show vaulted is in the wishlist panel, Vaulted header appears only when checked
- [ ] 7.4 Verify Wishlist Relics tab: by default only wishlisted rewards shown per card; Show all rewards reveals full card contents
