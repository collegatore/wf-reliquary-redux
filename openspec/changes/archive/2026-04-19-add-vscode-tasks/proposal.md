## Why

The project already validates cleanly through .NET CLI commands, but VS Code
contributors do not yet have checked-in tasks or launch settings for the common
build, test, run, and debug workflows. Adding these settings makes the approved
CLI workflow discoverable from the editor without changing application behavior.

## What Changes

- Add workspace VS Code task definitions for restore, build, test, and running
  the Avalonia presentation project.
- Add VS Code launch configuration for debugging the presentation project.
- Keep task commands rooted in the existing .NET 10 solution/project layout.
- Document the VS Code workflow in README so command-line and editor entry
  points stay aligned.
- Add validation coverage or review checks that protect the expected VS Code
  files and commands from drifting.

## Capabilities

### New Capabilities

- `vscode-workflows`: Contributor-facing VS Code build, test, run, and debug
  workflow definitions for the existing .NET/Avalonia solution.

### Modified Capabilities

- None.

## Impact

- Adds `.vscode/tasks.json` and `.vscode/launch.json`.
- Updates `README.md` with VS Code workflow guidance.
- May add or extend integration tests that inspect workspace configuration
  files.
- Does not change application runtime behavior, project references, data
  schema, or domain logic.
