# WF Reliquary Redux

## Project Description

WF Reliquary Redux is a companion app for the game *Warframe*. Its long-term goal is to help players identify how to acquire the items they want by collecting publicly available drop table data, parsing it into structured information, and turning that information into actionable acquisition paths.

The project is intended to ingest official or otherwise publicly accessible drop table sources, extract mission and reward data, and organize that data in a way that supports item lookup, farming decisions, and player planning. Over time, it should connect items to the missions, rotations, and reward sources where they can be obtained, then use that relationship data to build acquisition paths for requested items or sets of items.

At the product vision level, the app may also support player-centered planning features such as wishlists, mission rundowns, and other tools that help prioritize efficient farming opportunities across overlapping rewards.

## Product Increment 1 Priority

The first implementation priority is focused solely on Void Relics and the Prime Parts obtained from them. This increment should help players identify which relics contain a desired Prime Part and which missions are the best sources for those relics based on publicly available drop tables.

This increment should ingest public drop table data, parse relic source information, and preserve mission rotation chances where applicable. It should model the acquisition chain from Prime Part to Relic, then from Relic to mission source, including rotation-specific drop chances for missions that distribute relics through A, B, or C reward rotations.

- Prioritize Void Relics and Prime Parts obtained from relic rewards.
- Use drop table data as the source of truth for relic acquisition.
- Include mission rotation chances in relic source modeling.
- Defer broader item acquisition systems outside the relic ecosystem.
- Defer wishlist and generalized mission recommendation features until later increments.

## MVP Scope

- Implement the PI1 relic-and-Prime-Part workflow before broader companion-app features.
- Cross-platform Avalonia GUI for Windows and Linux.
- Presentation layer for searching and viewing Prime Part acquisition paths.
- Application layer for importing, parsing, normalizing, and querying drop table data.
- Data storage layer for persisting relic, mission, rotation, and Prime Part relationships.
- User flow from desired Prime Part to matching relics to relevant relic-source missions.

## Core Goals

- Load publicly available Warframe drop tables.
- Parse drop table data into a structured internal model.
- Build acquisition paths for items, including relevant missions and reward rotations.
- Support user-facing planning features as the product matures.
- Present the results in a clear desktop GUI on Windows and Linux.

## Expected Capabilities

- Import and refresh drop table data from public sources.
- Normalize item names, relic names, mission names, and rotation metadata.
- Search for a desired item and view its acquisition path.
- Show known mission sources for relevant relics in PI1.
- Display mission rotations and associated drop chances where available.
- Persist parsed data in a storage layer suitable for refresh and query operations.

## Example User Value

A player searches for a desired Prime Part. The app shows which relics include that part and then lists missions where those relics can be earned, including rotation-specific chances when relevant. This gives the player a direct farming path without manually cross-referencing relic rewards against mission drop tables.

## Suggested Future Extensions

- Wishlist support for Prime Parts and relic goals.
- Mission rundowns based on multiple desired items.
- Account-specific progress tracking.
- Relic and fissure planning support.
- Alerts when new drop table data becomes available.
- Rotation-aware mission efficiency analysis.
