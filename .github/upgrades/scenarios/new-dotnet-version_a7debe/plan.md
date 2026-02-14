# .NET 10.0 Upgrade Plan

Table of contents
- Executive Summary
- Migration Strategy
- Detailed Dependency Analysis
- Project-by-Project Plans
- Package Update Reference
- Breaking Changes Catalog
- Testing Strategy
- Risk Management
- Complexity & Effort Assessment
- Source Control Strategy
- Success Criteria

---

## 1. Executive Summary
This plan upgrades the solution from .NET 8 to **.NET 10.0 (Long Term Support)** using the **All-At-Once Strategy**.

Scope:
- Projects: 14 projects (including 6 test projects)
- Target framework: migrate all projects from `net8.0` / `net8.0-windows` to `net10.0` / `net10.0-windows`
- Key technologies: Blazor WebApp (ASP.NET Core), WPF desktop app

Assessment highlights:
- Total issues found: 79 (60 mandatory, 19 potential)
- Major risk area: WPF project (`CleanArchitecture.Presentation.Wpf`) with 55 issues (47 mandatory) — most are binary-incompatibility findings in generated `obj/` files
- NuGet packages requiring updates: EF Core and several Microsoft.Extensions packages (see §5 Package Update Reference)
- .NET 10 SDK: available on host; no `global.json` adjustments required

Rationale for All-At-Once:
- Solution size: 14 projects (small-to-medium) with clear dependency graph
- Majority of issues are mechanical (TFM changes, package updates) and auto-generated WPF artifacts will be regenerated after recompilation
- Blazor project impact is minimal and test coverage is present

Primary goal: perform an atomic, coordinated upgrade of all project TFMs and package references, rebuild the solution, and fix compilation/test failures in a single upgrade operation.

## 2. Migration Strategy
Selected Strategy: All-At-Once Strategy — All projects upgraded simultaneously in a single coordinated operation.

Justification:
- The solution is a small/medium monorepo (14 projects) with a well-defined dependency graph. All projects are currently targeting `net8.0` variants and have compatible package upgrade paths to `net10.0`.
- The WPF project shows many issues, but they originate largely from auto-generated `obj/` files and are expected to be resolved by recompilation with the new SDK. Only a few source files reference WPF APIs directly.

Strategy specifics (All-At-Once):
- Update TargetFramework properties in all project files to `net10.0` or `net10.0-windows` (for WPF and any windows-targeting projects).
- Update recommended NuGet packages across all projects to the versions suggested in the assessment (see §5).
- Restore dependencies and build the entire solution; address compilation errors discovered in this single pass.
- Run the full test suite and address failing tests as part of the same upgrade operation.

Phases (for clarity; the upgrade remains a single atomic operation):
- Phase 0: Preparation (SDK validation, branch setup already done)
- Phase 1: Atomic Upgrade (project TFMs + package updates + restore + build + fix compilation errors)
- Phase 2: Test Validation (run unit/integration tests, fix failing tests)
- Phase 3: Final Verification (ensure no vulnerabilities, update documentation)

Source control model: single upgrade branch `upgrade-to-NET10` contains the entire atomic change set. Prefer a single logical commit capturing the coordinated changes where feasible (see §10 Source Control Strategy).

## 3. Detailed Dependency Analysis
Summary of dependency analysis (topology levels):

- Level 0 (Foundation): `CleanArchitecture.Contracts`, `CleanArchitecture.Domain`
- Level 1: `CleanArchitecture.Application`, `CleanArchitecture.Domain.UnitTests`
- Level 2: `CleanArchitecture.Application.UnitTests`, `CleanArchitecture.Infrastructure.EfCore.Sqlite`, `CleanArchitecture.Infrastructure.InMemory`
- Level 3: `CleanArchitecture.Infrastructure.Composition`, Integration tests
- Level 4: `CleanArchitecture.Presentation.BlazorWebApp`, `CleanArchitecture.Presentation.Wpf`
- Level 5: `CleanArchitecture.ArchitectureTests`, `CleanArchitecture.Presentation.Wpf.UnitTests`

Migration order rationale:
- All-At-Once requires updating all projects simultaneously. The dependency graph is provided to guide where attention is needed during compilation fixes (libraries first conceptually, but updates applied atomically).

Critical path:
- Foundation libraries (`Contracts`, `Domain`) must be correct after TFM change because many downstream projects depend on them. Expect to address API/compilation errors originating from these libraries early during the build-and-fix pass.

Circular dependencies:
- None detected in the assessment output.

Notes on special project kinds:
- `CleanArchitecture.Presentation.Wpf` (WPF): targets `net8.0-windows` → `net10.0-windows`. Many issues are in `obj/` generated code; expect regeneration after build.
- `CleanArchitecture.Presentation.BlazorWebApp` (Blazor): minimal issues; verify `Program.cs` usage of `UseExceptionHandler` and any middleware behavioral changes.

