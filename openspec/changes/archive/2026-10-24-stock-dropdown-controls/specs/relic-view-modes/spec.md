# Spec Delta

## MODIFIED Requirements

### Requirement: Rewards modal pictogram
Each top-level relic row in Table mode SHALL provide a pictogram action, drawn as an icon from the `ui-iconography` capability rather than a text glyph, that opens a modal dialog showing that relic rendered as a **Verbatim** card in all-rewards mode (verbatim names, tier groups). Non-wishlisted rewards SHALL render dimmed exactly as on the main-panel Verbatim card; wishlisted rewards SHALL receive no additional highlight. The dialog SHALL be closable only via the window's native close control (no in-content Close button), returning the user to the table unchanged.

#### Scenario: Pictogram opens the Verbatim card modal
- **WHEN** the user activates the rewards pictogram on a relic row
- **THEN** a modal dialog opens showing that relic's Verbatim card with all rewards; non-wishlisted rewards are dimmed and wishlisted rewards render at full strength with no background highlight

#### Scenario: Closing the modal preserves table state
- **WHEN** the user closes the rewards modal via the window's native close control
- **THEN** the table is unchanged, including row expansion states

#### Scenario: Rewards pictogram is an icon with a tooltip
- **WHEN** the user rests the pointer on the rewards pictogram of a relic row
- **THEN** the pictogram is drawn as an icon in the row's text color and a tooltip names the action

---

### Requirement: Recruiting Chat message pictogram
Each top-level relic row in Table mode SHALL provide a pictogram action, drawn as an icon from the `ui-iconography` capability rather than a text or emoji glyph, that opens the existing Recruiting Chat message composer for that relic, equivalent to the "Compose Recruiting Chat message…" action on relic cards.

#### Scenario: Pictogram opens the Recruiting Chat message composer
- **WHEN** the user activates the Recruiting Chat message pictogram on a relic row
- **THEN** the Recruiting Chat message composer dialog opens for that relic

#### Scenario: Recruiting pictogram is an icon with a tooltip
- **WHEN** the user rests the pointer on the Recruiting Chat message pictogram of a relic row
- **THEN** the pictogram is drawn as an icon in the row's text color, not a colored emoji, and a tooltip names the action
