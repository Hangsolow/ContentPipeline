---
name: contentpipeline-docs-sync
description: Use when code changes affect public usage, architecture, configuration, or troubleshooting guidance, so docs stay aligned with implementation.
---

# ContentPipeline Docs Sync Skill

Use this skill whenever behavior or usage changes should be reflected in documentation.

## Scope

- `README.md`
- `docs/getting-started.md`
- `docs/architecture.md`
- `docs/configuration.md`
- `docs/advanced-usage.md`
- `docs/api-reference.md`
- `docs/testing.md`
- `docs/troubleshooting.md`

## Process

1. Identify which user-visible behavior changed.
2. Update only the relevant documentation pages, preserving existing style and structure.
3. Keep examples valid for current API shapes and runtime expectations.
4. Cross-link related docs when new behavior spans multiple topics.

## Completion checklist

- New/changed behavior is documented where users will look first.
- Examples match current naming and API signatures.
- References to generated models/pipeline steps/property converters remain accurate.

## Guardrails

- Do not add speculative documentation for behavior that is not implemented.
- Avoid broad rewrites when a focused update is sufficient.
