# CleanArchitecture .NET 10.0 Upgrade Tasks

## Overview

This document tracks the coordinated upgrade of the CleanArchitecture solution from .NET 8 to .NET 10.0. All projects will have their target frameworks and package references updated, followed by a build-and-fix pass and full test validation.

**Progress**: 1/4 tasks complete (25%) ![0%](https://progress-bar.xyz/25)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-02-14 17:21)*
**References**: Plan §Phase 0, Plan §2 Migration Strategy

- [✓] (1) Verify .NET 10.0 SDK is installed on the host per Plan §Phase 0
- [✓] (2) Runtime/SDK version meets minimum requirements (**Verify**)
- [✓] (3) Check for presence and compatibility of `global.json` or repository-level MSBuild props (if present) per Plan §Migration Strategy
- [✓] (4) Configuration files compatible with target version (**Verify**)

### [▶] TASK-002: Atomic framework and package upgrade with compilation fixes
**References**: Plan §2 Migration Strategy, Plan §4 Project-by-Project Plans, Plan §5 Package Update Reference, Plan §6 Breaking Changes Catalog

- [▶] (1) Update `TargetFramework` / `TargetFrameworks` in all project files listed in Plan §Project list to `net10.0` or `net10.0-windows` as appropriate
- [ ] (2) Update package references across all affected projects per Plan §5 (EF Core → 10.0.3, Microsoft.Extensions.* → 10.0.3, etc.)
- [ ] (3) Restore all dependencies (e.g., `dotnet restore`) for the solution
- [ ] (4) Build the entire solution and fix all compilation errors discovered (reference Plan §6 Breaking Changes Catalog for common issues)
- [ ] (5) Solution builds with 0 errors (**Verify**)

### [ ] TASK-003: Run full test suite and validate upgrade
**References**: Plan §7 Testing Strategy, Plan §6 Breaking Changes Catalog

- [ ] (1) Run all unit and integration test projects listed in Plan §Project list
- [ ] (2) Fix any failing tests (reference Plan §6 for likely breaking change causes)
- [ ] (3) Re-run tests after fixes
- [ ] (4) All tests pass with 0 failures (**Verify**)

### [ ] TASK-004: Final commit
**References**: Plan §10 Source Control Strategy

- [ ] (1) Commit all remaining changes with message: "TASK-004: Complete upgrade to .NET 10.0"


