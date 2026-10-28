---
name: contentpipeline-source-generator
description: Use when changing parser/emitter/code-builder logic in ContentPipeline source generation, generated model shape, or pipeline generation behavior.
---

# ContentPipeline Source Generator Skill

Use this skill for feature work or bug fixes in generator behavior.

## Scope

- `src/ContentPipeline/SourceGenerator/`
- `src/ContentPipeline/CodeBuilders/`
- `src/ContentPipeline/Utils/` when generation behavior depends on utility logic
- Related tests in `tests/ContentSourceGeneratorTests/`

## Process

1. Identify whether the change belongs in parsing, emitting, or shared code builders.
2. Reuse existing patterns in nearby parser/emitter files before introducing new abstractions.
3. Keep generated output deterministic, nullable-safe, and consistent with current naming/group conventions.
4. Add or update tests that assert the generated output and/or runtime pipeline behavior.

## Validation

Run from repository root:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

Use targeted test runs during iteration, then run the full test command before finishing.

## Guardrails

- Do not change generated naming or namespaces unless required by the task.
- Do not introduce non-incremental generator patterns when incremental patterns are available.
- Do not ship generator behavior changes without corresponding test updates.
