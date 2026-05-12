# Code Review Workflow

## Purpose

This project uses Codex for code generation and DeepSeek V4 Pro for code review.

The goal is to keep AI-generated code safe, maintainable, testable, and aligned with the project design.

## Roles

### Codex

Codex is the main coding agent.

Codex should:
- create small Unity C# scripts
- modify only specified files
- fix compile errors
- follow AGENTS.md
- avoid unrelated changes
- keep code simple and testable

Codex should not:
- rewrite the whole project without approval
- modify Library, Temp, Logs, Builds, or raw PLATEAU data
- introduce large third-party dependencies without approval
- implement unrelated features

### DeepSeek V4 Pro

DeepSeek V4 Pro is used as a code review and architecture review tool.

DeepSeek should:
- review git diff
- detect compile risks
- detect NullReferenceException risks
- check Unity lifecycle issues
- check Inspector reference risks
- check prefab and scene reference risks
- check performance risks
- check architecture problems
- check overengineering
- suggest small Codex tasks

DeepSeek should not:
- directly modify project files
- rewrite the whole codebase

### Human Developer

The human developer should:
- confirm requirements
- run Unity
- test gameplay
- approve changes
- decide whether to commit
- update Markdown records

## Standard Review Process

1. Confirm the current task in docs/TASKS.md.
2. Run a grill-me check if this is a core system.
3. Update the related Markdown specification.
4. Generate a focused Codex prompt.
5. Let Codex modify only specified files.
6. Run Unity and check Console errors.
7. Run git diff.
8. Run DeepSeek review.
9. Fix critical issues.
10. Test again in Unity.
11. Commit and push.

## Running DeepSeek Review

From the project root, run:

python tools/deepseek_review.py

The script reviews staged changes first.

If no staged diff exists, it reviews unstaged changes.

Review reports are saved to:

review_reports/

This folder is ignored by Git.

## Review Result Policy

If DeepSeek says "Safe to commit":
- run Unity once more
- commit and push

If DeepSeek says "Commit after minor fixes":
- ask Codex to fix the listed issues
- rerun Unity
- optionally rerun review
- commit and push

If DeepSeek says "Do not commit yet":
- stop
- fix critical issues first
- do not push broken code

## Codex Fix Prompt Template

Please fix the following DeepSeek review issues.

Target files:
- Assets/Scripts/...

Rules:
- Do not modify unrelated files.
- Do not rewrite the whole system.
- Keep public serialized fields stable unless necessary.
- Preserve existing class names unless the review requires renaming.
- Focus only on the listed issues.

Issues:
1. ...
2. ...

## Review Script Maintenance

The review script is located at:

tools/deepseek_review.py

Dependencies are listed in:

requirements.txt

API key is stored as an environment variable:

DEEPSEEK_API_KEY

Do not commit API keys.

Generated review reports and logs must not be committed.

Ignored folders:
- review_reports/
- logs/
