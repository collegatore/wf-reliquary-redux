## Context

Relic cards are rendered via a `DataTemplate` in `RelicsMissionsView.axaml` whose `Border.Width` is bound to `{DynamicResource RelicCardWidth}`. That resource is declared in `App.axaml` with a default of `300` and is currently the only thing that ever changes it: `SettingsViewModel.ApplyFontSize` recomputes `RelicCardWidth` (and `MissionCardWidth`) as `300 * (fontPt / mediumPt)` every time the font size changes — `300` here is the effective width at Medium (14pt), scaled proportionally for other font sizes. There is no independent, user-facing, persisted control for that `300` base today.

The Settings tab (Settings → Appearance) already has an established pattern for this kind of "applies live, auto-saves, excluded from the unsaved-changes guard" setting — font size and Relics/Missions layout orientation both follow it: a `SettingsViewModel` property whose setter clamps the value, updates a live Avalonia resource/state, and fires an async `Save<X>Now()` that loads-then-`with`-patches the persisted `UserSettings` record.

The side-by-side layout (`relics-missions-layout-orientation`) hardcodes the Relics column's `MinWidth="316"` with a comment explicitly tying it to `RelicCardWidth (300) + 2 × card margin (8)`. That capability's spec requires this minimum to stay declarative (no runtime measurement of card sizes) — once card width is configurable, the literal `316` must be replaced by something still declarative but derived from the configured width.

## Goals / Non-Goals

**Goals:**
- Let the user set the relic card's base width (its width at Medium font size) from Settings → Appearance, applied live and auto-saved, matching the font-size/orientation UX exactly.
- Preserve today's font-scaling behavior: relic cards must still grow/shrink proportionally as font size changes, exactly as they do today off the `300` literal — just off the user's configured base instead.
- Persist the base value across restarts with a default equal to today's fixed `300`.
- Keep the side-by-side Relics column's minimum width correct for whatever the current effective (font-scaled) card width is, without introducing runtime measurement.

- Give both the relic card width and font size numeric inputs the same `NumericUpDown` spinner, explicitly positioned on the right, so both controls look consistent.

**Non-Goals:**
- Mission card width is not made configurable in this change. `MissionCardWidth` keeps using the fixed `300` base and its existing font-size-ratio behavior, unchanged.
- No change to the stacked layout's `WrapPanel`-based card wrapping — it already reflows correctly for any card width with no min-width logic to update.
- No change to font-size behavior itself (its point range, presets, resource key, or its proportional-scaling relationship with card width) beyond how its value is adjusted via the input control.

## Decisions

**`RelicCardWidth` stays computed by `ApplyFontSize`'s ratio formula; only its base changes.** `SettingsViewModel.ApplyFontSize` keeps `resources["RelicCardWidth"] = base * (pt / mediumPt)`, but `base` becomes the configured `RelicCardWidthPx` instead of the literal `300`. This preserves font-scaling consistency — a user who bumps font size still gets proportionally larger cards, now scaled from their chosen base rather than a fixed one. Alternative considered: treat the new setting as an absolute, un-scaled pixel width (fully decoupled from font size). Rejected per explicit correction — decoupling breaks the existing invariant that card width tracks font size, which the user relies on for readability at different text sizes; the setting is the *base*, not an override.

**New property `RelicCardWidthPx` (non-nullable `double`) on `UserSettings`, default `300.0`.** Mirrors `FontSizePt`'s pattern (always-has-a-value, defaulted in `UserSettings.Default`) rather than `RelicsPanelHeight`'s nullable "unset falls back to a live/computed default" pattern — there's no live/computed source of truth to fall back to here (unlike panel height, which can be derived from the splitter position), so nullability would add no value.

**Clamped range 180–600px, increment 10.** These bounds apply to the *base* (Medium-font) width, not the rendered width, which will still scale up or down from there with font size. 180px is the smallest base that keeps card contents (icon, name, chance %) legible at Medium font per the existing card template; 600px comfortably fits ultrawide setups without a single card dominating a row. Clamping happens in the `SettingsViewModel` property setter via `Math.Clamp`, identical to `EffectiveFontSizePt`'s `Math.Clamp(value, 8.0, 32.0)`.

**`SettingsViewModel` mirrors the font-size auto-save shape exactly:** a `RelicCardWidthPx` property whose setter clamps, re-invokes `ApplyFontSize(fontSizePt)` (so `RelicCardWidth` is recomputed from the new base at the *current* font ratio, not just set to the raw base value), and fires `_ = SaveRelicCardWidthNow()` (a `LoadAsync` → `with { RelicCardWidthPx = ... }` → `SaveAsync`, matching `SaveFontSizeNow`/`SaveOrientationNow`). It is excluded from `HasUnsavedChanges` for the same reason font size and orientation are. `ApplyFontSize` itself is updated to read the configured base (from the loaded `UserSettings`, or a field the ViewModel keeps in sync) instead of the literal `300`.

**Both numeric inputs use Avalonia's built-in `NumericUpDown` spinner, with `ButtonSpinnerLocation="Right"` set explicitly on both.** An earlier iteration of this design replaced the spinner with a custom pair of flanking `RepeatButton`s to get independent left/right placement of increase and decrease. That was reverted: the built-in spinner (a single up/down control) is simpler, requires no extra ViewModel commands, and needs no styling to match — `ButtonSpinnerLocation="Right"` (Avalonia's own property) is enough to guarantee consistent, predictable positioning across both controls. `Width="120"` is kept the same on both so they align visually under the "Font size" and "Relic card width" labels.

**Side-by-side minimum width becomes a resource-derived value, not a runtime measurement, and tracks both inputs.** Instead of `RelicsMissionsView.axaml`'s Relics column hardcoding `MinWidth="316"`, the code-behind sets `ColumnDefinitions[0].MinWidth` from the current `RelicCardWidth` resource value (which already reflects both the configured base *and* the current font size) plus card margin (16), recomputed at view construction and whenever either the relic card width base or the font size changes. This stays declarative in spirit — it's a fixed arithmetic derivation from resource state, not a measured/rendered element size — while keeping the splitter constraint correct regardless of which of the two settings moved. The Missions column's `MinWidth` stays the literal `316`, since `MissionCardWidth` remains anchored to the fixed `300` base (though it still scales with font size) and is out of scope.

## Risks / Trade-offs

- **[Risk]** A very wide configured base (e.g. 600px) combined with a large font size and a narrow window in side-by-side layout could push the Relics column's new minimum past what the window can comfortably show, forcing horizontal scrolling/clipping. → **Mitigation**: this is the same class of trade-off the existing splitter minimum already accepts at 300px scaled by font size; the 600px base ceiling was chosen to keep this a bounded, unsurprising expansion rather than removing the cap.
- **[Risk]** Two independent settings (font size, card width base) both feed into the same `RelicCardWidth` resource via multiplication, which could confuse a user who doesn't realize font size still affects card width after setting a base. → **Mitigation**: the Settings UI groups both controls together under Appearance, and the "base at Medium font" framing is documented in the new capability's spec so the relationship is discoverable, not silent.
- **[Trade-off]** Reusing the existing `RelicCardWidth` `DynamicResource` (rather than introducing a new resource key) keeps `RelicsMissionsView.axaml`'s card template untouched, at the cost of `ApplyFontSize` needing access to the configured base (a small piece of ViewModel state) rather than a hardcoded literal — a minor increase in coupling in exchange for not touching the card template or side-by-side XAML structure.
