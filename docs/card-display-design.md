# Card Display Design

Design rationale for the relic and farming-mission cards shown on the **Relics and Missions** tab. This is an evergreen record of *why* the cards look and behave the way they do; for stack and architecture see [technical-reference.md](technical-reference.md).

## Governing use case: planning and scope overview

The Relics and Missions tab's job is **planning and scope overview** — seeing at a glance which relics are still worth farming, which missions drop them, and how big the remaining list is. Density and a noise-free layout matter more here than verbatim fidelity to the in-game reward text.

This wasn't always the governing use case. The tab's card layout was originally designed around a ~5–10s **reward match**: on Warframe's relic-reward screen a player sees a reward and must find it in the app and decide within roughly 5–10 seconds, which drove verbatim reward names, wide cards, and per-tier label rows (see [Verbatim mode](#verbatim-mode-preserved-for-comparison) below). Field use showed that use case doesn't actually happen on this tab — while cracking relics, the player has no way to know in advance which relics were cracked, so reward matching happens on the **wishlist screen** instead, not here. That correction is why this tab now has three view modes: two (Compact, Table) optimized for planning/overview, and Verbatim preserved as-is for side-by-side comparison against the redesign.

## Three relic view modes

The relics panel supports three user-selectable view modes, chosen in the **Appearance window** (File → Appearance) alongside the layout orientation control and persisted across sessions (`RelicViewMode` in `UserSettings`). The modeless Appearance window's live preview lets modes be compared side by side against the main window. Verbatim was the initial default while the modes were being compared; after field comparison, **Compact is the default** — it matches the governing planning/overview use case while staying card-shaped.

| Mode | What it optimizes for | Card/row shape |
|---|---|---|
| **Verbatim** | Exact reward-text matching (historical use case) | Tier-grouped cards, verbatim names, tier label rows |
| **Compact** (default) | Density, still card-shaped | One line per reward, clipped wording, bold leading tier letter |
| **Table** | Maximum density, scope overview | One row per relic, expandable into reward rows |

### Verbatim mode (preserved for comparison)

Verbatim mode is the original card design, unchanged, kept so it can be compared side by side against Compact and Table. Its rationale is now **historical** rather than governing: it existed to answer the ~5–10s reward match (see above), which is why reward names render exactly as they appear in-game — no abbreviation, no stripping of "Prime"/"Blueprint", no truncation/ellipsis — and why long names (e.g. `Revenant Prime Neuroptics Blueprint`, ~35 chars) are allowed to wrap rather than being clipped. Everything below through "Card width scales with font size" describes Verbatim mode specifically.

#### Two-channel encoding (Verbatim relic cards)

A Verbatim relic card answers three questions, each on its own non-overlapping visual register so none is ambiguous:

| Question | Channel |
|---|---|
| Which tier? | per-tier **accent bar** color (bronze / slate / gold) |
| Label vs reward? | **font weight + indentation** (tier label = semi-bold at the bar edge; rewards = normal weight, indented beneath) |
| Is it wishlisted? | **opacity** (wishlisted = full strength; others dimmed) |

Because the registers are *color-of-a-shape*, *text weight/position*, and *opacity*, they coexist without collision. This deliberately mirrors Warframe's own grammar — a uniform tier word plus a per-tier-colored icon — re-mapped from the game's dark theme to a light one.

##### Tier identity in a bar, not in the text

Each tier group is wrapped in a `Border` with a colored left edge (`BorderThickness="3,0,0,0"`) that spans the whole group, including any wrapped reward lines. The bar does three jobs at once: it carries the tier color, it groups the rewards, and it replaces the per-group separator lines that used to divide tiers. Reward names are **indented** (~12px) beneath the tier label, so each group reads as "header + its list" and the semi-bold label sits proud of the rewards; wrapped continuation lines align to the indented edge. The single separator between the card title and body is retained.

##### Why a uniform black, semi-bold label

The tier label (`Common` / `Uncommon` / `Rare`, title case, no colon) is uniform across all three tiers — it is a header cue, **not** a loot-rarity signal (rarity lives entirely in the bar). It is rendered **black and semi-bold**: against the normal-weight rewards (and the dimmed grey rewards) a black semi-bold header stays distinct by both weight and value. An earlier iteration used a uniform teal accent; black + semi-bold tested cleaner on the white card and carries the header/reward distinction by weight rather than spending a hue. Coloring the label *per tier* was rejected outright — metallic colors fail as text on white (see palette), and a per-tier label color would duplicate and potentially contradict the bar.

