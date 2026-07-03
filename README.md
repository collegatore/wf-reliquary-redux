# WF Reliquary Redux

## Project Description

Hey, Dreamers.

WF Reliquary Redux is a companion app for [Warframe](https://www.warframe.com) grinders, specifically aimed at Relics and Primes. In the good old days, when there were like 10 Prime warframes and a couple dozen weapons, it was manageable. When it tripled, it became problematic. When DE started to drop a new batch every three months... Well, it's not funny anymore. 

As you probably know, DE, in their generosity, gives away the official drop tables for everything. I've seen a dozen tools that help players to navigate these tables, but none has been as helpful and comfortable as [Warframe Reliquary](https://wf.xuerian.net/), long live the author of the original idea. It had been very reliable when I first discovered the site, but as time passed, it started to lag on new item updates. Then it went off for a couple of months, and this repeated. I have no idea if it will be available when you read this text. So I decided to make a remake, just this time it has to be as independent from the developer as possible. That's why it is a desktop app, and everyone can have their own copy. It gets the raw tables and parses them locally, so the DE site remains the only external dependency. And unless DE decides to change the format significantly, it should be highly resilient in the face of future releases.

The drill is simple: you want some gear, the app gives you a rundown on missions where you can get the corresponding relics. It supports wishlists as did the original site, but I personally never needed more than one. It supports player profiles for those who manage to actually keep more than one game account (I give them my deepest respects). It would store all drop table versions you encounter on your journey (for sentimental reasons), but everything not downloaded locally will be lost to time, like tears in rain.

## Technical note

In the wake of the AI era, I decided to take the stack I use in my work but leave all coding to AI agentic tools. This way, I understand what's going on in the code and still can audit it somehow, but I also gain experience in no-code workflows. On the other hand, this means that practically anyone with a coding agent on hand can fork this repo and continue the development if I suddenly go dark. I use OpenSpec for spec-driven development, and you may find all design artifacts in the corresponding directory. Proposals and designs are mostly human-readable even if you have no technical expertise whatsoever.

For stack decisions, project layout, build commands, and architecture, see [docs/technical-reference.md](docs/technical-reference.md).

## Drop Table Source

The application can download and store the official drop table HTML files. The default source URL is:

```text
https://www.warframe.com/droptables
```

This is DE's canonical address; it redirects to the current CDN location of the drop-table HTML (a `warframe-web-assets` DigitalOcean asset at the time of writing), and the download follows that redirect automatically. Edit the source URL in the `File -> Settings` menu to override the default if it changes, or if you don't trust the anonymous internet citizen (there's no auto-download and auto-import here either). The configured URL is stored in the user settings file, downloaded table data is stored as raw HTML content within the app database and can be exported to a file for cross-checking (or if anyone wants to peek at their history).

## Initial Setup

You probably should land on the `Drop Tables` tab right away from the start, but if something went wrong it is available at `File -> Drop Tables`.

`Download from URL` fetches raw data from the CDN source. It gets timestamped, which is the main sign of whether you have the latest version or not. 

`Read from file` is a more paranoid version, that allows you to block the app at the firewall and never be endangered by its shady businesses hidden in the plain sight of the open source repository. But it also can be useful when you exported the history of downloads into a folder and then want it back.

`Import stored source` parses the raw HTML from wherever you got it into the queryable database tables. 

The import action does not download or refresh the source file, and vice versa - download does not import anything. Therefore, when a new version comes out, repeat both steps. I intentionally made this a manual action to ~~reduce the carbon footprint~~ avoid anxiety of "my app does something without my command" (we have enough of that already on our PCs I guess).

Initially `Prime Gear` shows a lot of items (even more if you choose to show vaulted). To declutter the space, use "Mark as owned" on owned gear cards. This action is profile-wide, so you won't have to repeat on every new wishlist. If you own a lot of items already, there's group operations. If you own more than a half of everything, it might be usefult to "select all" and then deselect the missing parts. In Warframe there's `Esc -> Profile -> My Profile -> Equipment` tab which is the good point for cross-reference when sorted by Progress (items never mastered are at the very bottom of the list).

`Relics and Missions` have a couple of layout variants in `File -> Appearance` menu, please check if you prefer a non-default one. Relic cards are closer to the modern Web look, but I personally find a table form much more useful.

## App Data

The app stores its database and configs in `%LOCALAPPDATA%\WF Reliquary Redux\`. I think it should be available on almost every gaming PC without UAC elevation and other mess.

## Localization

To whoever might need this I apologize, as it is not planned. Biggest challenge I see is that neither Relic nor Reward names have any source of publicly available official translations. Which means, any custom translation will need a manual update when new items are released - and this is exactly the opposite of what I wanted to achieve with the app. Also there's a mess with chat message links like `[Axi A1 Relic]`: they effectively work only when a reference in chat exactly matches a name in language set for the Warframe client. So English is the safe default, I guess (I even doubt that [Warframe Market](https://warframe.market/) has any kind of a translation support).

## User Guide

The behaviors that aren't self-evident from the UI — ghost wishlist entries, profiles, and the Recruiting Chat message constructor — are covered in the [User Guide](docs/user-guide.md).
