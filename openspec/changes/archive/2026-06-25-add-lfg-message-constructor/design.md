## Context

The app already parses Warframe drop tables and shows relic cards on the Relics and Missions tab (`WishlistRelicsViewModel` → `WishlistRelicCardViewModel`, rendered by a `DataTemplate` in `MainWindow.axaml`). Each card knows its full display name (`"{Era} {Code} Relic"`, e.g. `Axi A1 Relic`) and carries its rewards with per-reward `RewardTier` and `IsWishlisted` flags. Settings persist to a single `user-settings.json` via `LocalUserSettingsStore`, and `SettingsViewModel` already implements a draft/saved/dirty pattern (live editable values vs. `savedX` baselines, `HasUnsavedChanges`, discard-rollback). Dialogs are small modal `Window`s (`WishlistNameDialog.ShowAsync(owner, …)`), and clipboard access is done via a `CopyLogPath`-style delegate injected from `MainWindow` (clipboard needs a `TopLevel`). The codebase enforces a strict four-layer architecture (Presentation → Application ← Infrastructure, Composition wires both) via `ProjectReferenceBoundaryTests`, and a UI-concurrency contract that keeps all DB/CPU work off the UI thread.

This change adds a way to generate recruiting-chat messages like `LF [Axi A1 Relic] rad` from a relic card, plus a Settings editor for the message format. The pattern engine is the substantive new logic; everything else is wiring onto existing seams.

## Goals / Non-Goals

**Goals:**
- Pure, unit-testable pattern engine in the Application layer: validate, reconcile (Parse), and substitute (build message).
- A configurable template (pattern + custom option groups) with in-code defaults, persisted in `user-settings.json`.
- A relic-card compose action that opens a modal popup with per-group selectors, a live preview, and copy-to-clipboard.
- A Settings sub-section that edits the template using the existing draft/save/dirty conventions, with Parse / Save / Restore defaults.
- Reserve `{relic_reward}` in the system-name set so the later reward feature is purely additive.

**Non-Goals:**
- The `{relic_reward}` checkbox-dropdown UI (wishlisted parts) — deferred.
- Rarity-aware refinement defaults (`inta` for common, `rad` for uncommon/rare) — deferred; it only resolves once a specific reward is chosen, which is the `{relic_reward}` work.
- Any settings-file migration. Pre-V1, the format changes in place; existing files pick up defaults on next load.
- Sending the message to Warframe directly — the user pastes from the clipboard.

## Decisions

### Pattern grammar: braces always mean placeholders
A placeholder is `{name}` where `name` is an identifier (`[A-Za-z_][A-Za-z0-9_]*`). There are no literal braces in patterns; the literals used in practice are `[` and `]`. This keeps validation simple: any `{` or `}` not forming a well-formed `{name}` is an error. System placeholder names are `relic_name` and `relic_reward`; every other name is a custom option group.

