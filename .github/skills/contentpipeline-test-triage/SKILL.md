---
name: contentpipeline-test-triage
description: Use when build or test failures occur in ContentPipeline to isolate root cause, reproduce reliably, and apply minimal corrective fixes.
---

# ContentPipeline Test Triage Skill

Use this skill when CI or local validation fails.

## Scope

- Build/test failures in:
  - `src/ContentPipeline/`
  - `tests/ContentSourceGeneratorTests/`
  - `tests/ContentPipelineBenchmarks/`
- Workflow parity checks against `.github/workflows/test.yml`

## Process

1. Reproduce with the same command shape used in CI.
2. Narrow to the smallest failing project or test set.
3. Fix root cause with minimal changes; avoid broad speculative edits.
4. Re-run failing scope first, then run the standard repo validation sequence.

## Validation sequence

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

## Guardrails

- Keep lockfiles intact unless dependency changes are explicitly required.
- Do not suppress nullable warnings or test failures to force green builds.
- Prefer deterministic fixes over timing-sensitive or environment-specific workarounds.
