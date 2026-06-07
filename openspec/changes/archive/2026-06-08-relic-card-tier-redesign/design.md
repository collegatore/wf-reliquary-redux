## Context

The relic card renders inside a `WrapPanel` of fixed-width cards on the "Relics and Missions" tab. Today each card shows the relic name, a separator, then per-tier groups (`"Common:"` italic header → reward `TextBlock`s → separator). Tier is signalled by italics; wishlist status by bold (`TextBlock.wishlisted`). The card `Width` is the literal `200`. The app's font size is a global setting that swaps the `ContentFontSize` resource (Small 11 / Medium 14 / Large 18) via `SettingsViewModel.ApplyFontSize`.

The governing use case is matching a reward seen on Warframe's relic reward screen within ~5–10 seconds. That makes verbatim, untruncated reward text non-negotiable, which in turn makes wrapping of long names (up to ~35 chars) a permanent reality rather than a bug to design away.

## Goals / Non-Goals

**Goals:**
- Replace the italic tier header with a per-tier accent **bar** (tier identity) plus a uniform **semi-bold** tier **label** (header vs reward), in normal case, no colon.
- Substitute **slate** for "silver" (Uncommon), because literal metallic silver/gold are illegible as colored text on white.
- Replace bold wishlist highlighting with **dimming of non-wishlisted** rewards.
- Keep reward names **verbatim, untruncated**, indented beneath their tier label; make wrapped names read as single items.
- **Drop per-group separators** (the bar conveys grouping); keep the single separator under the card title.
- **Scale both relic and mission card widths with the font size** so the character budget per line is roughly constant across Small/Medium/Large.
- Define tier/label colors as **named brush resources** to make a future dark theme a palette swap.
- Author a **persistent design-rationale document** (`docs/card-display-design.md`) so the reasoning outlives the change archive.

**Non-Goals:**
- Building a dark theme (only structuring colors so it is cheap later).
- Per-tier *text* coloring of labels or rewards (reopens the metallics-on-white problem).
- Abbreviating, stripping, or truncating reward names.
- Schema, port, navigation, or data-layer changes.

## Decisions

### Two independent visual channels

The card encodes two orthogonal facts with two non-overlapping registers, so neither is ambiguous:

| Question | Channel |
|---|---|
| Which tier? | accent **bar color** (bronze / slate / gold) |
| Label vs reward? | **font weight + indentation** (label = semi-bold, at the bar edge; rewards = normal weight, indented beneath) |
| Is it wishlisted? | **opacity** (wishlisted = full; others = ~50%) |

Because the registers are color-of-a-shape, text weight/position, and opacity, they coexist without collision. This is deliberately Warframe's own grammar (a uniform tier word + a per-tier-colored icon), re-mapped from dark to light.

> Note: an earlier iteration colored the label with a uniform teal accent. It was changed to **black + semi-bold** after testing — black reads as a cleaner header on the white card, and semi-bold (against the normal-weight, indented rewards) carries the header/reward distinction without spending a hue. Tier colour stays entirely in the bar.

### Light-theme palette

Literal metallics fail on white (silver ≈ invisible, gold ≈ washed out as text), so colors are chosen by role, not by copying game hexes:

| Element | Light-theme brush | Notes |
|---|---|---|
| Tier label (uniform) | black `#000000`, **semi-bold** | reads as a header on the white card; weight (not hue) separates it from the rewards |
| Bar — Common (Bronze) | `#C0772E` | warm bronze |
| Bar — Uncommon (Silver→Slate) | `#7C8A9A` | slate; pure grey would read as "disabled" |
| Bar — Rare (Gold) | `#D4A017` | darkened gold, legible as a shape |
| Reward (wishlisted) | default foreground | full opacity |
| Reward (not wishlisted) | default foreground | ~0.5 opacity (dim) |

The label is **uniform across tiers** — it is a header cue, *not* a loot-rarity signal (rarity lives entirely in the bar). It is black/semi-bold rather than a coloured accent; against the normal-weight rewards (and the dimmed rewards, which are grey), a black semi-bold header stays distinct by both weight and value, with no collision against the dimmed grey.

### Accent bar = grouping + tier, so separators go

Each tier group is a `Border` with `BorderThickness="3,0,0,0"`, `BorderBrush` = the tier brush, and a small left `Padding` (~6px). The label and all reward lines live inside, so the bar spans the full group height including wrapped lines — a wrapped name reads as one item. This single device replaces both the italic header's tier role and the per-group `Separator`, reducing vertical and horizontal noise. The separator directly under the card title is retained as the title/body divider.

