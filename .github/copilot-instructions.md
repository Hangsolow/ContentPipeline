---
description: "Repository-wide custom instructions for GitHub Copilot chat and code review"
---

# ContentPipeline Copilot Instructions

Primary AI-agent instructions now live in [`/AGENTS.md`](../AGENTS.md).

Use this file as a lightweight compatibility entrypoint for Copilot surfaces that read `.github/copilot-instructions.md`.

## Core requirements

- Follow [`/AGENTS.md`](../AGENTS.md) as the source of truth for workflow, architecture, and conventions.
- For source generator work, use the repository skills under `.github/skills/` when applicable.
- Keep changes aligned with `.editorconfig`, nullable-as-errors rules, and the established test workflow.