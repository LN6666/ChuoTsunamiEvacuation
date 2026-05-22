# P7 Automation Workflow

Date: 2026-05-22

## Principle

If a P7 task can be automated, automate it. If a result can be recorded in Markdown, record it. If a safety boundary can be checked by script, run the script before review or commit.

## P7-0 Command-Line Workflow

Run from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_preflight.ps1
```

The preflight script:

1. Runs `tools/p7/check_p7_scope.ps1`.
2. Runs `tools/p7/write_p7_status_report.ps1`.
3. Prints final PASS/FAIL.
4. Exits non-zero if the scope guard fails.

## Scope Guard

`tools/p7/check_p7_scope.ps1` checks changed and untracked files against HEAD.

It fails on:

- Changes under protected paths.
- Newly added large files outside explicitly allowed docs/log paths.

It warns on forbidden scope keywords such as P7-E/P7-F/P7-G, tsunami height, inundation depth, light curtain, indoor evacuation, real spawn, building collapse, road block gameplay, and crowd failure. Warnings must be reviewed, but warnings do not fail the guard unless they correspond to actual scope changes.

## Markdown Status Report

`tools/p7/write_p7_status_report.ps1` creates `docs/p7_status/` if needed and writes a Markdown report with:

- Date/time.
- Branch.
- Git status.
- Changed files.
- Latest commit.
- Scope guard result summary if available.
- Notes placeholder.

The status report is a coordination record. It is not a substitute for tests or DeepSeek review.

## When To Run Unity GUI Tests

Run Unity EditMode and PlayMode tests when a P7 stage changes:

- `Assets/Scripts/`
- `Assets/Data/`
- `Assets/Scenes/`
- Imported Unity assets or prefabs.
- Gameplay-facing scene wiring.
- Build/runtime code.

P7-A/P7-B/P7-C/P7-D must run automated Unity tests whenever Unity code/assets/scenes are changed. If Unity tests are skipped in those stages, the stage record must explain why the change is documentation/tooling-only.

## When Not To Run Unity Tests

P7-0 does not run Unity EditMode or PlayMode tests because P7-0 is docs/tools/prompts only. It does not change Unity scripts, Unity scenes, Unity assets, `Assets/Data`, packages, project settings, or gameplay behavior.

For P7-0, the appropriate validation is command-line preflight plus DeepSeek review.

## DeepSeek Review Command Pattern

Use the project DeepSeek workflow after P7-0 preflight passes. The review prompt is:

```text
deepseek_review_prompt_p70.md
```

The current project review script is:

```powershell
python tools\deepseek_review.py
```

If the review script requires credentials or produces local raw reports, keep raw reports under `review_reports/` and do not commit them.

## Commit / Push Workflow

1. Run P7 preflight.
2. Review `git diff --name-only`.
3. Confirm protected paths are untouched.
4. Run DeepSeek review for the stage.
5. Summarize review results in `docs/REVIEW_BACKLOG.md` if actionable.
6. Commit after a stable milestone.
7. Push only after the human developer approves.
