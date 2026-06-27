## 1. Upgrade EF Core to 10.0.0 (fixes CVE)

- [x] 1.1 In `src/Infrastructure/WF.Reliquary.Infrastructure/WF.Reliquary.Infrastructure.csproj`, bump `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` from `9.0.6` to `10.0.9`
- [x] 1.2 Run `dotnet restore` and confirm all four `SQLitePCLRaw.*` packages resolve to 2.1.11 across all projects

## 2. Fix CS8601 — Null Assignment in LocalDropTableSourceStore

- [x] 2.1 In `src/Infrastructure/WF.Reliquary.Infrastructure/DropTables/LocalDropTableSourceStore.cs` line 76, change `content.SourceUrl?.ToString()` to `content.SourceUrl?.ToString() ?? string.Empty`

## 3. Fix CS8602 — Null Dereference in MainWindowViewModel Constructor

- [x] 3.1 In `src/Presentation/WF.Reliquary.Presentation/MainWindowViewModel.cs`, move the five `AsyncRelayCommand` field initializations (currently lines 65–69) to run before the `BusyScope` constructor call (currently lines 55–63)

## 4. Verify

- [x] 4.1 Run `dotnet build` and confirm output shows **0 warnings, 0 errors**
- [x] 4.2 Run `dotnet test` and confirm all tests pass