**Why:** the alternative — reusing .NET composite formatting (`string.Format` / `CompositeFormat.Parse`) — fights the requirements. Composite format is positional (`{0}`) with `{{`/`}}` escaping; our placeholders are named, need custom-vs-system classification, and need *specific, positioned* error messages for the Parse UX. Translating named→indexed already requires parsing the braces ourselves, after which the formatter only re-validates more weakly (a generic `FormatException` that can't distinguish nested vs. unclosed vs. empty). So we own the scan.

### Validation: single forward scan, precise positioned errors
`LfgPatternParser.Validate` is a left-to-right pass tracking whether it is inside a placeholder. It reports, with the offending position: unclosed (`{` with no `}`), nested (`{` while already open), empty (`{}`), stray `}` (close with nothing open), and invalid name character. On success it returns the placeholder names in order of appearance. A separate semantic check requires `{relic_name}` to be present. Substitution (`LfgMessageBuilder`) is a plain `Regex.Replace(pattern, "\\{(\\w+)\\}", …)` because there are no format specifiers to honor.

**Why over a regex-residue approach (`Regex.Matches("\\{[^{}]*\\}")` + leftover-brace check):** the residue method is more compact but collapses nested/unclosed/stray into one generic "stray brace" bucket, losing the per-rule messages the Parse button is meant to show.

### Parse is reconcile, not reset
`LfgPatternParser.Reconcile(newPattern, currentGroups, savedGroups)` validates, then for each custom placeholder (in order) applies a three-tier lookup: keep the current draft group if present → restore from `savedGroups` if the draft has dropped it but saved config has it → create an empty group. Groups whose placeholder vanished are dropped. On validation failure it returns the error and leaves the caller's draft untouched.

The `savedGroups` parameter is the last-persisted template's groups, supplied by the editor viewmodel. This lets the reconciler restore options when a user re-introduces a placeholder they had temporarily removed, without the viewmodel itself needing to cache the original config separately.

**Why:** a reset would wipe the user's carefully entered options every time they tweak the pattern. Reconcile lets them add/remove placeholders without losing configured groups, and ordering groups by appearance keeps the editor aligned with the pattern.

### Defaults in code; draft/save/restore mirrors existing Settings; no separate LFG Save button
`LfgMessageTemplate.Default` is a static in code (pattern + `participation` + `refinement` groups). The Settings editor holds the template as a draft with a saved baseline, exactly like `SettingsViewModel` does for font size / source URL. There is no separate Save button for the LFG editor; the LFG template is persisted via the main Settings Save together with all other settings. `SettingsViewModel` holds delegates `GetCurrentLfgTemplate` and `IsLfgTemplateDirty` (injected by `MainWindow`), so `HasUnsavedChanges` and `SaveAsync` include LFG state without a direct reference to `LfgTemplateEditorViewModel`. `MainWindowShellViewModel.OnSettingsSaved` notifies `LfgEditor.NotifySaved` after each successful save to clear the LFG editor's dirty state. Restore defaults sets the draft to `LfgMessageTemplate.Default` (still requiring Save to persist).

**Why no separate Save:** the original design had a separate LFG Save button in Settings. During implementation it was found to create a split-save problem — the user had to remember to press two Save buttons on the same screen, and the leave-guard was inconsistent about which button had been pressed. Wiring the LFG template through the main Settings Save gives a single, predictable commit point consistent with all other editable settings on the page.

### Dirty-state tracking via explicit flag, not computed comparison
`LfgTemplateEditorViewModel.HasUnsavedChanges` is an explicit `isDirty` boolean set to `true` by any edit path (`NotifyDirty`) and `false` by `NotifySaved`. The original design computed dirty state by comparing `CurrentDraftTemplate()` against `savedTemplate` via `DraftMatchesSaved()`. This was replaced because: (1) Avalonia's `ComboBox` TwoWay binding may write `null` to `SelectedDefaultOption.set` during binding initialization, which silently corrupts `defaultValue` and makes the comparison permanently mismatch; (2) the comparison is evaluated after an `await` in `SaveAsync`, during which queued binding updates can arrive and cause a timing gap. The explicit flag is immune to both. A null guard on `SelectedDefaultOption.set` (`if (value is null) return;`) is retained as additional defence against Avalonia write-backs.

### Single-option groups hidden in the popup
`LfgMessageDialogViewModel` builds selectors for all custom groups internally (`allSelectors`) but exposes only those with more than one option in `Selectors`. The popup binds to `Selectors`, so single-option groups are silently substituted with their only value and produce no visible UI element.

**Why:** a "selector" with one option gives the user nothing to choose. Hiding it de-clutters the popup for common cases without removing the substitution. `allSelectors` retains all groups so `LfgMessageBuilder.Build` still receives a complete value map.

### Pattern displayed in popup before selectors
The popup renders the active template pattern string above the group selectors so users can verify they are filling in the right fields without returning to Settings, and to give context when all groups have a single option (i.e. no selectors appear).

### Persistence: extend the settings model, no migration
`UserSettings` gains an `LfgMessageTemplate` field; `LocalUserSettingsStore.SettingsFile` gains a nested object serialized with the existing `JsonSerializerOptions`. On load, a missing template yields `LfgMessageTemplate.Default`.

**Why:** the template is per-user configuration that belongs with other user settings; pre-V1 rule allows changing the format in place. **Re-initialization note:** no DB or file wipe is required — existing `user-settings.json` files simply gain the template on next save and read defaults until then.

### Popup is a modal Window; wiring follows existing dialogs
`LfgMessageDialog` mirrors `WishlistNameDialog`: a small modal `Window` with a `ShowAsync(owner, template, relicName)` entry point and its own viewmodel building per-group selectors, the live preview, and a copy command. The relic card's `WishlistRelicCardViewModel` cannot open a `Window` (needs the owner and clipboard, both `MainWindow` concerns), so the card raises a compose request that `MainWindow` handles — the same indirection the existing dialogs use.

The compose action lives in a per-card overflow menu identical to the prime gear cards: a `⋮` button in a `ColumnDefinitions="*,Auto"` header (name left, button right) whose `Button.Flyout` holds a `MenuFlyout` with a `MenuItem` (`MenuFlyout Placement="BottomEdgeAlignedRight"`, transparent borderless button). Using the established menu host means future per-relic actions slot into the same flyout rather than accreting standalone icons.

**Why:** "small pop-up window" matches the modal-`Window` pattern already in the codebase; the `⋮` menu matches the existing prime-gear card affordance for per-card actions; reusing the `CopyLogPath`-style clipboard delegate avoids leaking `TopLevel`/Infrastructure into viewmodels.

### Layer placement
Application: `LfgMessageTemplate`, `LfgOptionGroup`, `LfgOption`, `LfgPatternParser`, `LfgMessageBuilder`, and the extension of `IUserSettingsStore`/`UserSettings`. Infrastructure: `LocalUserSettingsStore` persistence. Composition: any new wiring. Presentation: the editor view/viewmodel, the dialog, and the card icon. The bulk of testable behavior (validation, reconciliation, substitution, defaults) lives in pure Application classes with no ports — covered by Unit tests; persistence round-trip and viewmodel wiring covered by Integration tests.

## Risks / Trade-offs

- **`{relic_name}` link correctness** → The bracketed text must be the exact in-game item name for Warframe chat to render an item link. We resolve `{relic_name}` to the full `DisplayName` including the `Relic` suffix (`Axi A1 Relic`) and keep refinement *outside* the brackets in the default pattern. If a user authors a pattern that puts other text inside the brackets, the link may not render — that is the user's responsibility, not a validation concern.
- **User-authored patterns can be valid but unhelpful** → e.g. a pattern with no custom groups, or one that omits the brackets. Validation only enforces structure + presence of `{relic_name}`; semantic quality is left to the live preview, which shows the user exactly what they will get.
- **Empty option group at construction time** → blocked at Save, so a persisted template always has fillable groups. A defensive fallback in `LfgMessageBuilder` (substitute empty string for a group with no options) avoids a crash if a malformed template is ever loaded.
- **Deferring rarity-aware refinement default** → users manually switch `inta`/`rad` in v1. Accepted: the smart default is meaningless without a chosen reward, which arrives with `{relic_reward}`. Reserving that name now keeps the follow-up additive.
