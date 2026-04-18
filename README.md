# WF Reliquary Redux

## Project Description

WF Reliquary Redux is a companion app for [*Warframe*](https://www.warframe.com). Its goal is to help a player to find the best mission for a combination of items the player desires. The project is intended to process the official drop tables, extract mission and reward data, and organize that information to support item lookup, farming decisions, and activity planning. It should connect items to the missions, rotations, and reward sources where they can be obtained, and use those relationships to build acquisition paths for individual items or entire sets.

## Core Technology Decisions

- Programming language: C#
- Runtime and SDK: .NET
- Primary data storage: SQLite
- Development environment: Visual Studio Code
- Target operating systems: Windows and Linux
- GUI framework: Avalonia

## Suggested Future Extensions

- Wishlist support and account-specific progress tracking.
- Mission rundowns based on multiple desired items.
- Alerts when new drop table data becomes available.
- Rotation-aware mission efficiency analysis.
- MCP
- OCR for mission rewards / inventory tracking
- mission duration statistics