## 1. Update Presentation project

- [x] 1.1 Bump `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter` from `12.0.1` to `12.0.5` in `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj`
- [x] 1.2 Bump `CommunityToolkit.Mvvm` from `8.4.0` to `8.4.2` in `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj`
- [x] 1.3 Bump `AsyncAwaitBestPractices` from `9.0.0` to `10.0.0` in `src/Presentation/WF.Reliquary.Presentation/WF.Reliquary.Presentation.csproj`

## 2. Update test projects

- [x] 2.1 Bump `NUnit` from `4.5.1` to `4.6.1` in `tests/Unit/WF.Reliquary.UnitTests/WF.Reliquary.UnitTests.csproj`
- [x] 2.2 Bump `Microsoft.NET.Test.Sdk` from `18.4.0` to `18.7.0` in `tests/Unit/WF.Reliquary.UnitTests/WF.Reliquary.UnitTests.csproj`
- [x] 2.3 Bump `NUnit` from `4.5.1` to `4.6.1` in `tests/Integration/WF.Reliquary.IntegrationTests/WF.Reliquary.IntegrationTests.csproj`
- [x] 2.4 Bump `Microsoft.NET.Test.Sdk` from `18.4.0` to `18.7.0` in `tests/Integration/WF.Reliquary.IntegrationTests/WF.Reliquary.IntegrationTests.csproj`

## 3. Verify

- [x] 3.1 Run `dotnet restore` and confirm no restore errors
- [x] 3.2 Run `dotnet build` and confirm clean build (no errors, no new warnings)
- [x] 3.3 Run `dotnet test` and confirm all tests pass
