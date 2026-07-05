## MODIFIED Requirements

### Requirement: Changelog with Release Notes

The repository SHALL contain a `CHANGELOG.md` at the root with a section per released version, newest first. Any release section published while Windows x64 is the only supported platform SHALL state that Windows x64 is the only supported platform and that Linux support is planned but has not yet been built or tested.

#### Scenario: Platform scope stated for 1.0.0-alpha

- **WHEN** a reader opens `CHANGELOG.md`
- **THEN** the `1.0.0-alpha` section states that only Windows x64 is supported and Linux support is planned but not yet delivered

#### Scenario: Platform scope stated for 1.0.0

- **WHEN** a reader opens `CHANGELOG.md`
- **THEN** the `1.0.0` section states that only Windows x64 is supported and Linux support is planned but not yet delivered
