# Design

## Context

See proposal.md (Why) and `specs/color-palette-setting/spec.md` for the behavior.

Current state relevant to the approach (assuming `restyle-control-bars` has landed, which this change depends on):

- The control-bar brushes (`ControlBarBackgroundBrush`, `ControlFieldBackgroundBrush`, `ControlOutlineBrush`, `ControlOutlineHoverBrush`, `ControlHoverBrush`, `ControlPressedBrush`) are declared in `Styles/ControlBars.axaml`'s `Styles.Resources` and consumed with `DynamicResource`. `CardSelectedBrush` lives in `App.axaml`'s `Application.Resources` and is used by the checked chip and the selected gear card. The "Vaulted" header and the footer use `ControlBarBackgroundBrush`.
- The chip's checked tick is drawn by Fluent's `Border#NormalRectangle`, colored by the global `CheckBoxCheckBackgroundFillChecked*` resources.
- Appearance settings follow one pattern: a `UserSettings` field with a default, a nullable field in `LocalUserSettingsStore`'s `SettingsFile` (enums via `JsonStringEnumConverter`), an `AppearanceViewModel` property that applies live through a static or delegate and saves immediately, and an application at startup in `MainWindow`'s constructor (`AppearanceViewModel.ApplyFontSize` is the static precedent).
- `LocalUserSettingsStore.LoadAsync` returns `UserSettings.Default` for the whole file on any `JsonException`.
- The Appearance window is fixed-size (498×310) with one radio-button row per setting.
- The off-screen renders that chose these palettes used the snapshot tool's `--set` resource overrides.

## Goals / Non-Goals

**Goals:**
- Swap the whole palette in one operation that every `DynamicResource` consumer follows live.
- Keep each palette's colors together in one file, ready for a future dark variant.
- An unknown palette value never costs the user their other settings.

**Non-Goals:**
- Dark theme, user-defined colors, palettes for cards or tier colors.
- Changing Fluent's global accent color.

## Decisions

### D1. Each palette is a `ResourceDictionary` file, swapped as the app's single merged palette dictionary
`Styles/Palettes/Linen.axaml` and `Styles/Palettes/Stone.axaml` each define the same keys (table below). `App.axaml`'s `Application.Resources` becomes a `ResourceDictionary` whose `MergedDictionaries` holds exactly one palette (Linen at load), alongside the existing app resources. A public static `PaletteApplier.Apply(ColorPalette)` in Presentation replaces that merged entry. The brush declarations move out of `ControlBars.axaml` and `CardSelectedBrush` moves out of `App.axaml`'s direct resources, so each key has a single source: the active palette.
- *Why:* replacing one merged dictionary raises a resources-changed notification that every `DynamicResource` re-resolves against, which is exactly "applies immediately". Keeping a key in both places would let the old value shadow the palette by lookup order.
- *Alternatives:* custom `ThemeVariant`s (`Linen`/`Stone` inheriting `Light`) with `ThemeDictionaries`. They switch through `RequestedThemeVariant`, but would collide with a future light/dark axis (it would need `LinenDark`, `StoneDark`...). Setting individual brushes from code would scatter the palette definition into C#.

| Key | Linen | Stone |
|---|---|---|
| `ControlBarBackgroundBrush` | `#E8E6DF` | `#EEEEE8` |
| `ControlFieldBackgroundBrush` | `#FFFFFF` | `#FAFAF7` |
| `ControlOutlineBrush` | `#B9B5A8` | `#CBCBC0` |
| `ControlOutlineHoverBrush` | `#4E8A5C` | `#6FA37C` |
| `ControlHoverBrush` | `#D7E8D5` | `#E2EDE3` |
| `ControlPressedBrush` | `#BCD9BA` | `#CCE0D0` |
| `CardSelectedBrush` (selection wash) | `#D8E0EE` | `#DFD2E3` |
| `SelectionAccentBrush` (chip tick fill) | `#3E5A8C` | `#876391` |
| `ControlInactiveForegroundBrush` (inactive text) | open, see Open Questions | open |
| `ControlInactiveOutlineBrush` (inactive outline) | open, see Open Questions | open |

