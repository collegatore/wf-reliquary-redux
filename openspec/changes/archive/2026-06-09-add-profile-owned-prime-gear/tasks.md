## 1. Application layer (port + domain)

- [x] 1.1 Add `IOwnedGearStore` port in `src/Application/WF.Reliquary.Application/Ports/` with `GetOwnedGearNamesAsync(CancellationToken)` and `SetOwnedAsync(string gearName, bool owned, CancellationToken)`, documenting that it resolves the active profile internally (mirror `IWishlistStore` doc comments).

## 2. Infrastructure layer (entity, schema, store, migration)

- [x] 2.1 Add `OwnedGearEntity` (`Id`, `ProfileId`, `GearName`) to `src/Infrastructure/.../Storage/Entities/UserDataEntities.cs` with a soft-reference comment on `GearName`.
- [x] 2.2 Register `DbSet<OwnedGearEntity> OwnedGear` in `ReliquaryDbContext` and configure the `owned_gear` table in `OnModelCreating`: columns `id`/`profile_id`/`gear_name`, unique index on `(ProfileId, GearName)`, FK `profile_id` → `profiles` with `DeleteBehavior.Cascade`.
- [x] 2.3 Implement `SqliteOwnedGearStore : IOwnedGearStore` in `src/Infrastructure/.../DropTables/`, resolving the active profile from `user_preferences.last_profile_id` (fallback earliest profile) like `SqliteProfileStore`/`SqliteWishlistStore`; `SetOwnedAsync` inserts (guarded by the unique index) when owned and `ExecuteDeleteAsync` when not.
- [x] 2.4 Generate the EF migration: `dotnet ef migrations add AddOwnedGear` (run `dotnet tool restore` first if needed); verify the generated `Up`/`Down` create and drop `owned_gear` with the FK and unique index.

## 3. Composition

- [x] 3.1 Add `CreateOwnedGearStore()` factory method returning `new SqliteOwnedGearStore()` to `ReliquaryServiceFactory`.

## 4. Presentation — view models

- [x] 4.1 Extend `PrimeGearCardViewModel` to implement `INotifyPropertyChanged` and carry an `IsOwned` property, a `MarkOwnedCommand` (`RelayCommand` toggling `IsOwned`), a state-aware `OwnedMenuLabel` ("Mark as owned" / "Unmark as owned"), and an `IsFadingOut` flag; pass the owned store, gear name, wished part view-models, and a parent callback into its constructor.
- [x] 4.2 Implement the `IsOwned` setter: short-circuit when unchanged; on the `false`→`true` edge, if the card has wished parts, invoke the confirm callback and act on the three-way result (keep tracking → persist owned; release tracking → persist owned and `RemoveItemAsync` each wished part from the active wishlist; cancel → revert `IsOwned` to false, raise `PropertyChanged`, persist nothing); with no wished parts, persist owned directly. Clearing owned persists directly with no dialog.
- [x] 4.3 Add an `OwnedConfirmChoice` enum (`KeepTracking`, `ReleaseTracking`, `Cancel`) and a `Func<string, Task<OwnedConfirmChoice>>?` confirm callback on `PrimeGearViewModel`, forwarded into each card so the card can request confirmation without referencing `Window`.
- [x] 4.4 In `PrimeGearViewModel`: inject `IOwnedGearStore`, load the owned set in `LoadAsync`, set each card's `IsOwned`, and keep the full built card list separate from the bound `ActiveCards`/`VaultedCards`.
- [x] 4.5 Add a `ShowOwned` bool property to `PrimeGearViewModel` (default false) that repopulates `ActiveCards`/`VaultedCards` on change, excluding owned cards unless `ShowOwned` is true.
- [x] 4.6 On a single card's `IsOwned` toggle, do a targeted add/remove (`OnCardOwnedChangedAsync` + ordered insert) rather than a full rebuild, so only the affected card changes; keep the full `RebuildVisibleCards` for load and `ShowOwned`.
- [x] 4.7 Mask the hide latency: overlap the DB write with the UI update in `PersistOwnedAsync`, and fade the card (`IsFadingOut` → opacity transition) before removal, gated by an injectable `OwnedHideFadeDuration` (default zero for tests; view sets ~180 ms).

## 5. Presentation — confirm-tracking dialog

- [x] 5.1 Create `OwnedGearTrackingDialog` (`.axaml` + `.axaml.cs`) modelled on `ConfirmDiscardDialog`: a `Window` with three buttons — **Keep tracking**, **Release tracking**, **Cancel** (the hide effect is conveyed in the message text and button tooltips) — and a static `ShowAsync(Window owner, string gearName)` returning `OwnedConfirmChoice` (`Cancel` is the enum `0` value, so closing/dismissing maps to `Cancel`).
- [x] 5.2 Wire the dialog in `MainWindow.axaml.cs`: set `primeGearVm.ConfirmOwnedTracking = gearName => OwnedGearTrackingDialog.ShowAsync(this, gearName)`, mirroring the `SettingsViewModel.ConfirmDiscard` wiring at `MainWindow.axaml.cs:82`.

## 6. Presentation — AXAML

- [x] 6.1 In `MainWindow.axaml`, add a **Show owned gear** checkbox + label (preceded by a vertical separator) to the control-panel row containing Show vaulted gear, bound to `PrimeGear.ShowOwned`.
- [x] 6.2 Change the gear card header (both Active and Vaulted templates) to a two-column layout: gear name stretched left, a ⋮ overflow `Button` on the right whose `MenuFlyout` has a single `MenuItem` bound to `OwnedMenuLabel`/`MarkOwnedCommand`; flyout uses `Placement="BottomEdgeAlignedRight"` + small `HorizontalOffset` to open toward the name, with a `ToolTip.Tip` (bound to `OwnedMenuLabel`) and show delay.
- [x] 6.3 Make muted/fade opacities class-driven (`owned`, `gear-vaulted`, `fading`) and add an `Opacity` `Transition` to both card Borders so revealed owned/vaulted cards stay muted and a just-owned card fades out (`fading` declared last to win).

## 7. Tests

- [x] 7.1 Add infrastructure tests for `SqliteOwnedGearStore`: mark/clear round-trip, idempotent mark, per-profile isolation, and survival across a simulated re-import.
- [x] 7.2 Add a migration test for `AddOwnedGear` (mirror `AddPlayerProfilesMigrationTests`) verifying the `owned_gear` table, FK cascade, and unique index.
- [x] 7.3 Add `PrimeGearViewModel` tests: cards reflect persisted owned state, `ShowOwned` toggles owned-card visibility across both sections, and toggling `IsOwned` updates the bound collections.
- [x] 7.4 Add confirm-flow tests with a stubbed confirm callback: dialog requested only when wished parts exist and only on the owned edge; KeepTracking persists owned and leaves the wishlist; ReleaseTracking persists owned and removes the gear's parts from the active wishlist; Cancel persists nothing and reverts `IsOwned`.

## 8. Verification

- [x] 8.1 Run `dotnet build` and `dotnet test`; fix failures.
- [x] 8.2 Run the app: mark gear with wished parts owned and exercise all three dialog choices; mark gear with no wished parts (no dialog); enable Show owned gear (owned reappears muted); confirm persistence across restart and isolation across profiles.
- [x] 8.3 Note in the change/PR that a schema migration was added (no data re-initialization required, per the pre-V1 persistence rule).
