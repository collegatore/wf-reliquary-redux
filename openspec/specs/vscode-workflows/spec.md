# vscode-workflows Specification

## Purpose
TBD - created by archiving change add-vscode-tasks. Update Purpose after archive.
## Requirements
### Requirement: VS Code Build and Restore Tasks

The repository SHALL provide checked-in VS Code tasks that allow contributors
to restore and build the existing solution from the repository root using the
approved .NET CLI workflow.

#### Scenario: Contributor restores from VS Code
- **WHEN** a contributor runs the VS Code restore task
- **THEN** the task executes `dotnet restore` from the repository root

#### Scenario: Contributor builds from VS Code
- **WHEN** a contributor runs the default VS Code build task
- **THEN** the task executes `dotnet build` from the repository root

### Requirement: VS Code Test Task

The repository SHALL provide a checked-in VS Code test task that runs the
existing NUnit unit and integration test projects through the root .NET
solution workflow.

#### Scenario: Contributor tests from VS Code
- **WHEN** a contributor runs the default VS Code test task
- **THEN** the task executes `dotnet test` from the repository root

### Requirement: VS Code Run Task

The repository SHALL provide a checked-in VS Code task that runs the current
Avalonia presentation project without adding product workflows or bypassing the
existing project layout.

#### Scenario: Contributor runs presentation shell from VS Code
- **WHEN** a contributor runs the VS Code presentation run task
- **THEN** the task executes `dotnet run --project` for
  `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj`

### Requirement: VS Code Debug Configuration

The repository SHALL provide a checked-in VS Code launch configuration that
debugs the Avalonia presentation project and builds the solution before launch.

#### Scenario: Contributor starts debugger
- **WHEN** a contributor starts the VS Code presentation debug configuration
- **THEN** VS Code builds the solution first and launches the presentation
  project output for `net10.0`

### Requirement: VS Code Launch Without Debug Configuration

The repository SHALL provide a checked-in VS Code launch configuration that
runs the Avalonia presentation project without attaching the debugger while
still building the solution before launch.

#### Scenario: Contributor starts without debugger
- **WHEN** a contributor starts the VS Code presentation launch configuration
- **THEN** VS Code builds the solution first and launches the presentation
  project output for `net10.0` with debugging disabled

### Requirement: VS Code Workflow Documentation

README guidance SHALL describe the available VS Code build, test, run, and
debug workflow alongside the existing command-line validation commands.
It SHALL also distinguish the non-debug launch configuration from the debug
configuration.

#### Scenario: Contributor reads README for editor workflow
- **WHEN** a contributor looks for VS Code usage guidance
- **THEN** the README identifies the checked-in tasks and launch configuration
  for build, test, run, and debug

### Requirement: VS Code Workflow Validation

The repository SHALL validate the checked-in VS Code workflow files so required
task labels, commands, project paths, launch target, and pre-launch build wiring
remain aligned with the current solution layout.

#### Scenario: VS Code workflow files drift
- **WHEN** validation tests inspect `.vscode/tasks.json` and
  `.vscode/launch.json`
- **THEN** missing or incorrect build, test, run, or debug configuration is
  reported as a test failure

