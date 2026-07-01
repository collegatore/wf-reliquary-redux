## Context

The app is an Avalonia MVVM desktop app (CommunityToolkit.Mvvm). Font size, relic card width, and Relics/Missions layout orientation currently live in `SettingsViewModel` alongside unrelated Save/Back-guarded settings (source URL, LFG template, relics panel height). All three already apply live: `SettingsViewModel` writes to shared `MainWindowShellViewModel` properties (`RelicCardWidth`, `Orientation`) via callbacks wired up in `MainWindow.axaml.cs`, and to `DynamicResource`s the whole app binds to — then auto-saves to `IUserSettingsStore` immediately, bypassing the Settings tab's Save button entirely.

The main `TabControl` in `MainWindow.axaml` is a standard single-`ContentPresenter` control: switching tabs tears down the previously-visible tab's content. That's why moving Settings-tab-adjacent controls into their own tab wouldn't solve the underlying problem — the user still couldn't see Prime Gear/Relics & Missions while adjusting Appearance. A separate, modeless window sidesteps this because it doesn't replace anything in `MainWindow`; it sits alongside it.

The app's only existing secondary-window pattern (`LfgMessageDialog`, `ConfirmDiscardDialog`, `OwnedGearTrackingDialog`) is modal (`ShowDialog`, static `ShowAsync(owner)` factory). This change introduces the first modeless secondary window.

## Goals / Non-Goals

**Goals:**
- Add `File → Appearance`, opening a modeless, single-instance window.
- Move (not mirror) font size, relic card width, and orientation controls out of the Settings tab into this window.
- Preserve all existing live-apply, auto-save, clamping, and font-scaling behavior for these three settings exactly as-is — only their host UI changes.
- Keep `MainWindow` as the sole live-preview surface: no new rendering of relic cards or app content inside the Appearance window itself.
- The Appearance window opens at a fixed size, positioned 50px left and 50px down from `MainWindow`'s current position, every time.

**Non-Goals:**
- No new preview surface, sample card, or duplicated `RelicsMissionsView` instance inside the Appearance window.
- No changes to `IUserSettingsStore`/`LocalUserSettingsStore` or the `UserSettings` shape — the window's size/position are neither resizable nor persisted, so no new fields are needed (a resizable + persisted-bounds version was built and then deliberately reverted; see Decision 7).
- No enforcement mechanism (code-level) for the "only live-applied cosmetics" guardrail — it's a documented convention for future changes to respect, not a runtime check.

## Decisions

**1. Extract a new `AppearanceViewModel` rather than reusing `SettingsViewModel` as-is.**
`SettingsViewModel`'s remaining responsibilities (Save/Back, unsaved-changes guard, source URL, LFG template) are unrelated to appearance and already architecturally separate (per the settings-tab spec, none of the three appearance settings ever participated in the guard). Extracting keeps each view model's concerns aligned with its window/tab. The font-size-to-card-width scaling math (`ApplyFontSize`, currently in `SettingsViewModel`) moves with the properties it computes.
*Alternative considered*: keep the properties on `SettingsViewModel` and bind `AppearanceWindow` to the same instance used by the Settings tab. Rejected — `SettingsViewModel` is constructed per-navigation today and carries guard state that has no meaning for a standalone window; reusing it would mean carrying dead weight or awkwardly branching its behavior by host.

**2. `AppearanceViewModel` talks to the shell the same way `SettingsViewModel` does today.**
`MainWindow.axaml.cs` currently wires `settings.ApplyOrientation`/`ApplyRelicCardWidth` callbacks to `MainWindowShellViewModel` properties. The same wiring pattern applies to `AppearanceViewModel` — constructed once, callbacks pointed at the shell, no new propagation mechanism.

**3. Window lifecycle: single nullable reference, `Show()` not `ShowDialog()`.**
A `File → Appearance` command checks a tracked instance: if none exists (or it was closed), construct and `Show()` a new `AppearanceWindow` with `Owner` set to `MainWindow`; if one exists, call `Activate()` on it instead of creating a second. The tracked reference is cleared on the window's `Closed` event so it works regardless of how the window was closed (X button, Alt+F4, etc.).
*Alternative considered*: a full dialog-service abstraction. Rejected as unnecessary scope for a single window — the app has no such service today and one window doesn't justify introducing one.

