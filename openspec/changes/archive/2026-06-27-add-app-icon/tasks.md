## 1. Prepare Icon Assets

- [x] 1.1 Create `src/Presentation/WF.Reliquary.Presentation/Assets/` directory
- [x] 1.2 Copy `D:\Downloads_stg\icon-full-size.png` into `Assets/icon-full-size.png` (keep original for reference)
- [x] 1.3 Use a PowerShell script (System.Drawing) to resize `icon-full-size.png` to 256×256 and save as `Assets/icon.png`
- [x] 1.4 Use the same PowerShell script to produce `Assets/icon.ico` containing four resolutions: 256×256, 48×48, 32×32, and 16×16 from the source PNG

## 2. Wire Up the Executable Icon

- [x] 2.1 Add `<ApplicationIcon>Assets\icon.ico</ApplicationIcon>` to the first `<PropertyGroup>` in `WF.Reliquary.Presentation.csproj`
- [x] 2.2 Add `<AvaloniaResource Include="Assets\icon.png" />` and `<None Include="Assets\icon.ico" />` item groups to the `.csproj`

## 3. Wire Up the Runtime Window Icon

- [x] 3.1 Add `Icon="avares://WF.Reliquary.Presentation/Assets/icon.png"` to the `<Window>` element in `MainWindow.axaml`

## 4. Verify

- [x] 4.1 Build the project (`dotnet build`) and confirm it succeeds with no warnings about missing icons
- [x] 4.2 Confirm the built `.exe` shows the branded icon in Windows Explorer (right-click the `.exe` in the `bin/` output folder)
