## 1. Color and width resources (App.axaml)

- [x] 1.1 Add `SolidColorBrush` resources: `TierLabelBrush` (`#000000`, black), `TierCommonBrush` (`#C0772E`), `TierUncommonBrush` (`#7C8A9A`), `TierRareBrush` (`#D4A017`)
- [x] 1.2 Add `RelicCardWidth` (default `300`) and `MissionCardWidth` (default `300`) `x:Double` resources, mirroring how `ContentFontSize` is declared
- [x] 1.3 Add a brief comment noting these are the light-theme values and the intended home for a future dark `ThemeDictionaries` swap

## 2. Card width tracks font size (SettingsViewModel.cs)

- [x] 2.1 In `ApplyFontSize(FontSizeOption)`, after setting `ContentFontSize`, compute `ratio = size / (double)resources["FontSizeMedium"]` and set `resources["RelicCardWidth"] = 300 * ratio` and `resources["MissionCardWidth"] = 300 * ratio`
- [x] 2.2 Confirm the live-preview and revert paths (settings preview / Back) recompute both widths too, since they route through `ApplyFontSize`

## 3. ViewModel changes (WishlistRelicsViewModel.cs)

- [x] 3.1 `WishlistRelicTierGroupViewModel`: stop appending `":"` to `TierName`; keep the title-case tier word
- [x] 3.2 `WishlistRelicTierGroupViewModel`: add `IsCommon` / `IsUncommon` / `IsRare` bools derived from the tier name (case-insensitive) for accent-bar class binding
- [x] 3.3 `WishlistRelicTierGroupViewModel`: remove `IsLast` and any trailing-separator logic (separators between groups are gone)
- [x] 3.4 `WishlistRelicRewardViewModel`: replace `ShowWishlistedStyle` with `IsDimmed = showAllRewards && !isWishlisted`
- [x] 3.5 `WishlistRelicCardViewModel`: drop the `groups[^1].IsLast = true` assignment

## 4. Window styles (MainWindow.axaml `Window.Styles`)

- [x] 4.1 Remove the `TextBlock.wishlisted` (bold) style
- [x] 4.2 Add `TextBlock.tier-label` style: `Foreground="{DynamicResource TierLabelBrush}"`, `FontWeight="DemiBold"`, non-italic, normal case
- [x] 4.3 Add `TextBlock.dimmed` style setting `Opacity` to ~`0.5` (or set reward `Opacity` via binding instead)
- [x] 4.4 Add `Border.tier-common` / `Border.tier-uncommon` / `Border.tier-rare` styles, each setting `BorderBrush` to the matching `TierCommonBrush` / `TierUncommonBrush` / `TierRareBrush`

## 5. Relic card template (MainWindow.axaml DataTemplate)

- [x] 5.1 Bind the card `Border` `Width` to `{DynamicResource RelicCardWidth}` (replaces the former literal width)
- [x] 5.2 Wrap each tier group in a `Border` with `BorderThickness="3,0,0,0"`, small left `Padding` (~`6,0,0,0`), a small bottom `Margin` for inter-group spacing, and `Classes.tier-common`/`Classes.tier-uncommon`/`Classes.tier-rare` bound to the VM bools
- [x] 5.3 Restyle the tier label `TextBlock`: `Classes="tier-label"`, remove `FontStyle="Italic"`, bind `Text="{Binding TierName}"` (now without colon)
- [x] 5.4 Reward `TextBlock`: keep `TextWrapping="Wrap"`, indent the rewards `ItemsControl` (~`12,0,0,0`) beneath the tier label, and apply dim via `Classes.dimmed="{Binding IsDimmed}"` (or `Opacity` binding)
- [x] 5.5 Remove the per-group `<Separator>` element from the tier-group template; keep the single separator under the card title
- [x] 5.6 Bind the mission card `Border` `Width` to `{DynamicResource MissionCardWidth}` (replace the literal `300`)

## 6. Design document (docs/)

- [x] 6.1 Create `docs/card-display-design.md` covering: the ~5–10s reward-matching use case; verbatim/untruncated name constraint; the two-channel encoding (bar = tier, text color = label vs reward, opacity = wishlist); the light-theme palette and silver→slate rationale; dim-not-bold highlighting; font-ratio width scaling for both card types; and the named-brush structuring for a future dark theme
- [x] 6.2 Add a link to `docs/card-display-design.md` from `docs/technical-reference.md`

## 7. Verification

- [x] 7.1 Build and run; open the "Relics and Missions" tab
- [x] 7.2 Confirm each tier group shows the correct bar color (bronze/slate/gold) and a teal, non-italic, colon-less label
- [x] 7.3 Toggle Show all rewards: wishlisted rewards stay crisp, non-wishlisted dim to ~50%, nothing is bold
- [x] 7.4 Confirm a long verbatim name (e.g. "Revenant Prime Neuroptics Blueprint") wraps without truncation, with continuation lines aligned to the left edge under the bar
- [x] 7.5 Cycle Small/Medium/Large font: both relic and mission cards resize live; the long relic name wraps to a consistent line count; adjust base widths if a size wraps to an extra line or leaves cards too empty
- [x] 7.6 Update any existing unit/integration assertions that referenced `ShowWishlistedStyle`, `IsLast`, or the `":"` suffix in `TierName`
