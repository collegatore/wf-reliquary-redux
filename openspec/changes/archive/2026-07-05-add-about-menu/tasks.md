## 1. About Dialog

- [x] 1.1 Create `AboutDialog.axaml` — a `Window` (`CanResize="False"`, `ShowInTaskbar="False"`, `WindowStartupLocation="CenterOwner"`) showing the application name, version, and a native `HyperlinkButton` (`NavigateUri` set to the repo README URL, `https://github.com/collegatore/wf-reliquary-redux/blob/main/README.md`), following the `ConfirmClearImportDialog` layout conventions.
- [x] 1.2 Create `AboutDialog.axaml.cs` — constructor sets the name/version text from `ApplicationInfo.Name` / `ApplicationInfo.Version`; static `ShowAsync(Window owner)` constructs the dialog and calls `ShowDialog(owner)`; a Close button click handler calls `Close()`.

## 2. Menu and Command Wiring

- [x] 2.1 Add `OpenAboutCommand` (`RelayCommand`) and an `Action? OpenAbout` hook to `MainWindowShellViewModel`, mirroring `OpenAppearanceCommand`/`OpenAppearance`.
- [x] 2.2 Add an `About` `MenuItem` to the File menu in `MainWindow.axaml`, after Drop Tables, Settings, and Appearance, bound to `OpenAboutCommand`.
- [x] 2.3 In `MainWindow.axaml.cs`'s `Wire()`, set `shell.OpenAbout = () => AboutDialog.ShowAsync(this);`.

## 3. Remove Version from Window Title

- [x] 3.1 Change `MainWindowViewModel.Title` (`MainWindowViewModel.cs:95`) to return `ApplicationInfo.Name` instead of `ApplicationInfo.GetStartupSummary()`.
- [x] 3.2 Update any existing unit tests asserting the window-title/`Title` property includes the version, so they assert the application name only.

## 4. Spec Sync and Verification

- [x] 4.1 Run the app, open File → About, verify the dialog shows the correct name and version and blocks interaction with the main window until closed.
- [x] 4.2 Click the README hyperlink and verify it opens the repository README in the default browser.
- [x] 4.3 Verify the main window title bar shows only the application name (no version).
- [x] 4.4 Run the existing test suite and fix any tests broken by the title/version change.
