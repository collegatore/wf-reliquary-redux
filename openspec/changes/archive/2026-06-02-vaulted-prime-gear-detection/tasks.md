## 1. Domain Model

- [x] 1.1 Add `bool IsVaulted` property to `PrimeGearEntry` record

## 2. Infrastructure — Schema & Import

- [x] 2.1 Add `gear_vault_status (gear_name TEXT PRIMARY KEY, is_vaulted INTEGER NOT NULL)` to the CREATE TABLE block in `SqliteDropTableImportStore`
- [x] 2.2 Add `gear_vault_status` to `ClearSnapshotAsync` DELETE list
- [x] 2.3 At the end of `InsertSnapshotAsync`, after all relics and mission_rewards are inserted, populate `gear_vault_status` with a CTE-based INSERT that derives gear names via `TRIM(SUBSTR(name, 1, INSTR(name, ' Prime ') + 5))` and sets `is_vaulted = 1` when no gear relic appears in `mission_rewards`

## 3. Infrastructure — Query

- [x] 3.1 Rewrite `SqlitePrimeGearReader` SQL to join `gear_vault_status` (gear name + vault status) with `reward_items` (parts), ordered by `gear_name, item_name`
- [x] 3.2 Update the C# reader loop to group by `gear_name` from the table; use `DeriveGearAndPart` only to extract the part name; set `IsVaulted` from `is_vaulted`

## 4. Presentation — ViewModel

- [x] 4.1 Add `bool IsVaulted` property to `PrimeGearCardViewModel`, set from `PrimeGearEntry.IsVaulted`

## 5. Presentation — UI

- [x] 5.1 Add a "Vaulted" text badge element to the prime gear card AXAML template, bound to `IsVaulted` (visible only when true)
- [x] 5.2 Apply reduced-opacity style to vaulted cards (bind card opacity or use a style selector on `IsVaulted`)
