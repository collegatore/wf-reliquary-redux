## Context

The Presentation project (`WF.Reliquary.Presentation`) currently has no `<ApplicationIcon>` set in its `.csproj` and no `Icon` property on `MainWindow`. Windows uses a generic placeholder for the taskbar button, title bar, and Explorer entry. The source asset (`icon-full-size.png`) is a square PNG with the branded sci-fi helmet logo.

## Goals / Non-Goals

**Goals:**
- Embed the branded icon in the Windows executable so it appears in Explorer, the taskbar pin, and the Alt-Tab switcher
- Show the icon in the Avalonia main window title bar and taskbar button at runtime
- Keep the conversion step reproducible without requiring third-party tool installation

**Non-Goals:**
- macOS or Linux icon bundles — Windows-only for now. **Linux note**: Linux does not use `.ico` at all; app icons there require a `.desktop` file with an `Icon=` entry and PNG assets placed under the XDG icon hierarchy (`/usr/share/icons/hicolor/<size>x<size>/apps/`). Different DEs (GNOME, KDE, XFCE) all follow the XDG spec so one set of files covers them, but this is a separate packaging step that is not implemented here.
- Dark-mode or high-contrast icon variants
- Splash screen or tray icon usage

## Decisions

### 1. Single-file strategy: ICO for both runtime and executable manifest

The same `icon.ico` serves both purposes: `<ApplicationIcon>` embeds it in the Windows PE, and Avalonia's `WindowIcon` reads it as an `AvaloniaResource` at runtime. Avalonia picks the appropriate frame from the multi-resolution ICO for the title bar rather than scaling a fixed-size PNG.

**Alternative tried first**: Embedding a 256×256 PNG as the Avalonia runtime icon while using the ICO only for the PE manifest. Rejected after testing: Avalonia scaled the 256px PNG down to the ~16–24px title bar slot in a single step, producing a visibly blurry result. Using the ICO directly lets Avalonia select the closest pre-rendered frame and scale a short distance, giving a sharper result.

### 2. Asset location: `Assets/` subfolder in Presentation project

A dedicated `Assets/` folder under `src/Presentation/WF.Reliquary.Presentation/` matches Avalonia project conventions and keeps image assets separate from `.axaml` and `.cs` files.

### 3. ICO generation via PowerShell + System.Drawing

`System.Drawing.Bitmap` is available on Windows without installing additional tools. A one-time PowerShell script resizes the source PNG and assembles the ICO binary format. The generated `icon.ico` is committed to the repo so no build-time generation step is needed.

**Alternative considered**: ImageMagick `magick` command. Would produce a higher-quality result but requires an external installation not guaranteed on all dev machines.

### 4. ICO frame sizes: 256, 48, 32 (no 16px)

The 16px frame is intentionally omitted. On high-DPI displays the title bar slot is ~20–24px; if a 16px frame is present Avalonia selects it as the closest match and scales it *up*, producing a blurry result. Without a 16px frame, Avalonia falls back to the 32px frame and scales it *down* slightly, which is visibly sharper. The Windows shell is unaffected — it selects frames intelligently regardless of which sizes are present.

### 5. ICO embedded as AvaloniaResource

`icon.ico` is declared as `<AvaloniaResource Include="Assets\icon.ico" />` and accessible at `avares://WF.Reliquary.Presentation/Assets/icon.ico`. `MainWindow` sets `Icon` to this URI. No separate PNG asset is included.

## Risks / Trade-offs

- **ICO binary assembly in PowerShell is manual** → Mitigated by using a well-known ICO format spec (ICONDIR + ICONDIRENTRY headers); the format is simple and stable.
- **System.Drawing availability** → `System.Drawing.Common` is available on .NET on Windows; this is a one-time dev script, not a build-time dependency.
- **High-DPI title bar quality** → Resolved by omitting the 16px frame; Avalonia now picks 32px and scales down. If a future display scaling level produces a slot smaller than 32px, re-examine frame selection.
