## Why

WF Reliquary Redux currently ships with no custom icon, so it displays a generic placeholder in the Windows taskbar, title bar, and Explorer. Adding the branded sci-fi helmet logo improves app identity and makes it instantly recognizable when running alongside other programs.

## What Changes

- Copy the provided full-size PNG (`icon-full-size.png`) into the Presentation project as an embedded asset
- Generate a multi-resolution `.ico` file (16, 32, 48, 256 px) from the source PNG
- Set `<ApplicationIcon>` in `WF.Reliquary.Presentation.csproj` so the Windows executable carries the icon
- Set `WindowIcon` on `MainWindow` so the Avalonia title bar and taskbar button show the icon at runtime

## Capabilities

### New Capabilities

- `app-icon`: Branding icon wired to both the Windows executable and the Avalonia main window

### Modified Capabilities

*(none)*

## Impact

- `src/Presentation/WF.Reliquary.Presentation/` — new `Assets/` folder with `icon.png` and `icon.ico`
- `WF.Reliquary.Presentation.csproj` — `<ApplicationIcon>` property and `<AvaloniaResource>` item added
- `MainWindow.axaml` — `Icon` property set to the embedded asset
- No API, database, or other layer changes
