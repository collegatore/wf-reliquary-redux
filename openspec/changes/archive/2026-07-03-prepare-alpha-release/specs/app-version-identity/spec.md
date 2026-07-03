## ADDED Requirements

### Requirement: Single-Source Version Metadata

The solution SHALL define the application version, product name, authors, copyright, and repository URL exactly once, in a repo-root `Directory.Build.props` applied to every project. `ApplicationInfo.Version` SHALL be derived from the compiled assembly's informational version (with any `+<metadata>` suffix trimmed), not from a hand-maintained constant. Releasing a new version SHALL require editing only the `Directory.Build.props` value.

#### Scenario: Version is bumped in one place

- **WHEN** the `<Version>` property in `Directory.Build.props` is changed and the solution is rebuilt
- **THEN** `ApplicationInfo.Version`, the compiled assemblies' informational versions, and every version surface in the app all report the new value with no other edits

#### Scenario: Informational version carries build metadata

- **WHEN** the assembly informational version contains a `+<metadata>` suffix (e.g. a commit hash)
- **THEN** `ApplicationInfo.Version` reports the version without the suffix

### Requirement: Version Visible in the Window Title

The main window title SHALL display the application name followed by the version (e.g. `WF Reliquary Redux 1.0.0-alpha`).

#### Scenario: Title shows name and version

- **WHEN** the main window is shown
- **THEN** its title is the application name followed by the current application version

### Requirement: Version Logged at Startup

The application SHALL append a startup entry containing the application name and version to the application log (`app.log`) each time it starts, so any log submitted with a bug report identifies the build that produced it.

#### Scenario: Startup entry written

- **WHEN** the application starts
- **THEN** an entry containing the application name and version is appended to `app.log`
