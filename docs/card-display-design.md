# Card Display Design

Design rationale for the relic and farming-mission cards shown on the **Relics and Missions** tab. This is an evergreen record of *why* the cards look and behave the way they do; for stack and architecture see [technical-reference.md](technical-reference.md).

## Governing use case: the ~5–10s reward match

The cards exist to support one time-pressured task: on Warframe's relic reward screen a player sees a reward and must find it in the app and decide within roughly 5–10 seconds. Every display decision below serves *fast visual matching* under that constraint, not density or decoration.

The dominant consequence: **reward names render verbatim**, character-for-character matching the in-game reward screen — no abbreviation, no stripping of "Prime"/"Blueprint", and no truncation/ellipsis. Any divergence forces the player to mentally translate text under time pressure, which is exactly what the card should remove. Because names are verbatim, long names (e.g. `Revenant Prime Neuroptics Blueprint`, ~35 chars) **will** wrap; the layout is designed to make wrapping look deliberate rather than broken.

## Two-channel encoding (relic cards)

A relic card answers three questions, each on its own non-overlapping visual register so none is ambiguous:

| Question | Channel |
|---|---|
| Which tier? | per-tier **accent bar** color (bronze / slate / gold) |
| Label vs reward? | **font weight + indentation** (tier label = semi-bold at the bar edge; rewards = normal weight, indented beneath) |
| Is it wishlisted? | **opacity** (wishlisted = full strength; others dimmed) |

Because the registers are *color-of-a-shape*, *text weight/position*, and *opacity*, they coexist without collision. This deliberately mirrors Warframe's own grammar — a uniform tier word plus a per-tier-colored icon — re-mapped from the game's dark theme to a light one.

### Tier identity in a bar, not in the text

Each tier group is wrapped in a `Border` with a colored left edge (`BorderThickness="3,0,0,0"`) that spans the whole group, including any wrapped reward lines. The bar does three jobs at once: it carries the tier color, it groups the rewards, and it replaces the per-group separator lines that used to divide tiers. Reward names are **indented** (~12px) beneath the tier label, so each group reads as "header + its list" and the semi-bold label sits proud of the rewards; wrapped continuation lines align to the indented edge. The single separator between the card title and body is retained.

### Why a uniform black, semi-bold label

The tier label (`Common` / `Uncommon` / `Rare`, title case, no colon) is uniform across all three tiers — it is a header cue, **not** a loot-rarity signal (rarity lives entirely in the bar). It is rendered **black and semi-bold**: against the normal-weight rewards (and the dimmed grey rewards) a black semi-bold header stays distinct by both weight and value. An earlier iteration used a uniform teal accent; black + semi-bold tested cleaner on the white card and carries the header/reward distinction by weight rather than spending a hue. Coloring the label *per tier* was rejected outright — metallic colors fail as text on white (see palette), and a per-tier label color would duplicate and potentially contradict the bar.

### Wishlist highlight = dim the noise, not bold the signal

When **Show all rewards** is enabled, the wishlisted rewards are the point and the rest are context, so non-wishlisted rewards drop to ~50% opacity while wishlisted rewards stay at full strength — the wanted reward is the only crisp text on the card. This was chosen over bold because it adds nothing to the layout (no glyph, no width cost, no weight change), survives wrapping, and is theme-proof. When Show all rewards is disabled, only wishlisted rewards are shown, so all render at full strength and dimming is a no-op.

## Light-theme palette

Literal metallic colors fail on a white background — silver is essentially invisible and gold washes out as text — so colors are chosen by role, not by copying in-game hexes. **Silver is substituted with slate**, because a neutral grey reads as "disabled" rather than as a tier.

| Element | Light-theme color | Notes |
|---|---|---|
| Tier label (uniform) | black `#000000`, **semi-bold** | header cue; weight (not hue) separates it from the rewards |
| Bar — Common (Bronze) | `#C0772E` | warm bronze |
| Bar — Uncommon (Silver→Slate) | `#7C8A9A` | slate; pure grey would read as disabled |
| Bar — Rare (Gold) | `#D4A017` | darkened gold, legible as a shape |
| Reward (wishlisted) | default foreground | full opacity |
| Reward (not wishlisted) | default foreground | ~0.5 opacity (dim) |

The label is *uniform across tiers*; it is a header cue, **not** a loot-rarity signal (rarity lives entirely in the bar). It is black/semi-bold rather than a colored accent — against the normal-weight rewards and the dimmed grey rewards it stays distinct by weight and value, with no collision against the dimmed grey.

## Card width scales with font size

The app's font size is user-selectable (Small 11 / Medium 14 / Large 18) and scales all text. Historically the card widths were fixed literals, so only one of the two variables scaled — at Large, the same name wrapped onto more lines and matching degraded. Both card types now scale width with the font, treating each card's **Medium width as its anchor** and scaling by the font ratio:

```
ratio            = ContentFontSize / FontSizeMedium      // 11/14, 14/14, 18/14
RelicCardWidth   = 300 * ratio   → Small ~236 / Medium 300 / Large ~386
MissionCardWidth = 300 * ratio   → Small ~236 / Medium 300 / Large ~386
```

The relic-card Medium anchor is **300px**, sized so the longest reward name found fits on a single line at the Medium font; the mission card keeps its existing 300px. One formula covers both card types. Scaling from the Medium anchor by the font ratio keeps the per-line character budget roughly constant across sizes, so a name that fits on one line at Medium also fits at Small/Large.

The relic anchor was originally 200px (the old fixed width), with reward names left un-indented to protect the narrow text column. Widening to 300px so the longest name fits on one line removed the wrapping pressure — which is why reward **indentation was re-introduced** (see above): once names no longer wrap, an indent buys readability without risking an extra line. The mission card scales for **consistency**, since its inner text already scales and the card should grow with it rather than getting cramped.

Implementation: `RelicCardWidth` and `MissionCardWidth` are `x:Double` resources in `App.axaml`; `SettingsViewModel.ApplyFontSize` recomputes both alongside `ContentFontSize`, and the cards bind `Width` to them via `DynamicResource`, so they react live to a font change (including the settings live-preview and revert paths).

## Theming and future dark theme

A dark theme is **not** implemented, but the structure is chosen so it becomes a palette swap rather than a markup change:

- The four tier/label colors are named `SolidColorBrush` resources in `App.axaml` (`TierLabelBrush` is black for the light theme); reward text otherwise uses the theme's default foreground, never a literal.
- A future dark theme should add a `ThemeDictionaries` entry with theme-appropriate values (e.g. `TierLabelBrush` → near-white, bars slightly lifted) and change no markup. The label's semi-bold weight carries over unchanged.
- The accent **bar** (rather than a tinted pill background) was chosen partly for this reason: bars, dimming, and opacity carry over to a dark background unchanged, whereas tinted backgrounds would need a separate dark palette.

If any of these decisions change — most likely when a dark theme lands — update this document alongside the code.
