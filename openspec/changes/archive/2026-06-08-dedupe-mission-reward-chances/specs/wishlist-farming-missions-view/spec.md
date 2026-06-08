## ADDED Requirements

### Requirement: Duplicate relic rewards within a pool are merged with summed chance
When the same relic appears more than once in a single mission reward pool (non-rotation) or in a single rotation (rotation mission) — because the imported drop table lists it multiple times with separate chances — the farming-missions rundown SHALL present it as exactly one relic entry whose chance is the sum of the duplicate chances. Merging SHALL key on the relic identity within its pool/rotation and SHALL occur before per-relic display, before cumulative- and effective-chance computation, and before the reward fingerprint used to group missions by type.

#### Scenario: Same relic listed twice in a non-rotation pool is shown once
- **WHEN** a non-rotation mission's reward pool lists a wishlisted relic twice, at 2% and 1%
- **THEN** the card shows a single row for that relic at 3%

#### Scenario: Same relic listed twice in one rotation is shown once
- **WHEN** a single rotation lists a wishlisted relic twice, at 2% and 1%
- **THEN** that rotation's section shows a single row for that relic at 3%

#### Scenario: Cumulative chance counts a merged relic once
- **WHEN** a pool lists relic R twice (2% and 1%) and a distinct relic S once (4%)
- **THEN** the merged entries are R at 3% and S at 4%, and the cumulative wishlist chance for the pool is 7%

#### Scenario: Merged tables still group together
- **WHEN** two missions of the same type have reward tables that are identical after duplicate relics are merged
- **THEN** the two missions are grouped into a single card

---

## MODIFIED Requirements

### Requirement: Effective chance is computed correctly per mission structure
For non-rotation missions the effective chance SHALL equal the sum of drop chances for all wishlisted relics in the pool, after duplicate relics within the pool have been merged into a single summed entry. For rotation missions the effective chance SHALL equal the arithmetic mean of cumulative wishlist chances across **all** rotations, including rotations with a cumulative chance of 0%, where each rotation's cumulative chance is computed from its merged relic entries. When multiple missions are grouped, their shared reward structure yields a single shared effective chance.

#### Scenario: Non-rotation effective chance is a pool sum
- **WHEN** a non-rotation mission rewards two wishlisted relics at 11.11% and 7.41%
- **THEN** the effective chance displayed on the card is 18.52%

#### Scenario: Rotation effective chance averages all rotations including zero-chance
- **WHEN** a three-rotation mission has rotation A at 18%, rotation B at 18%, rotation C at 0%
- **THEN** the effective chance is 12% (average of 18, 18, 0)

#### Scenario: Duplicate relic chances are summed before the effective chance is computed
- **WHEN** a non-rotation mission lists a single wishlisted relic twice, at 2% and 1%, and no other wishlisted relic
- **THEN** the effective chance displayed on the card is 3%
