## MODIFIED Requirements

### Requirement: Side-by-Side Minimum Width

In the side-by-side layout, the Relics panel SHALL enforce a minimum width derived from the
current effective relic card width (the configured base width scaled by the current font-size
ratio, plus card margin), so the user cannot drag the vertical splitter to a point where the
Relics panel cannot display a full card column. The Missions panel SHALL continue to enforce its
existing fixed minimum width, since mission card width is not user-configurable. Because both
minimums are derived declaratively from resource/setting state (not from measuring rendered card
elements), this SHALL NOT require runtime measurement of card sizes.

#### Scenario: Splitter cannot collapse a panel below one card width

- **WHEN** the user drags the vertical splitter toward one side beyond that panel's
  minimum width
- **THEN** that panel stops shrinking at its minimum width and remains able to show a
  card column

#### Scenario: Relics minimum width follows the configured relic card width

- **WHEN** the user sets a relic card base width other than the default 300 pixels
- **THEN** the Relics panel's minimum width in the side-by-side layout is recalculated
  from the new effective (font-scaled) width, so the panel still cannot be dragged
  narrower than one full relic card column

#### Scenario: Relics minimum width follows font-size changes

- **WHEN** the user changes the font size while the side-by-side layout is active
- **THEN** the Relics panel's minimum width is recalculated from the new effective relic
  card width, so the panel still cannot be dragged narrower than one full relic card column
