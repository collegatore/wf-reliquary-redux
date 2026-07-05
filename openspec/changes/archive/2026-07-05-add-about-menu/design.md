## Context

The main window title is bound to `MainWindowViewModel.Title` (`src/Presentation/WF.Reliquary.Presentation/MainWindowViewModel.cs:95`), which returns `ApplicationInfo.GetStartupSummary()` (`"{Name} {Version}"`, see `src/Application/WF.Reliquary.Application/ApplicationInfo.cs`). The File menu (`MainWindow.axaml:87-98`) already holds three items — Drop Tables, Settings, Appearance — each wired through a command on `MainWindowShellViewModel` and, for window-opening items, a delegate `Action`/`Func` set up in `MainWindow.axaml.cs` (the code-behind owns `Window` instances since view models can't reference `Window` directly).

Two existing dialog shapes can serve as a pattern:
- **Modeless, VM-backed** (`AppearanceWindow`) — open-or-activate, `Show(owner)`, stays open alongside the main window.
- **Modal, static-factory** (`ConfirmClearImportDialog`, `RecruitingMessageDialog`) — `static Task ShowAsync(Window owner)` that constructs the dialog and calls `ShowDialog(owner)`.

The user asked for a modal window, so the second shape fits.

## Goals / Non-Goals

**Goals:**
- Add a File → About menu item that opens a modal dialog.
- Show application name and version in that dialog.
- Provide a clickable hyperlink to the repo's README, opened in the user's default browser.
- Remove the version from the window title, leaving only the application name.

**Non-Goals:**
- No changelog, license text, or credits list in the dialog — just name, version, and the README link.
- No persistence of dialog state (position, etc.) — it's a simple modal, always centered on owner.
- Not introducing a general-purpose "hyperlink" control/style for reuse elsewhere; Avalonia's built-in control already covers the one link this dialog needs.

## Decisions

- **Modal `Window` with a static `ShowAsync(Window owner)` factory**, mirroring `ConfirmClearImportDialog`/`RecruitingMessageDialog`. No dedicated view model: all displayed content (`ApplicationInfo.Name`, `ApplicationInfo.Version`, the README URL) is static, so the dialog can read `ApplicationInfo` directly (e.g. via code-behind-set `TextBlock.Text` in the constructor, or `x:Static` bindings in XAML). This avoids a throwaway VM class for a dialog with no interactive state.
- **Command wiring mirrors `OpenAppearanceCommand`**: `MainWindowShellViewModel` gets `OpenAboutCommand` (a `RelayCommand`) plus an `Action? OpenAbout` hook invoked by the command; `MainWindow.axaml.cs` sets `shell.OpenAbout = () => AboutDialog.ShowAsync(this);` in `Wire()`. Unlike Appearance, no need to track a singleton instance field since `ShowDialog` blocks and disposes on close.
- **Hyperlink implementation**: Avalonia 11.1+ ships a native `Avalonia.Controls.HyperlinkButton` (available in the installed 12.0.5) — a `Button` subclass with a `NavigateUri` property that automatically opens the URI via Avalonia's `ILauncher` service when clicked, plus an `IsVisited` property for visited-state styling. This is used directly in XAML (`<HyperlinkButton Content="README" NavigateUri="..." />`), with no code-behind click handler or manual launcher call needed. The URL is a literal in the dialog's XAML: `https://github.com/collegatore/wf-reliquary-redux/blob/main/README.md`.
- **Window title change**: `MainWindowViewModel.Title` changes from `ApplicationInfo.GetStartupSummary()` to `ApplicationInfo.Name`. Kept as a `Title` property (not renamed) since it's already an established binding path (`DropTables.Title`) and renaming would be a gratuitous change beyond this proposal's scope.

## Risks / Trade-offs

- [Removing the version from the title bar could reduce at-a-glance visibility for support/bug-report purposes] → Mitigated by `ApplicationInfo.Version` still being logged at app startup (`app.log`, unchanged) and now surfaced in an easy-to-find About dialog.
