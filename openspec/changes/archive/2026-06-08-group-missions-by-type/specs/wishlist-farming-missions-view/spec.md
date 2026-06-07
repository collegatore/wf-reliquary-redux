## MODIFIED Requirements

### Requirement: Farming missions panel shows missions that reward wishlisted relics
The "Relics and Missions" tab SHALL include a farming missions panel displayed below the relic cards panel, separated from it by a divider. The panel SHALL display every mission that rewards at least one relic containing a wishlisted part, grouped into cards by mission type and identical reward table. Groups SHALL be ordered by effective wishlist-relic drop chance descending.

#### Scenario: Panel populated when wishlist has matching relics
- **WHEN** the active wishlist contains parts that are found in relics rewarded by one or more missions
- **THEN** those missions are shown in the farming missions panel as grouped cards, ordered by effective chance descending

#### Scenario: Panel is empty when no missions match
- **WHEN** the active wishlist is empty, or no mission rewards a relic containing a wishlisted part
- **THEN** the farming missions panel shows a placeholder (no cards)

#### Scenario: Panel refreshes when active wishlist changes
- **WHEN** the active wishlist is changed
- **THEN** the farming missions panel reloads to reflect the new wishlist

---

### Requirement: Non-rotation mission card shows contributing relics
A mission card for a non-rotation group SHALL display the group title (mission type or display label), the effective wishlist chance as a percentage, a nodes section listing member missions, and the wishlisted relics available in the pool. Relic display format is controlled by the "Show exact drop chances" toggle: per-relic rows with chances when on, a single compact row of names when off.

#### Scenario: Card header shows group title and effective chance
- **WHEN** a non-rotation group card is displayed
- **THEN** the card header shows the mission type (or display label for ungrouped missions) and the cumulative wishlist chance formatted as a percentage

#### Scenario: Contributing relics listed per-row with chances when toggle is on
- **WHEN** a non-rotation group card is displayed and "Show exact drop chances" is checked
- **THEN** each relic that contributes to the wishlist chance is listed with its name and individual chance percentage

#### Scenario: Relics shown compact when toggle is off
- **WHEN** a non-rotation group card is displayed and "Show exact drop chances" is unchecked
- **THEN** all contributing relics are shown as a single comma-separated row with no chance text

---

### Requirement: Rotation mission card shows per-rotation breakdown
A mission card for a rotation-based group SHALL display the group title, the effective chance (average across all rotations), a nodes section, and a section for each rotation. Each rotation section SHALL show the rotation display label and cumulative wishlist chance for that rotation, and SHALL list contributing relics in the format controlled by the "Show exact drop chances" toggle. Rotations with no wishlisted relics SHALL be shown explicitly at 0%.

#### Scenario: Rotation card header shows group title and average chance
- **WHEN** a rotation group card is displayed
- **THEN** the card header shows the group title and the average wishlist chance across all rotations, formatted as a percentage

#### Scenario: Each rotation is shown including zero-chance rotations
- **WHEN** a rotation group card is displayed
- **THEN** every rotation for the group is listed; rotations with no wishlisted relics show 0%

#### Scenario: Rotation sections list relics per-row with chances when toggle is on
- **WHEN** a rotation has at least one wishlisted relic reward and "Show exact drop chances" is checked
- **THEN** that rotation's section lists each contributing relic with its name and chance percentage

#### Scenario: Rotation sections show compact relics when toggle is off
- **WHEN** a rotation has at least one wishlisted relic reward and "Show exact drop chances" is unchecked
- **THEN** that rotation's section shows all contributing relics as a single comma-separated row with no chance text

---

### Requirement: Effective chance is computed correctly per mission structure
For non-rotation missions the effective chance SHALL equal the sum of drop chances for all wishlisted relics in the pool. For rotation missions the effective chance SHALL equal the arithmetic mean of cumulative wishlist chances across **all** rotations, including rotations with a cumulative chance of 0%. When multiple missions are grouped, their shared reward structure yields a single shared effective chance.

#### Scenario: Non-rotation effective chance is a pool sum
- **WHEN** a non-rotation mission rewards two wishlisted relics at 11.11% and 7.41%
- **THEN** the effective chance displayed on the card is 18.52%

#### Scenario: Rotation effective chance averages all rotations including zero-chance
- **WHEN** a three-rotation mission has rotation A at 18%, rotation B at 18%, rotation C at 0%
- **THEN** the effective chance is 12% (average of 18, 18, 0)

---

### Requirement: Vaulted relics are excluded implicitly
The farming missions panel SHALL only show missions and relics that are currently obtainable. Because a relic is considered vaulted if and only if no mission rewards it, the join through mission reward data excludes vaulted relics without any additional filter.

#### Scenario: Vaulted relics do not appear in any mission card
- **WHEN** a wishlist contains a part found only in vaulted relics
- **THEN** no mission card is shown for that part in the farming missions panel

---

### Requirement: Farming-mission card width scales with the selected font size
Each farming-mission card's width SHALL scale proportionally with the application's content font size, using the same font-ratio rule as relic cards, so that the card grows and shrinks together with the text it contains. Card width SHALL update when the font size setting changes, without requiring an application restart.

#### Scenario: Larger font widens mission cards
- **WHEN** the user selects the Large font size
- **THEN** each farming-mission card is rendered wider than it is at the Medium font size

#### Scenario: Smaller font narrows mission cards
- **WHEN** the user selects the Small font size
- **THEN** each farming-mission card is rendered narrower than it is at the Medium font size

#### Scenario: Mission card width updates live with the font setting
- **WHEN** the user changes the font size while farming-mission cards are visible
- **THEN** the mission cards resize to the new width without an application restart