### D2. The chip tick is recolored by chip-scoped styles, not by overriding Fluent's resources
`CheckBox.chip:checked /template/ Border#NormalRectangle` (and its `:pointerover` / `:pressed` combinations) set `Background` and `BorderBrush` to `SelectionAccentBrush`. These are pseudo-class style setters, so they win over the theme's.
- *Why:* overriding `CheckBoxCheckBackgroundFillChecked*` globally would also recolor gear-card part checkboxes and the Settings checkboxes, which the spec keeps on the standard accent.

### D3. The search box takes the field background through a scoped style
`Border.control-bar TextBox` sets `Background` to `ControlFieldBackgroundBrush` and `BorderBrush` to `ControlOutlineBrush`. Fluent's focus and pointer-over states on the template keep working, because those setters sit under pseudo-classes.

### D3b. Dropdowns get app-wide styles in a new `Styles/Dropdowns.axaml`, included after `ControlBars.axaml`
Unlike the bar styles, these selectors are not scoped to `.control-bar`, because every dropdown in the app follows the palette. Fluent 12.0.5 template facts: the closed `ComboBox` draws in `Border#Background`, the open list in `Border#PopupBorder`, and each `ComboBoxItem` in its `ContentPresenter`, recolored by the theme on `:pointerover`, `:pressed`, and `:selected`.
- **Closed ComboBox:** `Background` = `ControlFieldBackgroundBrush` and `BorderBrush` = `ControlOutlineBrush` on the control (these feed the template). `:pointerover` / `:pressed` on `Border#Background` use `ControlHoverBrush` / `ControlPressedBrush` with `ControlOutlineHoverBrush`. This is the restyle's `Border.control-bar ComboBox` style made app-wide: those bar-scoped styles move here without the `.control-bar` scope, so there's one ComboBox style.
- **Items:** `ComboBoxItem:pointerover` / `:pressed` on the `ContentPresenter` use the hover and pressed tints. `:selected` (and `:selected:pointerover`) uses `CardSelectedBrush`, keeping the default text color so the label stays readable on the wash. The popup frame keeps Fluent's light background, which is near-white in both palettes.
- **Selector popups (Wishlist, Profile):** the existing `FlyoutPresenter Button` style moves here from `MainWindow.axaml`. Entries get a transparent background and the hover/pressed tints on `PART_ContentPresenter`. Each list entry binds `Classes.active` to its `IsActive`, and `FlyoutPresenter Button.active` sets the entry's `Background` to `CardSelectedBrush`. The active entry is marked by that color alone: the entry templates in `PrimeGearView.axaml` and `SettingsView.axaml` drop the ✓ `TextBlock` and its blank placeholder, so names align without a glyph column. The `⋮` menus are `MenuFlyout`s (`MenuItem`, not `Button`), so these selectors don't touch them (see D3c).
- **Field look:** the restyle's `Button.bar-field` class becomes `Button.dropdown-field` and moves to this file. Both the Wishlist selector and the Profile selector in `SettingsView` use it.
- *Why:* one file owns "what a dropdown looks like" for the whole app, separate from bar layout, and every color comes from the palette keys in D1, so no new keys are needed.
- *Alternatives:* restyling only the control-bar dropdowns (rejected; the user asked for all of them); a custom `ControlTheme` for `ComboBox` (a full template copy for a color change).

### D3c. Card menus get palette styles scoped by a presenter class, and highlight only on pointer or keyboard
The three card `MenuFlyout`s (the Verbatim relic card template in `App.axaml`, the Compact one in `RelicsPanelView.axaml`, and the gear card in `PrimeGearView.axaml`) set `FlyoutPresenterClasses="card-menu"`. Styles in `Dropdowns.axaml` target `MenuFlyoutPresenter.card-menu MenuItem`, so the File menu is not affected.

Fluent 12.0.5 has no separate hover style for `MenuItem`. Its hover look is the `:selected` state (`Border#PART_LayoutRoot` background), which the pointer sets on enter, and which the menu also sets on its first item when it opens. That is the highlighted-on-open item. The styles therefore:
1. reset `MenuItem:selected /template/ Border#PART_LayoutRoot` to a transparent background;
2. apply `ControlHoverBrush` on `MenuItem:pointerover` and on `MenuItem:focus-visible` (keyboard focus), declared after the reset so they win;
3. apply `ControlPressedBrush` on `:pressed /template/ Border#PART_LayoutRoot:pointerover`, as the theme does.
An item selected programmatically on open is neither pointed at nor keyboard-focused, so it stays plain, and pointer and arrow-key navigation still highlight.
- *Fallback:* if the menu opened by a click turns out to give its first item `:focus-visible`, handle the flyout's `Opened` event to clear the presenter's selection and keep focus on the presenter itself. Task 3.4 checks this first.
- *Alternatives:* code-behind on every menu from the start (more moving parts for what styles can likely do); app-wide `MenuItem` styles (would also restyle the File menu, which is out of scope).

