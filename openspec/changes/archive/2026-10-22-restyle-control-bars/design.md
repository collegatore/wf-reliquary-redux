# Design

## Context

See proposal.md (Why) for motivation and `specs/control-bar-visual-style/spec.md` for the target look.

Current state relevant to the approach:

- Each bar is a `Border` with an inline `Background="#1A808080"`, `CornerRadius="4"`, `Padding="12,8"` (the missions separator panel uses `Padding="12,2"` and squared bottom corners). The tint is repeated across `RelicsPanelView.axaml`, `RelicsMissionsView.axaml` (two layout variants of the missions separator panel), `PrimeGearView.axaml`, and the main-window footer.
- Separators are `<Border Width="1" Background="Gray" Margin="4,0" />` siblings. Buttons set `Background="Transparent" BorderThickness="0"` inline.
- Checkboxes are a bare `CheckBox` next to a sibling `TextBlock`, so the label is not part of the control.
- App-wide styles live in `App.axaml` (`Application.Styles`); card styles live in `MainWindow.axaml` (`Window.Styles`). `MainWindow.axaml` already shows the pattern for neutralizing Fluent button chrome via `/template/ ContentPresenter#PART_ContentPresenter`.
- Font size is user-configurable: `AppearanceViewModel.ApplyFontSize` writes `ContentFontSize` and rescales size resources by `pt / FontSizeMedium`.
- `CardSelectedBrush` (`#332D7DC4`) is an app resource in `App.axaml`, currently used for the selected gear-card wash.
- The Prime Gear "Vaulted" section header already uses `FontWeight="SemiBold"`, a precedent for the heading style.

## Goals / Non-Goals

**Goals:**
- Express the control-bar look once, as named style classes and brush resources, so the views only declare structure (`Classes="button-group"`, `Classes="chip"`, ...).
- Make the look adjustable from one file.

**Non-Goals:**
- No custom templated controls or new control types.
- No dark-theme work: brushes stay single-valued (light) resources, ready to move into `ThemeDictionaries` later like the tier brushes.
- No restyle of the main-window footer, the Drop Tables or Settings tabs, cards, or flyout menus. The footer and the Vaulted header may switch to the shared tint resource without changing their appearance.

## Decisions

### D1. Styles live in a dedicated `ControlBars.axaml` styles file, included from `App.axaml`
A new `Styles/ControlBars.axaml` (`<Styles>` root) holds every control-bar style, and `App.axaml` includes it after `FluentTheme` and the TreeDataGrid theme. The brush resources (`ControlBarBackgroundBrush`, `ControlFieldBackgroundBrush`, `ControlOutlineBrush`, and the hover/pressed tints) live in that file's `Styles.Resources`, and the chip tint references the existing `CardSelectedBrush`.
- *Why:* all control-bar look in one place, next to nothing else. That keeps `App.axaml` (data templates and app resources) readable and makes restyling a single-file edit.
- *Alternatives:* putting the styles inline in `App.axaml` would grow an already mixed file. Per-view `UserControl.Styles` would recreate today's duplication across three views.

### D2. Buttons are separate outlined chips, grouped by a `StackPanel.button-group`
Markup is `<StackPanel Classes="button-group">…Buttons…</StackPanel>`. The class sets horizontal orientation and a 6px spacing, tighter than the bars' 10px spacing between groups. `StackPanel.button-group > Button` gives each button the field look: field background, 1px outline, corner radius 4, 32px minimum height. The hover and press tints and the hover outline are set on `/template/ ContentPresenter#PART_ContentPresenter`, following the existing pattern in `MainWindow.axaml`. The wishlist selector (`Button.bar-field`) shares these setters through a combined selector, so buttons and fields can't drift apart.
The two paired actions in the group-operations row (Select all / Select none, Mark owned / Unmark owned) use `StackPanel.button-group.joined`: zero spacing, the first button keeps only its left corners rounded and drops its right border, and the last keeps only its right corners and its full border, so the last button's left border is the single divider. Each button rounds its own corners, so no clipping container is needed.
- *Why:* each button reads as its own control, matching the checkbox chips beside it. The joined pairs are two halves of one action ("select which", "owned or not"), and joining them says so. Grouping by spacing needs no divider logic. Joined groups rely on `:nth-child(1)` / `:nth-last-child(1)`, which is safe for these always-visible pairs.
- *Alternatives:* the first implementation joined related buttons into one outlined segment with hairline dividers chosen by `:nth-child`. It was replaced after review in favor of separate chips, because the joined buttons looked glued together; joining was then kept only for the two group-operations pairs. A custom panel control is more machinery than three views need.

