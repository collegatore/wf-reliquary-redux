## 1. Publish profiles

- [ ] 1.1 Create `Properties/PublishProfiles/win-x64-self-contained.pubxml` (Release, win-x64, self-contained, single-file, native libs for self-extract, no trimming)
- [ ] 1.2 Create `Properties/PublishProfiles/win-x64-framework-dependent.pubxml` (Release, win-x64, framework-dependent, no trimming)
- [ ] 1.3 Verify both locally: `dotnet publish -p:PublishProfile=<name>`, launch the self-contained exe, confirm the framework-dependent output is substantially smaller

## 2. CI workflow

- [ ] 2.1 Create `.github/workflows/ci.yml`: Windows runner, .NET 10 setup, `dotnet build` + `dotnet test` on push/PR to `develop` and `main`
- [ ] 2.2 Push to a branch and confirm the workflow runs and goes green on GitHub

## 3. Release workflow

- [ ] 3.1 Create `.github/workflows/release.yml` triggered by `v*` tags with `permissions: contents: write`
- [ ] 3.2 Implement tag ↔ `Directory.Build.props` version verification (fail on mismatch)
- [ ] 3.3 Implement build + full test run, publish of both profiles, and zip packaging named `WF.Reliquary.Redux-<version>-win-x64[-self-contained].zip`
- [ ] 3.4 Implement CHANGELOG section extraction for the tagged version as the release body (fail if the section is missing) and GitHub Release creation with both zips attached
- [ ] 3.5 Dry-run the pipeline with a pre-release tag (e.g. `v1.0.0-alpha` on a test commit or a scratch tag), verify the created release, then clean up if it was a scratch run

## 4. Documentation

- [ ] 4.1 Add the README download/install section: artifact flavors and how to choose, .NET 10 desktop runtime link for the framework-dependent flavor, SmartScreen warning walkthrough, Windows-only platform note (Linux planned, not yet built/tested)
- [ ] 4.2 Mention the SmartScreen expectation in the User Guide's getting-started area (from `prepare-alpha-release`)
- [ ] 4.3 Document the release procedure (bump `Directory.Build.props`, update CHANGELOG, tag `v<version>` on `main`) and the local publish commands in `docs/technical-reference.md`

## 5. Verification

- [ ] 5.1 Download both artifacts from the (dry-run) GitHub Release on a clean environment; launch the self-contained exe without .NET installed and the framework-dependent build with the runtime present
- [ ] 5.2 Confirm the release body carries the CHANGELOG section including the data-wipe notice and the Windows-only note
