## Why

Users currently see all prime gear listed identically regardless of availability. Distinguishing vaulted gear (whose relics have been removed from mission rotations) helps players prioritize active farming vs. waiting for vault rotations.

## What Changes

- Add `IsVaulted` flag to `PrimeGearEntry` — true when no relic that rewards any of the gear's parts appears in `mission_rewards`
- Update `SqlitePrimeGearReader` SQL to compute vault status per gear in the same query
- Update `PrimeGearCardViewModel` to expose `IsVaulted`
- Update the prime gear cards UI to visually distinguish vaulted gear (e.g., a "Vaulted" badge or muted styling)

## Capabilities

### New Capabilities

- `prime-gear-vault-status`: Determining whether a prime gear set is vaulted based on whether any of its relics can be obtained from a mission, and surfacing that status in the prime gear cards view

### Modified Capabilities

- `prime-gear-parts-view`: Card display now includes a vaulted indicator; the query now returns vault status per gear

## Impact

- `PrimeGearEntry` record gains a new `bool IsVaulted` property — a non-breaking struct addition
- `SqlitePrimeGearReader.GetAllAsync` SQL query extended with a LEFT JOIN to `mission_rewards`
- `PrimeGearCardViewModel` and `PrimeGearViewModel` updated in the Presentation layer
- Prime gear AXAML view updated with a vaulted indicator element
