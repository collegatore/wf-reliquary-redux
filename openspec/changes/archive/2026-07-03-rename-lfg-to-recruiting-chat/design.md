## Context

The LFG Message Constructor feature is fully implemented. Its user-facing labels all use the term "LFG message", which is Warframe-community shorthand for Looking-for-Group. In-game, the channel where these messages are posted is labeled "Recruiting Chat", making that the more accurate and discoverable term for new players. This change updates only display strings; no logic, data model, or API changes are needed.

## Goals / Non-Goals

**Goals:**
- Update every user-visible string that says "LFG message" to say "Recruiting Chat message"
- Update the spec and docs to match the new terminology

**Non-Goals:**
- Renaming code identifiers (`LfgMessageDialog`, `LfgTemplateEditorViewModel`, `LfgPatternParser`, `Lfg/` namespace, etc.) — the internal naming remains as-is
- Changing any feature behavior or data format

## Decisions

**All `Lfg*` identifiers rename to `RecruitingMessage*`.** Every class, file, and namespace carrying the `Lfg` prefix is renamed in lockstep with the display-string change so the codebase uses a single consistent term throughout. Applying only a partial rename (e.g. display strings only, or only `LfgMessage*` but not `LfgOption*`) would leave a confusing split between old and new naming.

**Update archive specs only in the main spec.** The delta spec updates `openspec/specs/lfg-message-constructor/spec.md`. Archived change specs under `openspec/changes/archive/` are historical snapshots and should not be retroactively modified.

## Risks / Trade-offs

- [Low] Technical reference and card-display-design docs contain "LFG" — update those too so documentation is consistent → covered by tasks
- [Low] The `relic-view-modes` spec references "LFG message composer" — update that reference to stay consistent → covered by tasks
- [Accepted, pre-V1] Renaming `SettingsFile.LfgTemplate` to `RecruitingMessageTemplate` changes the JSON key; existing `user-settings.json` files will lose their saved template and fall back to the in-code default. This is acceptable before V1 and consistent with the existing pre-V1 note in the technical reference.