## 4. Project-by-Project Plans
This section contains per-project migration stubs. Each project will be upgraded as part of the atomic operation.

### Project list (all projects upgraded simultaneously)
- `CleanArchitecture.Contracts` (ClassLibrary)
- `CleanArchitecture.Domain` (ClassLibrary)
- `CleanArchitecture.Application` (ClassLibrary)
- `CleanArchitecture.Application.UnitTests` (Test)
- `CleanArchitecture.Domain.UnitTests` (Test)
- `CleanArchitecture.Application.UnitTests` (Test)
- `CleanArchitecture.Infrastructure.EfCore.Sqlite` (ClassLibrary)
- `CleanArchitecture.Infrastructure.EfCore.Sqlite.IntegrationTests` (Test)
- `CleanArchitecture.Infrastructure.InMemory` (ClassLibrary)
- `CleanArchitecture.Infrastructure.InMemory.IntegrationTests` (Test)
- `CleanArchitecture.Infrastructure.Composition` (ClassLibrary)
- `CleanArchitecture.Presentation.BlazorWebApp` (Blazor WebApp)
- `CleanArchitecture.Presentation.Wpf` (WPF App)
- `CleanArchitecture.Presentation.Wpf.UnitTests` (Test)

For each project, the following template applies (to be filled with specifics discovered during the build-and-fix pass):

```
Project: <name>
Current State: TargetFramework=<net8.0 or net8.0-windows>, SDK-style=<true|false>
Target State: TargetFramework=<net10.0 or net10.0-windows>

Migration Steps:
1. Update TargetFramework element in project file to `net10.0` or `net10.0-windows`.
2. Update PackageReferences per §5 Package Update Reference.
3. Restore dependencies.
4. Build solution; capture compilation errors.
5. Fix compilation errors (API changes, using directives, conditional compilation, platform-specific APIs).
6. Rebuild and verify project compiles without errors.

Validation:
- Project builds with 0 errors
- Unit tests (if any) run and pass
- No outstanding package vulnerabilities
```

## 5. Package Update Reference
The following packages were flagged by the assessment as recommended to upgrade. Apply these updates across affected projects during the atomic upgrade.

Common Package Updates (affecting multiple projects):

- `Microsoft.EntityFrameworkCore`: 8.0.8 → **10.0.3** (affects: `Infrastructure.EfCore.Sqlite`, integration tests)
- `Microsoft.EntityFrameworkCore.Sqlite`: 8.0.8 → **10.0.3** (affects: `Infrastructure.EfCore.Sqlite`, integration tests)
- `Microsoft.Extensions.Configuration`: 8.0.0 → **10.0.3** (affects multiple projects using configuration)
- `Microsoft.Extensions.Configuration.Binder`: 8.0.2 → **10.0.3**
- `Microsoft.Extensions.DependencyInjection`: 8.0.0 / 8.0.1 → **10.0.3**
- `Microsoft.Extensions.DependencyInjection.Abstractions`: 8.0.2 → **10.0.3**
- `Microsoft.Extensions.Hosting`: 8.0.0 → **10.0.3** (affects WPF project references)
- `Microsoft.Extensions.Hosting.Abstractions`: 8.0.0 → **10.0.3**
- `Microsoft.Extensions.Options.ConfigurationExtensions`: 8.0.0 → **10.0.3**

Notes:
- Where a package is used by multiple projects, update all references to the same target version to avoid binding/load mismatches.
- Test SDKs and test runners were marked as compatible and do not require updates unless CI indicates otherwise.


## 6. Breaking Changes Catalog
This catalog lists the notable breaking and behavioral changes flagged by the assessment. Use this as a checklist while fixing compilation and test failures.

1) WPF binary-incompatible API changes (Api.0001)
- Occurrences: many items in `obj/` generated code referencing `System.Windows.*` types and members.
- Impact: Generated code will be regenerated for the new target framework. Most items do not require manual code changes.
- Action: Rebuild solution after TFM/package updates and address any remaining compile errors in source files referencing WPF APIs (e.g., `TodosView.xaml.cs`, `RenameParamConverter.cs`).

2) `System.Uri` behavioral changes (Api.0003)
- Occurrences: resource locator URIs in generated files.
- Impact: URIs used in generated XAML loader calls may behave differently; validate runtime loading of XAML resources after upgrade.
- Action: Rebuild and run the WPF app; if resources fail to load, adjust pack URIs to new format (if required).

3) `UseExceptionHandler` overload behavioral change (Api.0003)
- Occurrences: `CleanArchitecture.Presentation.BlazorWebApp\Program.cs` uses `app.UseExceptionHandler("/Error", createScopeForErrors: true)`.
- Impact: Overload signature may have changed. Update call to match new overloads or extension method semantics.

4) EF Core major version (8 → 10)
- Occurrences: `Infrastructure.EfCore.Sqlite` and integration tests
- Impact: EF Core 10 may introduce API changes for migrations, query translation, or model builder behavior.
- Action: Rebuild and run integration tests; review EF usage for deprecated APIs and apply migration steps if necessary.

