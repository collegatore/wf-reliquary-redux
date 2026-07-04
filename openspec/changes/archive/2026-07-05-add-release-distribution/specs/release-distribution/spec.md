## ADDED Requirements

### Requirement: Repository-Defined Publish Profiles

The repository SHALL contain two publish profiles under `src/Presentation/WF.Reliquary.Presentation/Properties/PublishProfiles/`: a **self-contained** win-x64 profile producing a single-file executable that runs without any .NET installation (native libraries included in the file), and a **framework-dependent** win-x64 profile producing a small deployment that requires the .NET 10 desktop runtime. Neither profile SHALL enable assembly trimming. Both SHALL be invocable locally via `dotnet publish -p:PublishProfile=<name>`.

#### Scenario: Self-contained publish runs without installed runtime

- **WHEN** the self-contained profile is published and the output is copied to a Windows x64 machine without any .NET runtime installed
- **THEN** the application launches from the produced executable

#### Scenario: Framework-dependent publish is runtime-bound

- **WHEN** the framework-dependent profile is published
- **THEN** the output is significantly smaller than the self-contained output and launches on a machine with the .NET 10 desktop runtime installed

### Requirement: Continuous Integration on Push and Pull Request

The repository SHALL contain a GitHub Actions workflow that builds the solution and runs the full test suite (unit and integration) on a Windows runner for every push and pull request targeting `develop` or `main`. A build or test failure SHALL fail the workflow run.

#### Scenario: Push triggers build and tests

- **WHEN** a commit is pushed to `develop` or `main`, or a pull request targets either branch
- **THEN** the CI workflow builds the solution and runs all tests, and reports failure if any test fails

### Requirement: Tag-Triggered Release Pipeline

The repository SHALL contain a GitHub Actions release workflow triggered by pushing a tag matching `v*`. The workflow SHALL: verify that the tag version equals the `<Version>` in `Directory.Build.props` and fail otherwise; build and run the full test suite; publish both publish profiles; package each executable as an exe-only zip whose name contains the version and flavor; package the debug symbols (`.pdb`) as a separate `-symbols` zip; and create a GitHub Release for the tag with all three zips attached and the matching `CHANGELOG.md` version section as the release body. Because the symbols are identical across both builds, a single symbols package SHALL cover both; the workflow SHALL fail if the two builds' symbols are not identical. The workflow SHALL fail if the changelog section for the version cannot be found. The workflow SHALL run with the built-in `GITHUB_TOKEN` (`permissions: contents: write`) and SHALL require no repository configuration beyond the committed workflow files.

#### Scenario: Matching tag produces a release

- **WHEN** a tag `v<X>` is pushed and `<X>` equals the version in `Directory.Build.props`
- **THEN** a GitHub Release for the tag is created with the self-contained exe zip, the framework-dependent exe zip, and the `-symbols` zip attached, and the `CHANGELOG.md` section for `<X>` as its body

#### Scenario: End-user zips carry the exe without debug symbols

- **WHEN** either end-user zip (self-contained or framework-dependent) is opened
- **THEN** it contains the application executable and no `.pdb` files, and the `.pdb` symbols are instead found in the separate `-symbols` zip

#### Scenario: Mismatched tag fails fast

- **WHEN** a tag is pushed whose version does not equal the version in `Directory.Build.props`
- **THEN** the release workflow fails before publishing anything and no GitHub Release is created

#### Scenario: Missing changelog section blocks the release

- **WHEN** a matching tag is pushed but `CHANGELOG.md` has no section for that version
- **THEN** the release workflow fails and no GitHub Release is created

### Requirement: End-User Download Documentation

The README SHALL contain a download/install section for end users that: explains the two artifact flavors and which to choose (self-contained needs nothing installed; framework-dependent is smaller but requires the .NET 10 desktop runtime, with a link to its official download); states that the executable is unsigned and that Windows SmartScreen will warn on first launch, with the steps to proceed (More info → Run anyway); and states that Windows x64 is the only supported platform at this release, with Linux support planned but not yet built or tested.

#### Scenario: User picks an artifact

- **WHEN** a user reads the README download section
- **THEN** they can determine which zip to download based on whether they have (or want to install) the .NET 10 desktop runtime

#### Scenario: SmartScreen warning is expected

- **WHEN** a user consults the README after Windows blocks the first launch
- **THEN** the documented steps (More info → Run anyway) let them proceed, and the warning is explained as the consequence of an unsigned pre-release executable

#### Scenario: Platform scope is unambiguous

- **WHEN** a Linux user reads the README download section
- **THEN** they learn that no Linux build is provided yet and that Linux support is planned but untested
