## Why

Several small UX rough edges on the Relics and Missions tab make the layout feel unpolished: uneven spacing above the first row of Mission cards, an unlabelled controls panel that mixes Relic and Mission controls without distinction, a stray keyboard shortcut for Settings, and inconsistent button-background tones in dropdowns.

## What Changes

- Fix top-row spacing on Mission cards so the gap above the first row matches the gap between rows
- Add a **"Relics:"** label to the controls panel above relic cards
- Rename the **"Show vaulted relics"** checkbox to **"Show vaulted"**
- Move **"Show exact drop chances"** into its own separator section, preceded by a **"Missions:"** label
- Remove the **Ctrl+,** keyboard shortcut for opening Settings
- Lighten button backgrounds in all dropdown controls

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities

- `wishlist-relics-view`: control panel gains a "Relics:" label; "Show vaulted relics" checkbox renamed to "Show vaulted"
- `exact-chances-toggle`: toggle moves to a dedicated Missions separator section with a "Missions:" label; no longer grouped with the Relics controls
- `settings-tab`: Settings keyboard shortcut (Ctrl+,) requirement removed

## Impact

- **UI only** — no data model, database, or API changes
- Affected XAML: Relics and Missions tab layout (control panel, missions panel wrap/item containers), dropdown/ComboBox styles
- Affected spec files: `wishlist-relics-view`, `exact-chances-toggle`, `settings-tab`
- No breaking changes to existing behavior beyond label text and layout reorganisation
