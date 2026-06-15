# WF Reliquary Redux

## Project Description

WF Reliquary Redux is a companion app for glorious [Warframe](https://www.warframe.com) grinders, specifically aimed at Relics and Primes. In the good old days, when there were like 10 Prime warframes and a couple dozen weapons, it was manageable. When it tripled, it became problematic. When DE started to drop a new batch every three months... Well, it's not funny anymore. 

As you probably know, DE, in their generosity, gives away the official drop tables for everything. I've seen a dozen tools that help players to navigate these tables, but none has been as helpful and comfortable as [Warframe Reliquary](https://wf.xuerian.net/), long live the author of the original idea. It had been very reliable when I first discovered the site, but as time passed, it started to lag on new item updates. Then it went off for a couple of months, and this repeated. I have no idea if it will be available when you read this text. So I decided to make a remake, just this time it has to be as independent from the developer as possible. That's why it is a desktop app, and everyone can have their own copy. It gets the raw tables and parses them locally, so the DE site remains the only external source of truth. And unless DE decides to change the format significantly, it should be highly resilient in the face of future releases.

The drill is simple: you want some gear, the app gives you a rundown on missions where you can get the corresponding relics. It supports wishlists as did the original site, but I personally never needed more than one. It supports player profiles for those who manage to actually keep more than one game account (I give you my deepest respects). It would store all drop table versions you encounter on your journey (for sentimental reasons), but everything not downloaded locally will be lost to time, like tears in rain.

## Technical overview

In the wake of the AI era, I decided to take the stack I use in my work but leave all coding to AI agentic tools. This way, I understand what's going on in the code and still can audit it somehow, but I also gain experience in no-code workflows. On the other hand, this means that practically anyone with a coding agent on hand can fork this repo and continue the development if I suddenly go dark. I use OpenSpec for spec-driven development, and you may find all design artifacts in the corresponding directory. Proposals and designs are mostly human-readable even if you have no technical expertise whatsoever.

For stack decisions, project layout, build commands, and architecture, see [docs/technical-reference.md](docs/technical-reference.md).

## Drop Table Source

The application can download and store the official drop table HTML files. The default source URL is:

```text
https://warframe-web-assets.nyc3.cdn.digitaloceanspaces.com/uploads/cms/hnfvc0o3jnfvc873njb03enrf56.html
```

Edit the source URL in the application settings UI to override the default if it changes, or if you don't trust the anonymous internet citizen (there's no auto-download and auto-import here either). The configured URL is stored as the application-level user settings file in the current user's local application data folder. Downloaded table data is stored as raw HTML content within the app database and can be exported to a file for cross-checking (or if anyone wants to peek at their history).

Use **Import stored source** in the Drop Tables UI to parse the downloaded HTML into the queryable tables. The import action does not download or refresh the source file, so when a new version comes out, press the download button.