Reward names are **indented** (~12px) beneath the tier label, so each group reads as "header + its list" and the semi-bold label sits proud of the rewards. The indent is affordable because the relic card is now wide enough (see below) that the longest names fit on one line — so the indent buys readability without forcing extra wraps. Wrapped continuation lines align to the indented edge.

### Wishlist highlight = dim the noise

When Show all rewards is enabled, the wishlisted rewards are the point and the rest are context, so non-wishlisted rewards drop to ~50% opacity while wishlisted stay full strength — the wanted reward is the only crisp text. This adds nothing to the layout (no glyph, no width cost, no bold) and is theme-proof. When Show all rewards is disabled, only wishlisted rewards render, so all are full strength (dimming is a no-op).

`WishlistRelicRewardViewModel` replaces `ShowWishlistedStyle` (bold trigger) with `IsDimmed = showAllRewards && !isWishlisted`. The AXAML binds reward `Opacity` (or a `Classes.dimmed` style) to it. The `TextBlock.wishlisted` bold style is deleted.

### Card width scales with font (both card types)

The wrapping problem is that font scales and width does not. Fix the asymmetry by treating each card's **Medium width as its anchor** and scaling it by the font ratio. Add `RelicCardWidth` (300) and `MissionCardWidth` (300) double resources and set them in `ApplyFontSize` alongside `ContentFontSize`:

```
ratio          = ContentFontSize / FontSizeMedium    // 11/14, 14/14, 18/14
RelicCardWidth   = 300 * ratio   → Small ~236 / Medium 300 / Large ~386
MissionCardWidth = 300 * ratio   → Small ~236 / Medium 300 / Large ~386
```

The relic-card Medium anchor is **300px**, sized so the longest reward name found fits on a single line at the Medium font; the mission card keeps its existing 300px. The ratio model scales each card from its Medium anchor by the font ratio, keeping the per-line character budget roughly constant across sizes — a name that fits on one line at Medium also fits at Small/Large. One formula covers both card types. Each card binds `Width="{DynamicResource RelicCardWidth}"` / `{DynamicResource MissionCardWidth}`, so both react live to font changes exactly like `ContentFontSize` does.

The relic anchor was originally 200 (the old fixed width), with reward names left un-indented to protect the narrow text column. Widening to 300 so the longest name fits on one line removed the wrapping pressure — which in turn made it worth **re-introducing reward indentation** (above) for readability, since the cost of an indent no longer risks an extra wrap. The mission card scales for **consistency** (its inner text already scales with the font, so the card should grow with it). The base widths remain a tuning knob, confirmed against real content during verification.

### Tier-class binding for bar color

The bar color is selected per tier without coupling the ViewModel to brushes. `WishlistRelicTierGroupViewModel` exposes `IsCommon` / `IsUncommon` / `IsRare` bools (derived from the tier name). The group `Border` binds `Classes.tier-common` / `Classes.tier-uncommon` / `Classes.tier-rare` to those, and `Window.Styles` maps each class to the matching `BorderBrush` `{DynamicResource}`. This keeps colors in resources (themeable) and selection declarative.

### Colors as named resources (dark-theme prep, not dark theme)

All four colors are `SolidColorBrush` resources in `App.axaml` (`TierLabelBrush` is black for the light theme), and reward text uses the theme's default foreground (`{DynamicResource}`), never a literal. A future dark theme becomes: add a `ThemeDictionaries` entry with theme-appropriate values (e.g. `TierLabelBrush` → near-white, bars slightly lifted). No markup changes. The label's semi-bold weight, the bars, and the dim/opacity all carry over unchanged — which is the structural reason the bar was chosen over a tinted pill (pill backgrounds are the one primitive that would need a separate dark palette).

### Persistent design document

The change's `design.md` is change-scoped and moves to the archive on completion, so it is not a discoverable home for the lasting "why." Add `docs/card-display-design.md` in the style of `docs/technical-reference.md`: a focused, evergreen record of the card-display rationale — the ~5–10s matching use case, the verbatim-name constraint, the two-channel encoding, the light-theme palette and silver→slate substitution, dim-not-bold highlighting, and the font-ratio width scaling for both card types. Link it from `docs/technical-reference.md` so it is reachable from the documentation root (and thus from README/CLAUDE.md). It documents the *decisions and considerations*, not API/markup detail, and should be updated if these choices later change (e.g. when a dark theme lands).

## Risks / Open Questions

- **Relic Medium anchor (300px)** — chosen so the longest reward name found fits on one line at Medium; if a longer name surfaces later it may wrap, at which point bump the anchor (the indented layout still tolerates the occasional wrap). Confirm against real content during verification.
- **WrapPanel density at Large** — wider cards mean fewer per row at Large; acceptable, since the largest font implies the user prioritizes legibility over density.
