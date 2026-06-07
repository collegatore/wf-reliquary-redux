## Why

Relic cards currently signal reward tier with an italic `"Common:"` / `"Uncommon:"` / `"Rare:"` header and signal wishlist status with **bold** text. Both signals are weak: italic reads as low emphasis, bold is easy to miss, and neither communicates the in-game Bronze/Silver/Gold rarity language.

The card's real job is **fast visual matching under time pressure**: on Warframe's relic reward screen a player has ~5–10 seconds to find a reward in the app and decide. That reframes the design constraints:

- Reward names must render **verbatim** (matching the in-game reward screen exactly) — no abbreviation, no stripping of "Prime"/"Blueprint", no truncation/ellipsis. Any divergence forces the player to mentally translate under time pressure.
- Long names (e.g. `Revenant Prime Neuroptics Blueprint`, 35 chars) therefore **will** wrap; the layout must make wraps look deliberate rather than broken.
- The card width is currently a hardcoded `200px` while the font size is user-selectable (Small 11 / Medium 14 / Large 18). Only one of the two scales, so Large turns 2-line names into 3-line names and matching speed degrades.

## What Changes

- **Tier identity moves to a colored accent bar.** Each tier group gains a vertical accent bar in the card's left gutter, colored per tier: Common = bronze, Uncommon = **slate** (a metallic-silver substitute, since literal silver is invisible on white), Rare = gold. The bar spans the whole group including wrapped lines, and also conveys grouping — so the per-group `Separator` lines are removed (one separator under the card title is kept).
- **Tier label becomes a uniform header word.** `"Common"` / `"Uncommon"` / `"Rare"` in title case, no trailing colon, rendered in the default text color (black) in **semi-bold** weight, uniform across all three tiers. No italic, no uppercase. Tier *identity* is carried by the bar; the label's weight (plus its position above the indented rewards) distinguishes "header" from "reward". This mirrors Warframe's own grammar (a uniform tier word + a per-tier-colored shape).
- **Wishlist highlight switches from bold to dimming.** When Show all rewards is enabled, wishlisted rewards render at full strength and non-wishlisted rewards are dimmed (~50% opacity), so the wanted reward is the only crisp text on the card. Bold is no longer used. When Show all rewards is disabled (only wishlisted shown) all render at full strength.
- **Reward names are indented beneath the tier label and never truncated.** Names are indented under the label so each tier group reads as "header + its list"; wrapped continuation lines align to the indented edge. Verbatim text, wrapping allowed, no ellipsis. (The relic card is wide enough — see below — that the longest names fit on one line, so wrapping is now rare.)
- **Card width scales with the content font size** so the per-line character budget stays roughly constant across Small/Medium/Large, keeping wrapping behavior predictable. Each card's width is treated as its Medium anchor and scaled by the font ratio (`ContentFontSize / Medium`); the relic-card anchor is **300px**, sized so the longest reward name fits on a single line at the Medium font.
- **The farming-mission cards also scale their width with the font size**, using the same font-ratio rule (mission cards anchor at their current `300px`). The font setting today scales the text inside both card types but not the cards themselves; this fixes that asymmetry for both.
- **Tier/label colors are defined as named brush resources** so a future dark theme is a palette swap rather than a markup change.
- **A persistent design document is added** (`docs/card-display-design.md`) capturing the rationale behind the card display — the ~5–10s matching use case, the two-channel (bar = tier; label weight + indentation = header vs reward) encoding, the light-theme palette and the silver→slate substitution, the dim-not-bold highlight, and the font-ratio width scaling. The change's own `design.md` is change-scoped and gets archived; this doc is the lasting reference, in the style of `docs/technical-reference.md`.

## Capabilities

### New Capabilities

<!-- None — modifies an existing capability -->

### Modified Capabilities

- `wishlist-relics-view`: relic cards convey tier via a per-tier accent bar and a uniform-colored tier label (replacing italic headers); wishlist status conveyed by dimming non-wishlisted rewards (replacing bold); per-group separators removed; reward names verbatim/unindented/untruncated; card width scales with the selected font size.
- `wishlist-farming-missions-view`: farming-mission card width scales with the selected font size (same font-ratio rule as relic cards).

## Impact

- **`App.axaml`**: new brush resources `TierLabelBrush` (black), `TierCommonBrush`, `TierUncommonBrush`, `TierRareBrush`; new `RelicCardWidth` (default `300`) and `MissionCardWidth` (default `300`) double resources.
- **`SettingsViewModel.cs`** (`ApplyFontSize`): compute `ratio = size / FontSizeMedium` and set both `RelicCardWidth = 300 * ratio` and `MissionCardWidth = 300 * ratio` alongside `ContentFontSize`, so both card widths track the font setting live.
- **`MainWindow.axaml`**: relic card `DataTemplate` restructured — each tier group becomes a `Border` with a left accent bar bound to the tier brush and small left padding; tier label `TextBlock` restyled (semi-bold, default color, no italic, no colon); rewards `ItemsControl` indented under the label; reward `TextBlock` uses opacity for dim state; per-group `Separator` removed; relic card `Width` bound to `{DynamicResource RelicCardWidth}` and mission card `Width` bound to `{DynamicResource MissionCardWidth}`. New `Window.Styles` entries for tier label (with `FontWeight=DemiBold`) and dimmed/tier-bar classes; the `TextBlock.wishlisted` bold style is removed.
- **`WishlistRelicsViewModel.cs`**: `WishlistRelicTierGroupViewModel` exposes tier-class flags (`IsCommon`/`IsUncommon`/`IsRare`) for bar coloring and drops the trailing `":"`; `WishlistRelicRewardViewModel` replaces `ShowWishlistedStyle` (bold) with `IsDimmed` (= ShowAllRewards && !IsWishlisted); `IsLast`/separator logic for groups removed.
- **`docs/card-display-design.md`** (new): persistent design-rationale document; linked from `docs/technical-reference.md`.

No schema, port, navigation, or data-layer changes. No new automated tests beyond updating existing ViewModel assertions; dark theme is explicitly out of scope.
