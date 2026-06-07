# WF Reliquary Redux

## Project Description

WF Reliquary Redux is a companion app for [*Warframe*](https://www.warframe.com). Its goal is to help a player to find the best mission for a combination of items the player desires. The project is intended to process the official drop tables, extract mission and reward data, and organize that information to support item lookup, farming decisions, and activity planning. It should connect items to the missions, rotations, and reward sources where they can be obtained, and use those relationships to build acquisition paths for individual items or entire sets.

For stack decisions, project layout, build commands, and architecture see [docs/technical-reference.md](docs/technical-reference.md).

## Drop Table Source

The application can download and store the official Warframe drop-table HTML file. The default source URL is:

```text
https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html
```

Edit the source URL in the application UI to override the default for future downloads. The configured URL is stored as application-level user settings in the current user's local application data folder. Downloaded data is stored as a raw HTML file under `WF Reliquary Redux/DropTables`, with adjacent metadata for source URL, download time, and the parsed `Last Update` value. A download is considered unsuccessful if `Last Update` cannot be parsed from the downloaded file.

Use **Import stored file** in the application UI to parse the already downloaded HTML file into the local SQLite import database. The import action does not download or refresh the source file; use **Download drop tables** first when the stored source is missing or stale.

This workflow stops at normalized import status and stored parsed data. Item lookup, mission recommendation, and acquisition ranking remain future feature work.
