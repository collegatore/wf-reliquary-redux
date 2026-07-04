# User Guide

A quick tour of the behaviors that aren't self-evident from the UI. For setup basics (getting drop tables into the app), see the [README](../README.md#initial-setup).

## First launch (SmartScreen)

The app is unsigned, so the first time you run the downloaded executable Windows SmartScreen will probably show **"Windows protected your PC"**. Click **More info → Run anyway** to start it. This is expected for an unsigned pre-release build; see [Download and Install](../README.md#download-and-install) for the full walkthrough and which artifact to pick.

## Download and import are separate steps

`File → Drop Tables` has two distinct actions:

- **Download from URL** (or **Read from file**) fetches the raw drop-table HTML and stores it. Nothing is parsed yet.
- **Import stored source** parses the stored HTML into the queryable database.

Neither triggers the other. When DE publishes a new drop-table version, do both: download, then import. This is intentional — the app never touches the network or changes your data without an explicit command. The footer shows the currently imported drop-tables version.

Downloaded sources accumulate in the local database as history; **Export sources** writes them back out as HTML files.

## Wishlist entries survive re-imports (ghost entries)

Wishlist items are stored by part name, not by reference to the imported tables. When you import a new drop-table version, your wishlists are untouched. If a wishlisted part no longer appears in the current tables (DE renamed it, or you imported an older table), it shows as a **ghost entry** — still on the wishlist, but with no matching relics or missions. It's not lost; it reappears in full once an import contains it again.

## Profiles and wishlists

**Profiles** (managed in the Settings tab) are one level above wishlists — one per game account, if you have several. Each profile has its own wishlists, its own owned-gear marks, and remembers its own last-active wishlist. Deleting a profile deletes its wishlists. Everything outside Settings (Prime Gear, Relics and Missions) always operates on the active profile silently.

Most players need exactly one profile and one wishlist; the defaults ("Default"/"Default") are created for you and the selectors stay out of the way.

## Prime Gear: owned, vaulted, and group operations

- **Vaulted** gear is collected in its own section (hidden by default — toggle **Show vaulted** to expand it); vault status comes from the imported tables.
- Marking gear **Owned** moves it to a separate group; if the item still has wishlisted parts, the app asks whether to keep tracking them.
- Cards can be **selected** (click to toggle) for group operations: select all visible, mark selected owned (one confirmation for the whole batch), or unmark selected.
- The search box filters as you type (more than 2 symbols)

## Relic view modes

The Relics and Missions tab renders relics in one of three modes (switchable in the Appearance window):

- **Verbatim** — full card, rewards grouped under tier label rows, names exactly as DE prints them. For when you're matching against the in-game reward screen.
- **Compact** (default) — one line per reward, led by a bold tier letter (**C**/**U**/**R**). Names are clipped for density: trailing "Relic" is dropped from relic names, "Prime" is dropped from reward names, and a trailing "Blueprint" is dropped *unless* it's a main blueprint (`Revenant Prime Blueprint` → `Revenant Blueprint`, but `Revenant Prime Neuroptics Blueprint` → `Revenant Neuroptics`). Display-only — the underlying data is untouched.
- **Table** — one expandable row per relic, showing only wishlisted rewards when expanded. Each row has ⊞ (full reward card in a popup) and 💬 (Recruiting Chat message composer).

## Recruiting Chat message

Each relic card's `⋮` menu (or 💬 in Table mode) opens a message composer that builds recruiting-chat text like `LF [Axi A1 Relic] rad` from a template. The template lives in the Settings tab:

- The **pattern** is free text with `{placeholder}` slots. `{relic_name}` is built in and required — it resolves to the full relic name.
- Every other placeholder defines a **custom option group**: you list the choices (descriptor + inserted value) and a default. The composer shows a selector per group with a live preview.
- The default template is `{participation} [{relic_name}] {refinement}` with Hosting/Looking-for-group and the four refinement levels.

## Appearance window

`File → Appearance` opens a small always-on-top window (not a tab — you can watch changes apply live):

- **Font size** — Small / Medium / Large; scales the whole content area, including card widths.
- **Relic card width** — base width in pixels; the effective width also scales with font size.
- **Relic view mode** — Verbatim / Compact / Table (see above).
- **Layout orientation** — Relics and Missions side-by-side (default) or stacked vertically. Both layouts have a draggable splitter, and its position is remembered separately per orientation.

All appearance settings apply immediately and save automatically.

## Where your data lives

Everything is under `%LOCALAPPDATA%\WF Reliquary Redux\`:

| What | Where |
|---|---|
| Settings (window, appearance, source URL, recruiting template) | `user-settings.json` |
| Database (imported tables, wishlists, profiles, source history) | `DropTables\drop-table-import.sqlite` |
| Log file | `logs\app.log` |

The log records errors (and the app version at startup) — attach it to bug reports. **Settings → Maintenance → Open log file** gets you there; error toasts also offer "Copy log path". Deleting the whole directory resets the app to a fresh state.