##### Wishlist highlight = dim the noise, not bold the signal

When **Show all rewards** is enabled, the wishlisted rewards are the point and the rest are context, so non-wishlisted rewards drop to ~50% opacity while wishlisted rewards stay at full strength — the wanted reward is the only crisp text on the card. This was chosen over bold because it adds nothing to the layout (no glyph, no width cost, no weight change), survives wrapping, and is theme-proof. When Show all rewards is disabled, only wishlisted rewards are shown, so all render at full strength and dimming is a no-op. Show all rewards applies in Verbatim and Compact modes; it's hidden in Table mode (see below).

#### Light-theme palette

Literal metallic colors fail on a white background — silver is essentially invisible and gold washes out as text — so colors are chosen by role, not by copying in-game hexes. **Silver is substituted with slate**, because a neutral grey reads as "disabled" rather than as a tier. This palette is used by Verbatim's tier bars (Compact and Table carry tier identity in letter markers instead — see below).

| Element | Light-theme color | Notes |
|---|---|---|
| Tier label (uniform, Verbatim only) | black `#000000`, **semi-bold** | header cue; weight (not hue) separates it from the rewards |
| Bar — Common (Bronze) | `#C0772E` | warm bronze |
| Bar — Uncommon (Silver→Slate) | `#7C8A9A` | slate; pure grey would read as disabled |
| Bar — Rare (Gold) | `#D4A017` | darkened gold, legible as a shape |
| Reward (wishlisted) | default foreground | full opacity |
| Reward (not wishlisted) | default foreground | ~0.5 opacity (dim) |

#### Card width scales with font size

The app's font size is user-selectable (Small 11 / Medium 14 / Large 18) and scales all text. Historically the card widths were fixed literals, so only one of the two variables scaled — at Large, the same name wrapped onto more lines and matching degraded. Both card types now scale width with the font, treating each card's **Medium width as its anchor** and scaling by the font ratio:

```
ratio            = ContentFontSize / FontSizeMedium      // 11/14, 14/14, 18/14
RelicCardWidth   = 300 * ratio   → Small ~236 / Medium 300 / Large ~386
MissionCardWidth = 300 * ratio   → Small ~236 / Medium 300 / Large ~386
```

The relic-card Medium anchor is **300px**, sized so the longest reward name found fits on a single line at the Medium font; the mission card keeps its existing 300px. One formula covers both card types (and Compact mode, which reuses `RelicCardWidth`). Scaling from the Medium anchor by the font ratio keeps the per-line character budget roughly constant across sizes, so a name that fits on one line at Medium also fits at Small/Large.

The relic anchor was originally 200px (the old fixed width), with reward names left un-indented to protect the narrow text column. Widening to 300px so the longest name fits on one line removed the wrapping pressure — which is why reward **indentation was re-introduced** (see above): once names no longer wrap, an indent buys readability without risking an extra line. The mission card scales for **consistency**, since its inner text already scales and the card should grow with it rather than getting cramped.