### D3. Chips are `CheckBox` controls with their label as `Content` and the `chip` class
`<CheckBox Classes="chip" Content="Show owned gear" IsChecked="{Binding …}" />` replaces each `StackPanel(CheckBox + TextBlock)`. `CheckBox.chip` sets padding, outline, and corner radius, and binds `FontSize` to `ContentFontSize`. The label is `Content`, which the app-wide `TextBlock` font-size style doesn't reach. The checked tint is applied to the template's root border (`Border#PART_Border`) under `CheckBox.chip:checked`, using `CardSelectedBrush`, and the pointer-over and pressed states use the hover tints on the same element. The Fluent template fixes the box column at 20px and the box row's height from the `CheckBoxMinHeight` resource (32) inside the template, so the chip keeps the 32px height and shifts the box 8px into the pill with a margin on the template's inner grid.
- *Why:* putting the label in `Content` is the only way to get one click/hover target without code. Styling the stock template keeps keyboard focus and accessibility behavior intact.
- *Alternatives:* a `ToggleButton` chip would lose the visible checkbox glyph, and we decided against that look. Wrapping the existing pair in a `Border` with a pointer handler would need code-behind and still not make the label clickable by default.

### D4. Section headings use a `TextBlock.section-heading` class and a new font-size resource, `SectionHeadingFontSize`
The style sets `FontWeight="SemiBold"` and `FontSize="{DynamicResource SectionHeadingFontSize}"`, plus a right margin that provides the gap before the first control group. `App.axaml` declares the Medium anchor (16, versus content 14). `AppearanceViewModel.ApplyFontSize` rescales it by the same `pt / FontSizeMedium` ratio it already applies to the card widths.
- *Why:* the global `TextBlock` style binds `FontSize` to `ContentFontSize`, so a fixed heading size would stop being "slightly larger" at the Large setting (18) and become larger than intended at Small (11). Scaling it the way card widths already scale keeps the ratio constant.
- *Alternatives:* SemiBold only, without the size bump, would not match the decided look. A `ScaleTransform` would distort the layout box.

### D5. The wishlist field caption is a muted `TextBlock.field-caption` inside the selector button
The selector `Button`'s content becomes a horizontal `StackPanel` of `TextBlock.field-caption` ("Wishlist:") and the existing name `TextBlock`, which keeps its `MaxWidth`, ellipsis, and tooltip. `field-caption` dims the text with `Opacity` (matching the hint texts' existing convention in these views) and sets a small right margin. The selector gets the `bar-field` class: the same field background, outline, and hover as the button chips (D2), so it reads as a control on the bar.
- *Why:* the caption is inside the button's hit area, so clicking it opens the flyout with no extra wiring (spec scenario "Caption does not open anything separately").

### D6. The bar surface becomes a `Border.control-bar` class; the missions separator panel keeps its compact variant
`Border.control-bar` carries the tint, corner radius, margin, and padding. The missions separator panel adds a `compact` class (`Padding="12,2"`), and its squared bottom corners stay local to it. All bars on the two tabs switch to the class, which removes every inline tint there.

### D7. Bar dropdowns take the field look through a scoped `ComboBox` style
`Border.control-bar ComboBox` sets the field background, outline, corner radius, and 32px minimum height on the control, which feeds Fluent's `Border#Background` template part. `:pointerover` and `:pressed` on `Border#Background` apply the hover and pressed tints with the hover outline. Only the closed field is styled; the open list keeps Fluent's look.
- *Why:* the sort-mode dropdown sits next to the wishlist-style fields, so it must look and react the same. Scoping to `.control-bar` keeps dropdowns elsewhere in the app unchanged (an app-wide dropdown restyle is planned in `add-color-palette-setting`).

## Risks / Trade-offs

- [Chip height breaks the compact missions bar, which must stay as tall as the Vaulted header bar] → the chip keeps Fluent's 32px height, which the missions bar already held with its old bare checkbox, so its height is unchanged. Verified by comparing the two bars in snapshots.
- [Fluent `CheckBox` / `Button` template part names differ in Avalonia 12, so selectors silently match nothing] → confirm the part names first (task 1.1). Verify each state (rest, hover, pressed, checked, disabled) visually.
- [Translucent white field background looks wrong in a future dark theme] → accepted. It's a named brush resource (`ControlFieldBackgroundBrush`), so the dark-theme work redefines it in one place.
- [Wider controls (caption + name, chip padding) crowd the Prime Gear primary bar at Large font size] → the search box is docked right, and the left group has room today. Verify at Large font size and a narrow window.

## Open Questions

- Should the wishlist selector also get a dropdown chevron after the name? It is purely decorative and doesn't affect the specs. Decide during visual tuning.
