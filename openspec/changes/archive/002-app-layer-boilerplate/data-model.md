# Data Model: Application Layer Boilerplate

## Overview

This feature's "data model" describes the repository structure and validation
objects created by the boilerplate. It does not introduce product domain data.

## Entity: Solution Workspace

Represents the root build and validation boundary for all boilerplate projects.

Fields:

- `Name`: solution display name.
- `SolutionPath`: root solution file path.
- `Projects`: included presentation, application, infrastructure, and test
  projects.
- `ValidationCommands`: restore, build, and test commands documented for
  contributors.

Relationships:

- Contains all boilerplate projects.
- Is documented by Contributor Workflow.

Validation rules:

- Must be located at the repository root.
- Must include all required source and test projects.
- Must restore, build, and test successfully from command-line workflows.

## Entity: Presentation Layer

Represents the future desktop UI boundary.

Fields:

- `ProjectPath`: presentation project path.
- `Purpose`: UI composition and user interaction.
- `AllowedReferences`: application layer only, plus framework dependencies.

Relationships:

- May reference Application Layer.
- Must not own persistence or business rules.

Validation rules:

- Must exist as a distinct project or project group.
- Must not contain domain parsing, query, or persistence behavior.

## Entity: Application Layer

Represents the future core business-logic boundary.

Fields:

- `ProjectPath`: application project path.
- `Purpose`: orchestration, parsing abstractions, normalization, query behavior,
  and business rules.
- `AllowedReferences`: platform/runtime dependencies only; no presentation or
  infrastructure project references.
- `PlaceholderBehavior`: minimal behavior used to prove unit test wiring.

Relationships:

- May be referenced by Presentation Layer.
- May be referenced by Infrastructure Layer.
- Is validated by Unit Test Layer.

Validation rules:

- Must not reference Presentation Layer.
- Must not reference Infrastructure Layer.
- Must include at least one placeholder behavior covered by NUnit.

## Entity: Infrastructure Layer

Represents future file, database, and external data-source adapters.

Fields:

- `ProjectPath`: infrastructure project path.
- `Purpose`: persistence, file access, and external-source implementation
  details.
- `AllowedReferences`: application layer and infrastructure dependencies.
- `StorageDependency`: SQLite provider dependency placeholder.

Relationships:

- May reference Application Layer to implement application-defined abstractions.
- Is validated by Integration Test Layer.

Validation rules:

- Must not be referenced by Application Layer.
- Must not create a runtime database or schema in this feature.

## Entity: Unit Test Layer

Represents fast validation for application behavior.

Fields:

- `ProjectPath`: unit test project path.
- `Framework`: NUnit.
- `CoveredProject`: application project.

Relationships:

- References Application Layer.

Validation rules:

- Must contain at least one passing test for application-layer placeholder
  behavior.
- Must run through the documented test command.

## Entity: Integration Test Layer

Represents future cross-layer validation space.

Fields:

- `ProjectPath`: integration test project path.
- `Framework`: NUnit.
- `CoveredProjects`: infrastructure and application projects as needed.

Relationships:

- May reference Application Layer and Infrastructure Layer.

Validation rules:

- Must exist as a separate test project or clearly separated integration test
  group.
- Must not require an external service or runtime database to pass in this
  boilerplate feature.

## Entity: Contributor Workflow

Represents documented setup and validation guidance.

Fields:

- `ReadmePath`: README location.
- `RestoreCommand`: command for dependency restore.
- `BuildCommand`: command for solution build.
- `TestCommand`: command for test validation.
- `LayerLayoutSummary`: concise explanation of project responsibilities.

Relationships:

- Documents Solution Workspace and all layer projects.

Validation rules:

- README must include restore/build guidance, test guidance, and layer layout.
- Commands must be practical from VS Code terminals on Windows and Linux.

## State and Lifecycle

Boilerplate lifecycle states:

1. `Created`: solution and layer projects exist.
2. `Referenced`: project references express intended dependency direction.
3. `Validated`: restore, build, and test commands pass.
4. `Documented`: README explains setup, validation, and layer responsibilities.
