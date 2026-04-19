## 1. VS Code Configuration

- [x] 1.1 Create `.vscode/tasks.json` with restore, default build, default test, and presentation run tasks using repository-root `dotnet` commands.
- [x] 1.2 Create `.vscode/launch.json` with a presentation debug configuration that uses the build task as `preLaunchTask`, targets `net10.0`, and sets the presentation project as the working directory.
- [x] 1.3 Add a `.vscode/launch.json` presentation launch configuration that uses `noDebug: true` for running without the debugger.

## 2. Validation Coverage

- [x] 2.1 Add or extend integration test helpers so tests can read and parse strict JSON workspace files from `.vscode/`.
- [x] 2.2 Add integration tests that verify required task labels, commands, groups, and presentation project path in `.vscode/tasks.json`.
- [x] 2.3 Add integration tests that verify launch name, debug type, pre-launch task, program path, and working directory in `.vscode/launch.json`.
- [x] 2.4 Add integration tests that verify the non-debug presentation launch configuration.

## 3. Documentation

- [x] 3.1 Update `README.md` to describe the VS Code restore, build, test, run, and debug workflows alongside the existing CLI commands.
- [x] 3.2 Update README validation tests if needed so contributor-facing VS Code guidance is protected.

## 4. Verification

- [x] 4.1 Run `dotnet test` from the repository root and resolve any validation issues.
- [x] 4.2 Run `openspec validate add-vscode-tasks --type change --strict` and resolve any proposal/spec/task issues.
- [x] 4.3 Review the final change to confirm no application runtime behavior, domain logic, project references, or storage schema were changed.
