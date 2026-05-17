# Parser Contract: Warframe Drop Table Sample

## Purpose

Define the application-facing contract for turning the preserved Warframe drop
table HTML into normalized records. This is not a web API; it is the expected
boundary between infrastructure parsing and application import behavior.

## Input

`DropTableParseRequest`

| Field | Required | Description |
|-------|----------|-------------|
| `sourceUrl` | Yes | Original public drop table URL. |
| `rawArtifactPath` | Yes | Repo-relative or absolute path to the preserved HTML sample. |
| `retrievedAt` | Yes | Timestamp when the sample was retrieved. |

## Output

`DropTableParseResult`

| Field | Description |
|-------|-------------|
| `sourceSample` | Non-persistent source provenance, including detected source last-update date. |
| `rewardItems` | Distinct normalized reward items found in relevant sections. |
| `relics` | Base relics identified by era and code. |
| `relicRewards` | Base relic reward rows with derived reward tier. |
| `missionSources` | Mission reward table headers and parsed source fields. |
| `rewardRotations` | Rotation headings found under mission sources. |
| `missionRewards` | Mission reward probability rows. |
| `diagnostics` | Non-fatal parse warnings with source context. |

## Required Parsing Behavior

- Locate the Mission Rewards section by the `missionRewards` heading.
- Locate the Relics section by the `relicRewards` heading.
- Stop parsing each section at the next top-level section heading.
- Treat table header rows under `Relics` as relic headings that include a
  refinement label; use the `Intact` table to assign persistent reward tiers.
- Treat table header rows under `Missions` as mission source headings unless
  the header is a rotation heading.
- Treat `Rotation A`, `Rotation B`, and `Rotation C` as optional mission reward
  groupings.
- Parse mission reward rows into item name, rarity label, and chance percent.
- Parse relic reward rows into item name and reward tier. Derive reward tier
  from Intact relic chances: 25.33% is `Common`, 11.00% is `Uncommon`, and
  2.00% is `Rare`.
- Ignore Relics-section rarity labels for tier assignment because they are not
  reliable for this model.
- Preserve source order for every parsed mission reward row and relic reward
  row.
- Preserve duplicate reward names as separate rows.
- Preserve source order for every parsed mission source and use that source
  order, not the display label alone, when linking rotations and mission reward
  rows back to their parent source.
- Preserve duplicate mission source display labels as separate mission source
  records.

## Error Handling

Fatal parse failures:

- Raw artifact cannot be read.
- Mission Rewards section cannot be found.
- Relics section cannot be found.

Non-fatal diagnostics:

- A row has an unparseable chance label.
- A relic-like name cannot be split into era and code.
- An Intact relic reward chance does not match a known reward tier.
- A mission label cannot be split into region, node, or mission type.

Non-fatal diagnostics must include enough context to find the source heading and
row.

## Acceptance Examples

Relic reward heading:

```text
Axi A1 Relic (Intact)
```

Expected parse:

```text
Relic era: Axi
Relic code: A1
Refinement seen in source: Intact
Persistent refinement record: none
Persistent reward rows: linked directly to Axi A1 Relic
Tier derivation: 25.33 = Common, 11.00 = Uncommon, 2.00 = Rare
```

Mission reward row:

```text
Mercury/Apollodorus (Survival), Rotation B, Lith C14 Relic, Rare (7.69%)
```

Expected parse:

```text
Mission source: Mercury/Apollodorus (Survival)
Mission type: Survival
Rotation: B
Reward item: Lith C14 Relic
Recognized relic: Lith C14
Rarity label: Rare
Chance percent: 7.69
```
