# Changelog

All notable changes to WF Reliquary Redux are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Color palettes.** File → Appearance has a new Palette setting with two palettes: **Linen** (the default: warm linen bars, a fern-green hover, ink-blue selection) and **Stone** (stone bars, a sage hover, heather selection). The choice applies immediately and is remembered.

### Changed

- **Filters, sort, and display moved into bar buttons.** The Filters and Sorting rows are gone. Filters is a funnel menu of things to **hide** (Relics: Vaulted; Prime Gear: Vaulted, Owned, Not wishlisted); sort is a double-arrow menu button plus a direction button on the Relics bar; a new eye menu (Display) holds the relic view mode and Show all rewards. A button whose setting isn't the default is highlighted like a checked filter and describes the setting on hover.
- **New defaults: everything is visible, sorted Era descending.** Vaulted relics, vaulted gear and owned gear now show until hidden from Filters, and relics sort Axi first. Filters and Show all rewards are now remembered between sessions. An Era ascending sort you saved earlier is kept.
- **Relic view mode** moved from File → Appearance to the eye button on the Relics bar.
- **Unmark owned** in group operations is always shown next to Mark owned, inactive when no selected card is owned.

- **New default look.** The control bars, dropdowns, card menus, and the selection color (checked filters, selected items, cards selected for group operations) now use the Linen palette instead of grey and stock blue.
- **Dropdowns** across the app share one look: a light field with a green hover, and the selected item marked in the palette's selection color. The Wishlist and Profile selectors mark the active entry by color instead of a check mark.
- **Card menus** (the `⋮` on relic and gear cards) open without highlighting their first item; hovering or using the arrow keys highlights an item.
- **Inactive controls** keep their background and show muted text and outline.
- **Wishlist and Profile selectors** are now standard dropdowns that list only the wishlists or profiles, so they open aligned under the field and work with the arrow keys and Enter. Rename, Delete and New moved to a `⋮` menu right next to each selector, with icons; Delete turns red on hover. "+ New list" / "+ New profile" are now "New list" / "New profile".
- **Table mode** row actions use icons instead of the `⊞` and `💬` symbols, stay muted until hovered, and name themselves in a tooltip. The Recruiting Chat message window's Copy button uses a standard copy icon instead of `📋`.

## [1.0.0] — 2026-07-05

First stable release, out of preview.

> ### Platform scope
>
> **Windows x64 is the only supported platform in this release.** Linux support
> is planned but has not yet been built or tested.

## [1.0.0-alpha] — 2026-07-03

First public preview.

> ### Platform scope
>
> **Windows x64 is the only supported platform in this release.** Linux support
> is planned but has not yet been built or tested.

### Added

- Drop-table acquisition: download the official Warframe drop tables from DE
  (default source `https://www.warframe.com/droptables`), or read a previously
  exported HTML file; import parses the stored HTML into a local SQLite database.
  Download and import are separate, explicitly manual steps.
- Prime Gear tab: browse all prime gear from the imported tables, mark items as
  owned, and add parts to wishlists; vaulted and owned gear are visually separated.
- Relics and Missions tab: rundown of the relics that drop your wishlisted parts
  and the missions that drop those relics, with three relic view modes
  (Verbatim / Compact / Table) and stacked or side-by-side layout.
- Wishlists and player profiles: multiple wishlists scoped to profiles; wishlist
  entries survive drop-table re-imports.
- Recruiting Chat message constructor: per-relic template-based message composer
  with customizable placeholders and copy-to-clipboard.
- Appearance window: font size, relic card width, relic view mode, and
  Relics/Missions layout orientation.
- Window size and position persistence across launches.
- Error reporting: toast cards with log access; errors are appended to
  `%LOCALAPPDATA%\WF Reliquary Redux\logs\app.log`, each entry stamped with the
  app version.
- Version identity: the window title and startup log line show the app version.

[1.0.0]: https://github.com/collegatore/wf-reliquary-redux/releases/tag/v1.0.0
[1.0.0-alpha]: https://github.com/collegatore/wf-reliquary-redux/releases/tag/v1.0.0-alpha
