## Context

The SQLite database already stores all prime component items (kind `PrimePart`) via the `reward_items` table populated during import. Item names follow the pattern `{GearName} Prime {PartName}`, e.g. `Afuris Prime Receiver`, making the parent gear name and part name trivially derivable from the stored name. No schema changes are needed.

The current UI is a single flat `Grid` in `MainWindow`. Introducing a second tab requires wrapping the existing content in a `TabControl`.

## Goals / Non-Goals

**Goals:**
- Display a Prime Gear tab with one card per prime item, each showing the gear name and a checkbox list of its component parts.
- Derive gear name and part label from `reward_items.name` at query time without schema changes.
- Wire the query through the standard application port → infrastructure → presentation layering.

**Non-Goals:**
- Persisting checkbox state — checkboxes are UI-only, no storage for checked/unchecked.
- Filtering, searching, or sorting gear cards.
- Showing relic or mission context on this tab.
- Handling non-prime blueprint items (`item_kind = 'Blueprint'` or `'Other'`).

## Decisions

### Gear name derivation in SQL

The parent gear name is extracted as the substring up to and including the word "Prime":

```sql
TRIM(SUBSTR(name, 1, INSTR(name, ' Prime ') + 5)) AS gear_name,
TRIM(SUBSTR(name, INSTR(name, ' Prime ') + 7))    AS part_name
```

This works for all current `PrimePart` items because every such name contains ` Prime ` (space-bounded). Grouping and sorting happen in the query.

**Alternative considered**: derive grouping in C# after fetching all names. Rejected because SQL grouping reduces data transfer and keeps the logic in one place.

### Application port

A new read-side port `IPrimeGearReader` is added to the application layer:

```csharp
public interface IPrimeGearReader
{
    Task<IReadOnlyList<PrimeGearEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed record PrimeGearEntry(string GearName, IReadOnlyList<string> Parts);
```

Implemented in infrastructure alongside the existing import store. No workflow or service layer needed — it is a direct query.

**Alternative considered**: reuse `IDropTableImportStore` for reads. Rejected because that interface is write-only by design and mixing read concerns would widen its contract.

### Presentation structure

- `MainWindow` is refactored to use `TabControl` with two tabs: the existing controls move into a **Drop Tables** tab; the new **Prime Gear** tab hosts the cards view.
- A new `PrimeGearViewModel` owns `ObservableCollection<PrimeGearCardViewModel>` and loads data on construction via an injected `IPrimeGearReader`.
- `PrimeGearCardViewModel` holds `GearName` and `ObservableCollection<PrimePartViewModel>` (name + `IsChecked`).
- Cards are rendered with a wrapping `ItemsControl` (WrapPanel layout); each card uses a `Border` with a header `TextBlock` and a checkboxes `ItemsControl`.

## Risks / Trade-offs

- **Name-based derivation is fragile**: if DE ever ships a prime item whose name does not follow the `{X} Prime {Part}` pattern, the SQL substring logic will produce a wrong gear name or omit the item. Mitigation: the `item_kind = 'PrimePart'` filter already guarantees the parser judged the name prime-part-shaped; a unit test over a known set catches regressions.
- **TabControl refactor touches MainWindow**: the existing drop-table UI must be moved into a tab without breaking its bindings. Mitigation: keep the existing `MainWindowViewModel` intact; introduce `PrimeGearViewModel` as a separate VM injected alongside.
- **Checkbox state is ephemeral**: users lose check progress on restart. Accepted for now per Non-Goals; a future change can add persistence.
