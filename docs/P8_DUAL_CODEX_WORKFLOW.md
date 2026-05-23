# P8 Dual-Codex Workflow

## Start Condition

Do not start dual worktrees until P8-A setup has preserved the local P7 baseline and committed the data-layer foundation.

## Codex A: P8Compat

Focus:

- Baseline preservation.
- P2-P6 compatibility.
- Scene readiness.
- Smoke tests and handoff checks.
- Protection of `P7_HighDetail_Chuo.unity`.

Future worktree:

`D:\UnityProjects\ChuoTsunamiEvacuation-P8Compat`

## Codex B: P8Hazard

Focus:

- Hazard data schema.
- Evidence source plan.
- Risk-front configuration.
- Science/visual separation.
- Loader and validator behavior.

Future worktree:

`D:\UnityProjects\ChuoTsunamiEvacuation-P8Hazard`

## Coordination Rules

- Keep P8-A source of truth in the main P8 branch until preservation is complete.
- Avoid parallel edits to the accepted baseline scene.
- Keep hazard science fields and visual/cinematic fields separate in code, data, and docs.
- Treat P7 map limitations as known conditions, not blockers.