5) Microsoft.Extensions.* packages
- Occurrences: various hosting/DI/configuration packages
- Impact: Minor API/behavioral changes possible. Recompile and update call sites if necessary.

General guidance:
- Treat most issues as compilation-time discoveries. Resolve compiler errors by following the recommended API replacements or adjusting code.
- For runtime behavioral changes, rely on tests and targeted manual validation.

## 7. Testing Strategy
Testing will be performed after the atomic upgrade pass completes and the solution builds.

Test discovery:
- Test projects found: `Application.UnitTests`, `Domain.UnitTests`, `ArchitectureTests`, `Infrastructure.*.IntegrationTests`, `Presentation.Wpf.UnitTests`, `Application.UnitTests`.

Validation steps:
1. After upgrade and build, run all unit tests.
2. Run integration tests that exercise EF Core Sqlite and InMemory providers.
3. For WPF: there are unit tests; runtime UI verification is manual and out-of-scope for automated tasks unless UI tests exist.
4. For Blazor WebApp: run integration/unit tests and start the app in a dev environment to validate `UseExceptionHandler` behavior.

Success criteria for tests:
- All unit and integration tests pass.
- No new test infrastructure failures (test SDKs compatible).

If tests fail:
- Triage failures by project, fix API mismatches or behavioral regressions, run tests again within the same upgrade operation.

## 8. Risk Management
High-level risk assessment

- High Risk: `CleanArchitecture.Presentation.Wpf` — 55 issues (47 mandatory). Most issues are binary-incompatibility observations in generated `obj/` files. Risk mitigation: rely on regeneration of generated files after rebuild; inspect a small set of source files that directly reference WPF APIs (`TodosView.xaml.cs`, converters) and address API changes if compilation errors surface.

- Medium Risk: `CleanArchitecture.Infrastructure.EfCore.Sqlite` — EF Core package must be updated to 10.0.3; potential EF API changes may require code adjustments (migrations, APIs used by the repository layer).

- Low Risk: `CleanArchitecture.Presentation.BlazorWebApp` — only TFM change and a behavioral change in `UseExceptionHandler` call site.

Mitigations:
- Ensure the developer machine/CI has .NET 10 SDK installed (already validated).
- Update all package versions in a single pass; do not mix old and new major versions across projects during the atomic upgrade commit.
- Run build and all tests in CI after upgrade branch is pushed.
- If critical breaking changes appear, isolate the fix within the upgrade branch; consider temporary multi-targeting only if unavoidable.

Rollback plan:
- If the atomic upgrade proves too disruptive, revert `upgrade-to-NET10` branch and open targeted incremental upgrades for high-risk projects. Keep the plan and branch for reference.

## 9. Complexity & Effort Assessment
Relative complexity per project (Low/Medium/High):
- Low: `CleanArchitecture.Contracts`, `CleanArchitecture.Domain`, `CleanArchitecture.Application`, test projects (TFM change only)
- Medium: `Infrastructure.InMemory`, `Infrastructure.Composition` (package updates + some API reviews)
- Medium-High: `Infrastructure.EfCore.Sqlite` (EF Core major version update; integration tests)
- High: `Presentation.Wpf` (many findings; mainly generated, but requires careful rebuild and runtime validation)
- Low: `Presentation.BlazorWebApp` (small behavioral change in `Program.cs`)

Notes:
- Complexity is relative; the All-At-Once approach centralizes fixes into one cohesive pass.
- If high-risk items take longer to fix, consider isolating those fixes to follow-up incremental patches, but prefer to keep them on the upgrade branch.

## 10. Source Control Strategy
Branching and commit guidance (All-At-Once):

- Upgrade branch: `upgrade-to-NET10` (already created)
- Work locally on `upgrade-to-NET10`. Apply all TFMs and package updates in a single atomic change set.
- Prefer a single logical commit containing:
  - All project file TargetFramework changes
  - All package reference updates
  - Any minimal code fixes required to compile

Review process:
- Create a pull request from `upgrade-to-NET10` into `master`.
- Include assessment and plan links in PR description and list known risks and test status.

Notes on commit granularity:
- While a single logical commit is preferred to show atomic upgrade intent, you may use smaller commits for clarity (e.g., "Update TFMs", "Update packages", "Fix WPF compile errors") but keep them on the same branch and merge the branch as the upgrade unit.

## 11. Success Criteria
The upgrade is complete when all of the following are true:

Technical criteria:
- All projects target `net10.0` or `net10.0-windows` per their platform needs.
- All package updates from §5 are applied across affected projects.
- The solution builds without errors.
- All unit and integration tests pass.
- No outstanding security vulnerabilities reported by the assessment tools.

Process criteria:
- Changes are contained in the `upgrade-to-NET10` branch with a clear PR description.
- Reviewers have validated the PR and CI shows green builds and test runs.

Validation:
- Run the solution build and test suite in CI; if green, merge to `master`.