### D4. The palette is persisted as a string and parsed leniently
`ColorPalette { Linen, Stone }` lives in `Application/Settings`. `UserSettings` gains `ColorPalette Palette = ColorPalette.Linen`. `SettingsFile` stores `string? Palette`, written as the enum name and read with `Enum.TryParse(ignoreCase: true)`, falling back to Linen.
- *Why:* the store resets every setting on a `JsonException`, and `JsonStringEnumConverter` throws on an unknown name, so a strict enum field would turn one bad palette value into a full settings wipe. The other enum fields keep their converter; only the new field avoids the risk.

### D5. `AppearanceViewModel.SelectedPalette` applies and saves like the other settings; startup applies the saved palette
`SelectedPalette` calls `PaletteApplier.Apply` and then saves with `current with { Palette = … }`, mirroring `SelectedViewMode`. `MainWindow`'s constructor applies `initialSettings.Palette` next to the existing `ApplyFontSize` call, before the window is shown, so there's no flash of Linen when Stone is saved. The Appearance window gets a "Palette" row with Linen and Stone radio buttons (the window's existing `EnumToBoolConverter` pattern), and its fixed height grows to fit one more row.

### D6. The snapshot tool renders scenes under a chosen palette
`Scene` gains an optional `ColorPalette`, and `Program` calls `PaletteApplier.Apply` before each scene (Linen by default). The scene list gains Stone variants of the Prime Gear and side-by-side Relics scenes. `--set` overrides still apply on top for trying new colors.

## Risks / Trade-offs

- [Every user's look changes on upgrade (grey/blue → Linen)] → intended, since that's the default the user chose. The changelog entry for the release should mention the new palette and the Stone option.
- [Moving `CardSelectedBrush` into the palettes changes the selected gear-card wash from the current blue to ink] → required by the spec (selection color is one color across the app). Verify the wash is still readable on owned (faded) cards.
- [A `ResourceInclude` swap at runtime might not refresh every consumer, for example ones resolved with `StaticResource`] → all consumers of these keys use `DynamicResource`. Verify by switching palettes in the running app with each tab open.
- [Fluent's `:selected` item style also changes the item's foreground, which could clash with the selection wash] → set the foreground explicitly in the `:selected` styles if needed. Verify the selected item's text in an open list under both palettes.
- [`FlyoutPresenter Button` also matches buttons in any future flyout] → intended, since flyout buttons are dropdown entries in this app. If a flyout ever needs regular buttons, scope the selector with a class on the flyout's content.
- [The translucent-to-opaque change: the old brushes were alpha over the window background; palette colors are opaque] → on purpose, for predictable colors. A dark variant will define its own opaque set.

## Open Questions

- **How should inactive controls look?** (Must be answered with the user before task 3.5; it decides the values of the two `ControlInactive*` keys and the inactive style.) Today `group-selection-discoverability` greys out an inactive bar button by fading the whole button to 45% opacity, as a stopgap. Candidates:
  1. **Opacity fade** (the stopgap): simplest, but the faded fill lets the bar color show through, and a faded divider in a joined pair looks broken.
  2. **Muted text and outline, unchanged fill:** the button keeps its field background, and its text and outline switch to the palette's `ControlInactiveForegroundBrush` / `ControlInactiveOutlineBrush`. Crisp, and it reads as "present but off". This is the likely recommendation.
  3. **Flattened:** no fill and no outline, muted text only, so the button recedes into the bar. Clearest "not now", but the row's layout looks emptier.

  Whichever is chosen, the colors come from the palette (one inactive text color and one inactive outline per palette), so Linen and Stone can each tune the contrast against their bar. The choice also applies to the dropdown and chip disabled states, for consistency.
