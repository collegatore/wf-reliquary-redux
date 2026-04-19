# Research: Application Layer Boilerplate

## Decision: Target .NET 10 (`net10.0`)

**Rationale**: .NET 10 is the current long-term support release and the best
starting point for a new cross-platform desktop foundation. Microsoft lists
.NET 10 as active LTS support through November 14, 2028.

**Source**:
- Microsoft .NET support policy: https://dotnet.microsoft.com/en-us/platform/support/policy

**Alternatives considered**:
- .NET 8 LTS: supported but closer to end of support for a new project base.
- .NET 9 STS: shorter support window and less suitable for a fresh foundation.

## Decision: Use Avalonia 12 for the presentation project

**Rationale**: The constitution and README approve Avalonia for desktop UI.
Avalonia documentation currently covers Avalonia 12, making it the appropriate
target for new boilerplate. The presentation project will stay minimal and avoid
product workflows.

**Sources**:
- Avalonia documentation: https://docs.avaloniaui.net/docs/welcome
- Avalonia 12 breaking changes: https://docs.avaloniaui.net/docs/avalonia12-breaking-changes

**Alternatives considered**:
- No presentation project: rejected because the feature explicitly asks for all
  application layers and the constitution names presentation as a layer.
- A different UI framework: rejected because it would deviate from the approved
  stack.

## Decision: Include Microsoft.Data.Sqlite as the infrastructure storage dependency

**Rationale**: SQLite is the approved local storage engine. Microsoft.Data.Sqlite
is the lightweight ADO.NET provider documented by Microsoft and is appropriate
for a future infrastructure adapter. This feature will not create a schema or
runtime database file.

**Source**:
- Microsoft.Data.Sqlite overview: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/

**Alternatives considered**:
- Entity Framework Core SQLite: deferred because the boilerplate does not yet
  need object-relational mapping or migrations.
- No storage dependency: rejected because the infrastructure layer should prove
  the approved persistence stack can be referenced later.

## Decision: Use NUnit 4 for test projects

**Rationale**: The constitution requires NUnit. NUnit documentation is current,
and NUnit 4 aligns with the project's forward-looking .NET 10 baseline.

**Source**:
- NUnit documentation: https://docs.nunit.org/

**Alternatives considered**:
- xUnit or MSTest: rejected because the constitution requires NUnit.
- Test projects without assertions: rejected because the boilerplate needs at
  least one meaningful placeholder behavior test.

## Decision: Use one solution with five projects

**Rationale**: A root solution gives contributors one restore/build/test entry
point. Separate projects for Presentation, Application, Infrastructure, Unit
Tests, and Integration Tests make layer boundaries visible and testable.

**Alternatives considered**:
- Single project with folders: rejected because project references would not
  enforce dependency direction.
- Separate solutions per layer: rejected because it complicates a small
  boilerplate and contributor workflow.

## Decision: Application owns abstractions; infrastructure implements adapters later

**Rationale**: To preserve layer separation, the application layer should remain
independent of persistence and UI implementation details. Future infrastructure
code can reference application abstractions without the application layer
referencing infrastructure.

**Alternatives considered**:
- Application references infrastructure directly: rejected because it leaks
  storage details into core logic.
- Presentation references infrastructure directly: rejected because it bypasses
  application orchestration and weakens testability.

## Decision: README is the contributor workflow source of truth

**Rationale**: The constitution identifies README as the canonical entry point.
This feature changes contributor setup and validation, so README must document
the layer layout and commands verified by quickstart.

**Alternatives considered**:
- Store commands only in quickstart: rejected because contributors naturally
  start at README.
- Delay README until domain features exist: rejected because the boilerplate
  changes repository use immediately.