**4. Owner relationship kept even though modeless.**
Setting `Owner = MainWindow` (without `ShowDialog`) still gives correct taskbar/minimize/restore grouping on Windows, at no cost to the "both windows usable simultaneously" goal — modeless owned windows don't block input to the owner.

**5. Settings tab shrinks; no guard-logic changes needed.**
Removing the Appearance section from `SettingsView.axaml`/`SettingsViewModel` requires no changes to the unsaved-changes guard, since font size/card width/orientation were already excluded from it. This is a deletion, not a behavior change, for everything that stays in Settings.

**6. The Appearance window's own controls are pinned to a fixed small font size: `Window.FontSize` plus a local `Window.Styles` override.**
`App.axaml` applies `TextBlock`/`TextBox` styles app-wide, binding `FontSize` to the dynamic `ContentFontSize` resource — the very thing the Appearance window's own font-size control changes. Left alone, the window's own labels would resize themselves as the user drags the control that's supposed to only affect the main window. Two mechanisms are needed together: setting `FontSize="{StaticResource FontSizeSmall}"` on the `Window` itself covers controls that pick up font size through ordinary property-value inheritance (e.g. `RadioButton`'s content, which the Fluent theme renders via a `TemplateBinding` to `RadioButton.FontSize`, not a standalone styled `TextBlock`); a local `Window.Styles` override for the `TextBlock`/`TextBox` selectors is still needed on top, since `App.axaml` sets those two explicitly via a `Setter` (which beats inherited values) — a window-local style of equal selector specificity takes precedence over the Application-level one, so the override is reliable without touching global styles.

**7. Window bounds: fixed size, non-resizable, offset-from-owner position — no persistence, after three attempts at persisting it.**
The window went through three iterations: (a) fire-and-forget bounds save on `AppearanceWindow.Closing`, which raced `MainWindow.OnWindowClosing`'s blocking shutdown save and silently dropped bounds when the user closed the Appearance window and then the app in quick succession (caught by manual verification, task 8.7); (b) making that save block too, which fixed the race but violated the codebase's hard rule that only `MainWindow.axaml.cs` may block on async (enforced by `UiConcurrencyContractTests.InteractiveCode_DoesNotBlockOnAsync_OutsideStartupShutdown`); (c) routing the disk write through `MainWindow`'s own shutdown boundary instead, which worked but was still failing manual verification in practice.
Given that, the requirement itself was reconsidered: the window doesn't need to be resizable or remember its position at all. It now has a fixed `Width`/`Height` (`CanResize="False"`), and `MainWindow.OpenOrActivateAppearanceWindow()` sets its `Position` to `MainWindow.Position` offset by `(-50, +50)` every time it opens — computed fresh from the owner's current position, never read from or written to settings. This removed the `AppearanceWindowWidth/Height/X/Y` fields from `UserSettings` and the `AppearanceViewModel` properties/window logic that read and wrote them, along with the entire persistence problem.

## Risks / Trade-offs

- **[Risk]** Stale single-instance tracking if the `Closed` event isn't wired correctly (e.g. window can be "activated" after it's already gone) → **Mitigation**: clear the tracked reference in the `Closed` handler unconditionally, and guard the activate-vs-create branch on the reference being non-null.
- **[Risk]** Future contributors add a non-live-apply setting to the Appearance window, eroding its "always live" identity → **Mitigation**: this design doc and the proposal's guardrail note serve as the documented convention; no code enforcement exists, so this relies on review discipline.
- **[Risk]** Extracting `AppearanceViewModel` touches `MainWindow.axaml.cs` wiring and `SettingsViewModel` simultaneously, so a mid-refactor state could leave both views bound to overlapping state → **Mitigation**: move the three settings' properties/commands/callbacks in one pass rather than mirroring them temporarily; tasks.md should sequence this as a single atomic step, not incremental duplication.
- **[Risk]** (Resolved — see Decision 7) Persisting Appearance window bounds proved not worth the complexity: two implementation attempts had real correctness bugs (a lost-write race, then a concurrency-contract violation), caught by manual verification rather than by design. Resolved by dropping the requirement — fixed size and owner-relative position need no persistence at all.

## Migration Plan

Not applicable — no persisted data or schema changes. This is a UI-only relocation of existing controls; `IUserSettingsStore` reads/writes for font size, card width, and orientation are unchanged in shape, so no user-facing settings-file migration is needed.

## Open Questions

None outstanding.
