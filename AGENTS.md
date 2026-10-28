# ContentPipeline Agent Guide

This is the canonical instruction file for AI coding agents working in this repository.

## Project overview

ContentPipeline is a Roslyn source generator for Optimizely CMS that converts content models to JSON-friendly pipeline models at compile time.

- **Source generator target**: `netstandard2.0` (`src/ContentPipeline/ContentPipeline.csproj`)
- **Generator language version**: C# 11
- **Test project target**: `net10.0` (`tests/ContentSourceGeneratorTests/ContentPipelineSourceGeneratorTests.csproj`)
- **Benchmarks target**: `net8.0`
- **Primary package**: `Hangsolow.ContentPipeline`

## Build and test (use this order)

Run from repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

For release-equivalent local validation:

```bash
dotnet build -c Release --no-restore
dotnet test -c Release --no-build --verbosity normal
```

## Repository layout

- `src/ContentPipeline/`
  - `SourceGenerator/` parser + emitter logic
  - `CodeBuilders/` reusable code generation builders
  - `Utils/` generator helper types
- `tests/ContentSourceGeneratorTests/`
  - `SourceGeneratorTests/` test content types, converters, and pipeline step fixtures
  - `Tests/` behavior/unit tests
- `tests/ContentPipelineBenchmarks/` benchmark project
- `docs/` user-facing documentation
- `.github/workflows/`
  - `test.yml` (PR test gate)
  - `release.yml` (package/release workflow)

## C# and repository conventions

- Nullable is globally enabled and nullable warnings are errors (`Directory.Build.props`).
- Prefer `is null` / `is not null` checks.
- Use file-scoped namespaces.
- Prefer expression-bodied members for properties/accessors/lambdas; regular methods use block bodies.
- Use `nameof(...)` instead of hardcoded member-name strings.
- Keep source-generator code compatible with C# 11.
- Follow `.editorconfig` formatting and naming conventions.

## Source generator implementation patterns

- Use `IIncrementalGenerator` patterns and avoid introducing non-incremental behavior.
- Keep parser and emitter responsibilities separated.
- Emit generated artifacts with stable namespaces and naming.
- Keep generated output nullable-safe (`#nullable enable`) and deterministic.
- Reuse existing code builders/helpers before adding new generation logic.

## Testing expectations

- Add/update tests for any generator behavior changes.
- Prefer the smallest targeted test first, then full `dotnet test` before finishing.
- For generation changes, validate both emitted shape and runtime behavior expectations in existing test suites.

## Documentation expectations

When behavior or public usage changes:

- Update related docs in `docs/` (especially `getting-started.md`, `advanced-usage.md`, `api-reference.md`, or `troubleshooting.md`).
- Keep examples aligned with current APIs and patterns.

## Agent skills in this repository

Use the closest matching skill before implementing:

- `.github/skills/contentpipeline-source-generator/SKILL.md` — source generator feature/fix changes.
- `.github/skills/contentpipeline-test-triage/SKILL.md` — test/build failure diagnosis and stabilization.
- `.github/skills/contentpipeline-docs-sync/SKILL.md` — docs updates coupled to code/API changes.

If a task crosses multiple areas, apply the source-generator skill first, then use the others as needed.
