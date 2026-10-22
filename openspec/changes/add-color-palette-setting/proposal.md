# Proposal

## Why

The control-bar restyle gave the bars a structure, but their colors are still a flat grey with Fluent's stock blue for "selected", which reads as unfinished. Off-screen renders of candidate palettes showed that a coordinated set (bar, controls, a hover tint marking available options, and a matching selection color) looks markedly better, and that two of them suit different tastes. Users should be able to pick the one they prefer.

## What Changes

- Add a **color palette** setting with two palettes:
  - **Linen** (default): warm linen bars, white controls, a fern-green hover, and an ink-blue selection color.
  - **Stone**: light stone bars, off-white controls, a sage hover, and a heather (dusty mauve) selection color.
- Add a **Palette** selector to the Appearance window. Like the other appearance settings, a change applies to the main window immediately and is saved automatically.
- A palette colors the control bars (bar surface, button and field backgrounds, outlines, the hover and pressed tints, the hover outline), the checked-chip tint and tick, the selected gear-card wash, and the surfaces that share the bar tint (the Prime Gear "Vaulted" header and the footer). The search box gets the same solid background as the other bar fields.
- A palette also colors **every dropdown in the app**: all ComboBoxes (relic sort mode, Drop Tables source, Settings, the recruiting-message dialog) and the Wishlist and Profile selector popups. Their closed field uses the field background and outline, items under the pointer use the hover tint, and the selected or active item uses the selection color. The Profile selector button gets the same field look as the Wishlist selector.
- In the Wishlist and Profile selector popups, the active entry is marked **by the selection color alone**, the same way a ComboBox marks its selected item. The ✓ glyph goes away.
- The cards' **`⋮` menus** (relic cards in Verbatim and Compact modes, gear cards) follow the palette too: an item under the pointer uses the hover tint, pressed uses the pressed tint.
- A card menu **opens with no item highlighted**. Today its first item shows the hover look as soon as the menu opens. Pointing at an item, or moving to it with the keyboard, still highlights it.
- **Inactive controls** (a disabled bar button, chip, or dropdown) get palette-defined colors. How they should look (fade, muted text and outline, or flattened) is an open question to settle with the user before building. It replaces the interim 45% opacity fade from `group-selection-discoverability`.
- **Visible default change:** existing users move from the current grey/blue look to Linen on upgrade. Their settings files, which have no palette field, load as Linen.

Out of scope: a dark theme (palettes are light-only; the brushes stay grouped so a dark variant can be added later), palettes for anything beyond the control bars, dropdowns, card menus, and selection color (card bodies, relic tier colors, the wishlist highlight), the main menu bar's File menu, and user-defined colors.

## Capabilities

### New Capabilities
- `color-palette-setting`: the selectable color palettes (Linen, Stone), what each colors (control bars, dropdowns, card menus, selection color), how dropdowns mark the active item, card menus opening without a highlighted item, live application, and persistence across sessions.

### Modified Capabilities
- `appearance-window`: "Appearance Window Content" adds the palette selector to the window's controls.

## Impact

- Depends on `restyle-control-bars`: the palettes define the control-bar brushes that change introduces (`Styles/ControlBars.axaml`).
- Code: Application (`UserSettings` gains a palette value and a palette enum), Infrastructure (`LocalUserSettingsStore` reads and writes it), Presentation (palette resource dictionaries, a palette applier, `AppearanceViewModel` and `AppearanceWindow`, startup application in `MainWindow`, chip tick, search-box and dropdown styles, the Profile selector in `SettingsView`).
- Tests: settings round-trip and legacy-file default; `AppearanceViewModel` apply and auto-save.
- Tooling: the UI snapshot tool gains a way to render scenes under each palette.
- No database changes.
