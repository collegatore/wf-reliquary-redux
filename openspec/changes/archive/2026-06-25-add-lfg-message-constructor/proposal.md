## Why

When a player wants to run a relic together with others, they advertise in Warframe's recruiting chat with a short, conventional message like `LF [Axi A1 Relic] rad` ("looking for group, everyone brings Axi A1 Relic refined to radiant"). Today the user has to hand-type that for every relic, remembering the exact item name and the refinement shorthand. The app already shows the exact relic names on the Relics and Missions tab, so it is well-placed to generate these messages with one click — and to let each player tailor the message format to their own conventions.

## What Changes

- **Compose action on relic cards.** Each relic card on the Relics and Missions tab gains a small compose icon. Clicking it opens a modal popup pre-filled for that relic.
- **LFG message popup.** A small modal window with one selector per configurable option group (e.g. participation, refinement), a live preview of the resulting message, and a copy-to-clipboard button.
- **Configurable message pattern (Settings).** A new Settings section lets the user edit the message pattern (e.g. `{participation} [{relic_name}] {refinement}`) and the option groups it references. A **Parse** button reconciles the option-group editors against the current pattern; **Save** persists; **Restore defaults** reverts to the in-code defaults.
- **Pattern engine.** A placeholder grammar with system placeholders (`{relic_name}` resolved from the card; `{relic_reward}` reserved but with no UI yet) and user-defined custom option groups (each option has a descriptor and a substituted value). Includes a validator (no unclosed/nested/empty placeholders; must contain `{relic_name}`) and a reconciler (Parse preserves existing groups, adds empty groups for new placeholders, drops removed ones).
- **Persistence.** The pattern and option groups are stored in the existing `user-settings.json`. Pre-V1: the settings format changes in place with no migration; existing settings files pick up the defaults on next load.

## Capabilities

### New Capabilities
- `lfg-message-constructor`: Generating Warframe recruiting-chat messages for a relic from a user-configurable pattern — the relic-card compose action and popup, the configurable pattern/option-group model, the pattern validator and reconciler, message substitution, and the Settings editor that manages all of it.

### Modified Capabilities
<!-- None. The compose icon and Settings section are additive surfaces owned by the new capability; no existing spec's requirements change. -->

## Impact

- **Application** — new `Lfg` models (`LfgMessageTemplate`, `LfgOptionGroup`, `LfgOption`), `LfgPatternParser` (validate + reconcile) and `LfgMessageBuilder` (substitute); `IUserSettingsStore` / `UserSettings` extended to carry the template.
- **Infrastructure** — `LocalUserSettingsStore` persists the template inside `SettingsFile` JSON (no migration; format evolves in place).
- **Composition** — `ReliquaryServiceFactory` wiring as needed.
- **Presentation** — a Settings sub-section editor view + viewmodel; a new `LfgMessageDialog` modal window + viewmodel (mirrors `WishlistNameDialog`, reuses the `CopyLogPath`-style clipboard delegate); a compose icon added to the relic-card `DataTemplate` that raises a request `MainWindow` handles to open the dialog.
- **Out of scope (deferred):** the `{relic_reward}` checkbox-dropdown UI and rarity-aware refinement defaults — `{relic_reward}` is only reserved in the engine's system-name set.
