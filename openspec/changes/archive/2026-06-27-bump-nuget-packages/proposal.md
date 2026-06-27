## Why

All external NuGet packages have been audited against nuget.org and the GitHub Advisory Database. No vulnerabilities were found, but 8 packages across 3 projects have newer versions available. Keeping dependencies current reduces accumulation of patch-level bugs and future upgrade friction.

## What Changes

- `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`: 12.0.1 → 12.0.5 (4 patch releases)
- `CommunityToolkit.Mvvm`: 8.4.0 → 8.4.2 (2 patch releases)
- `AsyncAwaitBestPractices`: 9.0.0 → 10.0.0 (only change: adds .NET 10 TFM)
- `NUnit`: 4.5.1 → 4.6.1 (minor; partial-breaking delegate change does not apply on net10.0)
- `Microsoft.NET.Test.Sdk`: 18.4.0 → 18.7.0 (minor)

## Capabilities

### New Capabilities

None.

### Modified Capabilities

None. This change contains no requirement or behavior changes — only dependency version numbers in `.csproj` files.

## Impact

- `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj` — Avalonia ×4, CommunityToolkit.Mvvm, AsyncAwaitBestPractices
- `tests/Unit/WF.Reliquary.UnitTests/WF.Reliquary.UnitTests.csproj` — NUnit, Microsoft.NET.Test.Sdk
- `tests/Integration/WF.Reliquary.IntegrationTests/WF.Reliquary.IntegrationTests.csproj` — NUnit, Microsoft.NET.Test.Sdk
