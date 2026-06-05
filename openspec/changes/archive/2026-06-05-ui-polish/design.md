## Context

Ten UI refinements spread across the Drop Tables, Prime Gear, and Wishlist Relics tabs. All changes are self-contained: no new routes, no schema changes, no new tests beyond what already exists. One new Application port (`IImportMetadataReader`) and one Infrastructure implementation are required to expose import status for the Drop Tables tab.

Current state of the tabs is fully understood from `MainWindow.axaml`, existing ViewModels, and the four affected specs.

## Goals / Non-Goals

**Goals:**
- Restructure the Drop Tables tab into three labelled sections separated by visual dividers
- Remove the redundant app-name `TextBlock` from the Drop Tables tab
- Disable Save URL when the field value matches the already-saved URL
- Rename "Last update" to "Downloaded file version" and add a parallel "Imported file version" line
- Relocate the Show vaulted checkbox into the wishlist-selector panel; replace the standalone separator with a labelled "Vaulted" section header
- Add a "Show all rewards" checkbox to Wishlist Relics; filter cards to wishlisted-only rewards by default

**Non-Goals:**
- No schema changes
- No new unit or integration tests (existing tests cover the affected ViewModel logic)
- No change to routing, navigation, or tab structure

## Decisions

### Drop Tables tab layout

Replace the current flat `Grid` with a `StackPanel` of three section blocks. Each block is: a `TextBlock` section header + `Separator` + section content `StackPanel`. This is the simplest structure; no new custom controls needed.

### Save URL disabled state — CanSaveUrl

Add a `bool CanSaveUrl` computed property to `MainWindowViewModel` that returns `!string.Equals(SourceUrlText, SavedSourceUrl, StringComparison.Ordinal)`. A private `SavedSourceUrl` field is updated whenever settings are loaded or saved. The `SaveSourceUrlCommand` already exists; bind `IsEnabled` to `CanSaveUrl` (Avalonia's `Command` binding does `CanExecute`, but the simplest route is a dedicated property since the command's `CanExecute` currently only checks `!IsBusy`).

### Import status text — IImportMetadataReader

A new Application port `IImportMetadataReader` with one method:
```csharp
Task<string?> GetImportedVersionAsync(CancellationToken ct = default);
```
Returns `null` when no import has been done, or the `source_last_update` string when available (mirrors the existing "Last Update" field already stored in `import_metadata`).

Infrastructure: `SqliteImportMetadataReader` — a simple EF query on `ImportMetadataEntity` using the existing `ReliquaryDbContext`.

`MainWindowViewModel` gets a new `ImportStatusText` property (string), loaded in `LoadStatusAsync` and refreshed after a successful import alongside `StatusText`.

### Prime Gear — Show vaulted moves to wishlist panel

The `CheckBox` + label for `ShowVaulted` moves inside the existing wishlist-selector `Border > StackPanel` (appended after the wishlist button). The standalone separator `Border` below the Active cards is removed entirely. A new `Border` containing only a `TextBlock` labelled **"Vaulted"** (same visual style as the wishlist panel) is inserted between Active and Vaulted `ItemsControl` blocks, with `IsVisible` bound to `PrimeGear.ShowVaulted`. This gives the section a clear heading without the checkbox being duplicated.

### Wishlist Relics — ShowAllRewards filter

Add `bool ShowAllRewards { get; set; }` to `WishlistRelicsViewModel` (default `false`). Pass the flag into `WishlistRelicCardViewModel` constructor. In `WishlistRelicCardViewModel`, filter the rewards used to build `TierGroups` when `showAllRewards = false`: only rewards where `IsWishlisted = true` are passed to the tier groups. When `ShowAllRewards = true`, all rewards are passed (current behavior). `ShowAllRewards` setter calls `RebuildCards()` like `ShowVaulted` already does.

The checkbox lives in the existing control panel `Border` alongside the existing "Show Vaulted Relics" checkbox.

## Risks / Trade-offs

- **CanSaveUrl and IsBusy interact**: the Save URL button needs both `CanSaveUrl` and `!IsBusy` to be true. Binding `IsEnabled` to `CanSaveUrl` and leaving the `Command` binding as-is means both conditions apply independently. This is correct but worth noting: the button can appear disabled even when not busy if the URL is unchanged.
- **WishlistRelicCardViewModel is currently immutable**: adding a `showAllRewards` constructor parameter is a one-line change, but it means the card can't switch modes after construction — `RebuildCards()` reconstructs all cards on toggle, which is the current pattern and works fine.
