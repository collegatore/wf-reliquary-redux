# WF Reliquary Redux Technology Stack

## Overview

WF Reliquary Redux should be implemented as a cross-platform desktop application using C# for the core application logic and SQLite for local data storage. The project must be buildable and runnable from Visual Studio Code on both Windows and Linux.

The GUI framework for the project is Avalonia. It supports the required cross-platform desktop delivery, fits well with a C# application stack, and works cleanly in a VS Code based workflow on both target operating systems.

## Core Technology Decisions

- Programming language: C#
- Runtime and SDK: .NET
- Primary data storage: SQLite
- Development environment: Visual Studio Code
- Target operating systems: Windows and Linux
- GUI framework: Avalonia

## Application Layers

### Presentation Layer

The presentation layer should be a desktop GUI implemented with Avalonia on top of C# and .NET. The presentation stack should support:

- Desktop application development for Windows and Linux
- Smooth integration with .NET tooling
- Productive development from Visual Studio Code
- A maintainable UI architecture for search, navigation, and results views

### Application Layer

The application layer should be implemented in C# and contain the core business logic for:

- Loading publicly available Warframe drop table data
- Parsing and normalizing relic, Prime Part, mission, and rotation data
- Building acquisition paths from Prime Parts to relics to missions
- Querying stored data for presentation to the user

### Data Storage Layer

The data storage layer should use SQLite as the primary local database. SQLite is a strong fit for the project because it is lightweight, cross-platform, easy to distribute with a desktop app, and well suited to structured relational data such as Prime Parts, relics, missions, rotations, and drop chances.

The storage model should be relational, using SQL tables for normalized game data and relationships. If needed, raw imported source payloads may also be stored for traceability or debugging.

## Build and Tooling Expectations

- The project should use the .NET SDK and standard CLI tooling.
- The codebase should be buildable from the terminal in Visual Studio Code.
- The build workflow should work on both Windows and Linux without requiring Visual Studio.
- Project structure and dependencies should remain compatible with cross-platform development and local SQLite usage.

## Technology Selection Guidance

The stack should be designed so that the data loading, processing, and storage layers remain independent from the Avalonia presentation layer. This keeps the core application portable and leaves room for future UI changes without forcing a redesign of the domain or persistence model.

## Recommended Direction

The current recommended stack for the project is:

- C# with .NET for application development
- SQLite for local relational storage
- Visual Studio Code as the primary development environment
- Windows and Linux as first-class target platforms
- Avalonia as the cross-platform C# GUI framework
