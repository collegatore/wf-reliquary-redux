## Why

The application version is currently only visible in the OS window title bar (`WF Reliquary Redux <version>`), which is easy to overlook and crowds the title with information most users only need occasionally. There's also no discoverable link back to the project's repository or documentation from within the app. A standard File → About menu item with a modal dialog gives the version, and a way to reach the README, a conventional, discoverable home.

## What Changes

- Add an **About** item to the File menu, below the existing Drop Tables / Settings / Appearance items.
- Selecting About opens a modal dialog showing the application name, version, and a hyperlink to the repository's README.
- **BREAKING** (behavior, not API): the main window title no longer includes the version — it shows the application name only. The version moves exclusively into the About dialog.
- The hyperlink opens the README in the user's default browser (`https://github.com/collegatore/wf-reliquary-redux/blob/main/README.md`).

## Capabilities

### New Capabilities
- `about-dialog`: File → About menu item and the modal dialog it opens, showing app name, version, and a README hyperlink.

### Modified Capabilities
- `app-version-identity`: the "Version Visible in the Window Title" requirement is replaced — the window title shows only the application name, and the version is instead shown in the About dialog.

## Impact

- `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml` — add the About `MenuItem`; change `Window.Title` binding to drop the version.
- `src/Presentation/WF.Reliquary.Presentation/MainWindowViewModel.cs` — `Title` property no longer calls `ApplicationInfo.GetStartupSummary()`.
- `src/Presentation/WF.Reliquary.Presentation/MainWindowShellViewModel.cs` — new `OpenAboutCommand`.
- `src/Presentation/WF.Reliquary.Presentation/MainWindow.axaml.cs` — wire the command to show the new dialog (owner = main window).
- New files: `AboutDialog.axaml` / `AboutDialog.axaml.cs` (new modal `Window`, `ConfirmClearImportDialog`-style `ShowAsync(Window owner)`), no dedicated view model needed since content is static `ApplicationInfo` values plus a fixed URL.
- `openspec/specs/app-version-identity/spec.md` — delta spec removing the title-bar version requirement.