Implementation: `RelicCardWidth` and `MissionCardWidth` are `x:Double` resources in `App.axaml`; `AppearanceViewModel.ApplyFontSize` recomputes both alongside `ContentFontSize`, and the cards bind `Width` to them via `DynamicResource`, so they react live to a font change (including the Appearance window's live-preview path).

### Compact mode

Compact mode keeps the Verbatim card's border, header, width scaling (`RelicCardWidth`), and vaulted styling, but drops the tier label rows entirely: the body is a flat list of reward lines, tier-ordered (Common → Uncommon → Rare), each leading with a **bold** letter marker — `C` / `U` / `R` — in place of the label row (e.g. **C** Forma). This halves the vertical footprint per tier group at the cost of the "header + its list" reading — an accepted tradeoff since planning/overview favors seeing more relics over reading each one slowly.

Two rejected embellishments, for the record: the markers were originally parenthesized — `(C)` — but bare bold letters read cleaner and cost two characters less per line; and each line originally carried a per-tier accent bar in its left gutter (the Verbatim palette), but per-line bars added color noise without the grouping job they do in Verbatim (where one bar spans a whole tier group), so they were removed and the freed gutter reclaimed. Tier identity in Compact rests on the letter alone.

Word clipping (Compact and Table) is a **presentation-only** transform, applied to display strings and never to stored/domain names:
- Relic names drop the trailing word "Relic" (`Axi A15 Relic` → `Axi A15`).
- Reward names drop the word "Prime" (`Revenant Prime Neuroptics Blueprint` → `Revenant Neuroptics Blueprint`).
- Reward names drop a trailing "Blueprint" **unless** the original name ends with "Prime Blueprint" — a main blueprint keeps the word so it stays distinguishable from its component blueprints (`Revenant Prime Blueprint` → `Revenant Blueprint`; `Revenant Prime Neuroptics Blueprint` → `Revenant Neuroptics`; `Forma Blueprint` → `Forma`, since it has no "Prime" to protect the trailing "Blueprint").

Implementation: `CompactWording` (`src/Presentation/WF.Reliquary.Presentation/CompactWording.cs`) is the pure string transform, unit-tested against the scenarios above; `WishlistRelicCardViewModel`/`WishlistRelicRewardViewModel` expose the clipped names and tier letter as computed properties alongside the verbatim ones, so Verbatim and Compact read from the same underlying data.

### Table mode

Table mode is the density answer for scope overview: one row per relic (clipped name, wishlisted-tier markers like `C, R` for tiers containing at least one wishlisted reward — with a count prefix when a tier has more than one, e.g. `C, 2 R` for one wishlisted common and two rares — era→alphabetical order, vaulted rows dimmed and hideable from the Filters menu), expandable into child reward rows — but **only for wishlisted rewards** (compact name led by a **bold** tier letter, matching Compact mode's marker style); non-wishlisted rewards are omitted from expansion entirely rather than shown dimmed, since Table mode's job is scope overview and a row of noise defeats that. Two pictogram actions per relic row — the rewards icon opens a modal with the relic's Verbatim-style card in all-rewards mode (non-wishlisted rewards dimmed exactly as on the main-panel Verbatim card, so the full reward context is still visible when wanted — closable only via the window's native close control), the chat icon opens the existing Recruiting Chat message composer. The Display menu's **Show all rewards** entry is greyed out while Table mode is active (it has no card to apply to) but its checked state is retained for when the user switches back to a card mode.

Table mode is built on Avalonia's `TreeDataGrid` control in hierarchical mode (`HierarchicalTreeDataGridSource<T>`, built in code rather than pure-XAML bindings — the source needs to define the expander column, the markers column, and two pictogram `TemplateColumn`s that render empty on child rows).

**Dependency note:** on the Avalonia 12 line (this app targets 12.0.5), the *official* `Avalonia.Controls.TreeDataGrid` package became a paid Avalonia Accelerate product — adding it fails the build with `AVLIC0001` (no license key), and there's no free official 12.x build. Table mode instead uses the free community fork `Caly.Avalonia.Controls.TreeDataGrid` (`12.0.1.3`, by BobLd), which keeps the upstream assembly name, API, and theme avares path unchanged, so nothing about the structure above differs from what the official package would have looked like. This is a deliberate cost/trust tradeoff — a third-party, less-vetted dependency in exchange for avoiding the commercial license — not an oversight; don't "fix" it back to the official package. If the fork is abandoned, the alternatives are a paid Avalonia license or re-architecting Table mode on free built-in controls (`TreeView` or nested `ItemsControl` + `Expander`).

## Mission cards

Mission cards on this tab are **out of scope** for the view-mode work above; they keep the single card design described here (no Compact/Table equivalents) and are unaffected by the relic view mode setting.

## Theming and future dark theme

A dark theme is **not** implemented, but the structure is chosen so it becomes a palette swap rather than a markup change:

- The four tier/label colors are named `SolidColorBrush` resources in `App.axaml` (`TierLabelBrush` is black for the light theme); reward text otherwise uses the theme's default foreground, never a literal. Compact and Table carry tier identity in text markers and encode state as opacity (dimming), which is theme-proof by construction.
- A future dark theme should add a `ThemeDictionaries` entry with theme-appropriate values (e.g. `TierLabelBrush` → near-white, bars slightly lifted) and change no markup. The label's semi-bold weight carries over unchanged.
- The accent **bar** (rather than a tinted pill background) was chosen partly for this reason: bars, dimming, and opacity carry over to a dark background unchanged, whereas tinted backgrounds would need a separate dark palette.

If any of these decisions change — most likely when a dark theme lands — update this document alongside the code.
