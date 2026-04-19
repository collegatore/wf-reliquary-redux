## Context

WF Reliquary Redux currently has a root .NET 10 solution, an Avalonia
presentation project, application and infrastructure class libraries, and NUnit
test projects. The README documents `dotnet restore`, `dotnet build`, and
`dotnet test`, but there are no checked-in VS Code workflow files. Contributors
using VS Code must either remember commands manually or create private task and
launch settings.

This change adds editor workflow configuration only. It must preserve the
existing CLI workflow as the source of truth and avoid changing application
runtime behavior.

## Goals / Non-Goals

**Goals:**

- Provide checked-in VS Code tasks for restoring, building, testing, and running
  the current solution.
- Provide a VS Code debug launch configuration for the Avalonia presentation
  project.
- Keep commands cross-platform by using `dotnet` directly and repository-root
  relative paths.
- Update README guidance so contributors can find the editor workflow.
- Validate the expected VS Code files and command labels.

**Non-Goals:**

- Do not add domain behavior, parser code, persistence schema, or UI workflows.
- Do not replace the command-line validation flow.
- Do not introduce new runtime dependencies.
- Do not require Visual Studio-specific files or platform-specific scripts.

## Decisions

### Decision: Use checked-in `.vscode/tasks.json`

Define workspace tasks for:

- `dotnet: restore` using `dotnet restore`.
- `dotnet: build` using `dotnet build`.
- `dotnet: test` using `dotnet test`.
- `dotnet: run presentation` using `dotnet run --project src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj`.

Rationale: these commands mirror the existing README and solution/project
layout. Using `dotnet` directly keeps the workflow portable between Windows and
Linux developer machines.

Alternative considered: use PowerShell or shell-specific wrapper scripts. This
was rejected because the baseline requires standard .NET CLI workflows across
Windows and Linux.

### Decision: Make build the default build task and test a test task

The build task should use the VS Code `build` group with `isDefault: true`.
The test task should use the `test` group with `isDefault: true`.

Rationale: this lets contributors use VS Code's built-in Run Build Task and Run
Test Task commands without memorizing project-specific labels.

Alternative considered: leave every task ungrouped. This was rejected because
it makes common workflows less discoverable.

### Decision: Use checked-in `.vscode/launch.json` for presentation launch and debugging

Add `.NET` launch configurations that build first and start the compiled
presentation assembly from the Debug `net10.0` output path:

`src/Presentation/WF.Reliquary.Presentation/bin/Debug/net10.0/WF.Reliquary.Presentation.dll`

The launch configurations should set `cwd` to the presentation project folder
and use the build task as `preLaunchTask`. One configuration should attach the
debugger, and one should set `noDebug: true` for a normal launch.

Rationale: Avalonia is the runnable desktop entry point today. Debugging the
compiled DLL after a build works with standard C# VS Code debugging and avoids
new scripts.

Alternative considered: debug through `dotnet run`. This was rejected for the
initial configuration because launch configurations that point at the compiled
assembly are the common C# extension shape and pair cleanly with a pre-launch
build task.

### Decision: Validate configuration as repository artifacts

Add integration-test coverage or equivalent validation that inspects the VS
Code JSON files for required task labels, commands, launch name, target project,
and pre-launch build wiring.

Rationale: the existing integration tests already validate contributor-facing
guidance and project wiring. Configuration drift should be caught before a
contributor discovers the workflow is broken.

Alternative considered: rely on manual review only. This was rejected because
the task labels and paths are easy to accidentally drift.

## Risks / Trade-offs

- VS Code C# extension behavior can vary by installed extension version ->
  Mitigation: keep launch configuration minimal and based on standard `coreclr`
  fields plus `dotnet`/compiled assembly conventions.
- Launch assembly path depends on Debug build output -> Mitigation: use the build
  task as `preLaunchTask` and target the current `net10.0` framework.
- JSON comments are not universally accepted outside VS Code -> Mitigation:
  keep `.vscode/*.json` strict JSON without comments so tests and external
  parsers can read it.
- Editor workflow could diverge from README commands -> Mitigation: update
  README and add validation for task labels and command paths.
