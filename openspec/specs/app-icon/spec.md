# Capability: App Icon

## Purpose

The application SHALL ship with a branded icon embedded in the Windows executable and displayed in the main window title bar and taskbar button at runtime.

## Requirements

### Requirement: Presentation project embeds branded icon assets

The Presentation project SHALL include `Assets/icon.ico` (multi-resolution: 32, 48, 256 px) and `Assets/icon-full-size.png` (the original source) derived from the source PNG. No separate `icon.png` is included; the ICO serves both the PE manifest and the Avalonia runtime icon.

#### Scenario: Assets present in project directory
- **WHEN** a developer checks out the repository
- **THEN** `src/Presentation/WF.Reliquary.Presentation/Assets/icon.ico` and `Assets/icon-full-size.png` exist and are non-empty

#### Scenario: ICO is embedded as AvaloniaResource
- **WHEN** the Presentation `.csproj` is built
- **THEN** `icon.ico` is embedded via `<AvaloniaResource>` and accessible at `avares://WF.Reliquary.Presentation/Assets/icon.ico`

### Requirement: Windows executable carries the branded icon

The Presentation `.csproj` SHALL set `<ApplicationIcon>` to the `.ico` file so that Windows Explorer, the taskbar pin, and the Alt-Tab switcher display the branded icon for the compiled executable.

#### Scenario: Executable icon visible in Explorer
- **WHEN** the application is built and the `.exe` is viewed in Windows Explorer
- **THEN** the branded icon is displayed instead of a generic placeholder

#### Scenario: ApplicationIcon property set in csproj
- **WHEN** `WF.Reliquary.Presentation.csproj` is inspected
- **THEN** `<ApplicationIcon>Assets\icon.ico</ApplicationIcon>` appears in the first `<PropertyGroup>`

### Requirement: Main window displays the branded icon at runtime

`MainWindow.axaml` SHALL set the `Icon` property to the embedded ICO asset so the title bar and taskbar button show the branded icon while the application is running.

#### Scenario: Window title bar shows icon
- **WHEN** the application is launched
- **THEN** the main window title bar displays the branded icon

#### Scenario: Icon property wired in MainWindow
- **WHEN** `MainWindow.axaml` is inspected
- **THEN** the `Window` element has `Icon="avares://WF.Reliquary.Presentation/Assets/icon.ico"`
