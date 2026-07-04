## MODIFIED Requirements

### Requirement: Presentation project embeds branded icon assets

The Presentation project SHALL include `Assets/icon.ico` (multi-resolution: 32, 48, 256 px) and `Assets/icon-full-size.png` (the original source) derived from the source PNG. No separate `icon.png` is included; the ICO serves both the PE manifest and the Avalonia runtime icon.

#### Scenario: Assets present in project directory
- **WHEN** a developer checks out the repository
- **THEN** `src/Presentation/WF.Reliquary.Presentation/Assets/icon.ico` and `Assets/icon-full-size.png` exist and are non-empty

#### Scenario: ICO is embedded as AvaloniaResource
- **WHEN** the Presentation `.csproj` is built
- **THEN** `icon.ico` is embedded via `<AvaloniaResource>` and accessible at `avares://WF.Reliquary.Redux/Assets/icon.ico` (the assembly component follows `<AssemblyName>`, which is `WF.Reliquary.Redux`)

### Requirement: Main window displays the branded icon at runtime

`MainWindow.axaml` SHALL set the `Icon` property to the embedded ICO asset so the title bar and taskbar button show the branded icon while the application is running.

#### Scenario: Window title bar shows icon
- **WHEN** the application is launched
- **THEN** the main window title bar displays the branded icon

#### Scenario: Icon property wired in MainWindow
- **WHEN** `MainWindow.axaml` is inspected
- **THEN** the `Window` element has `Icon="avares://WF.Reliquary.Redux/Assets/icon.ico"`
