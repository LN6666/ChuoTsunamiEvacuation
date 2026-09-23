# Codex Prompt: P7-B Wave 2-B LOD3 Candidate Dry Run

Working tree:

`D:\UnityProjects\ChuoTsunamiEvacuation-P7BWave2B`

Branch:

`p7b-wave2b-lod3-candidate-dryrun`

Base:

`origin/p7-high-detail-city-foundation`

Phase:

PBL7 / P7-B Wave 2-B

## Objective

Create a conservative command-line-only LOD3 candidate dry-run package for planning candidate `53393690`, with fallback candidates `53393672` and `53394611`.

This is a metadata feasibility step only. It must not import, copy, move, delete, or modify PLATEAU data or Unity assets.

## Fixed P7 Scope

P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

## Dry-Run Rules

- Use only file/path metadata from `D:\PLATEAU_DATA\Chuo_2025_CityGML`.
- Do not parse CityGML geometry for quality claims.
- Do not import assets into Unity.
- Do not copy candidate assets into `Assets/`.
- Do not create or modify Unity scenes.
- Do not modify existing gameplay code.
- Do not modify `Chuo_BaseMap.unity`.
- Do not modify `ProjectSettings` or `Packages`.
- Do not modify `Assets/Data` or `Assets/PLATEAU`.
- Do not implement P8 tsunami hazard, inundation, flood, or light curtain systems.
- Do not implement P9 crowd, real spawn, indoor evacuation, congestion, or failure systems.

## Deliverables

- `tools/p7/inspect_p7b_lod3_candidate.ps1`
- `tools/p7/run_p7b_wave2b_preflight.ps1`
- `docs/P7B_WAVE2B_LOD3_CANDIDATE_DRYRUN.md`
- `docs/P7B_WAVE2B_IMPORT_DECISION.md`
- `docs/P7B_WAVE2B_VISUAL_FEASIBILITY_PLAN.md`
- `docs/P7B_WAVE2B_TEST_RESULTS.md`
- `docs/P7B_WAVE2B_KNOWN_LIMITATIONS.md`
- `deepseek_review_prompt_p7b_wave2b.md`
- shared updates to `docs/TASKS.md`, `docs/REVIEW_BACKLOG.md`, and `docs/P7_DECISION_LOG.md`

## Required Validation

Run:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2b_preflight.ps1
```

Unity tests are required only if Unity files are changed. This dry-run package should not change Unity files.

Run DeepSeek review with:

```powershell
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p7b_wave2b.md
```

Commit and push only if preflight passes, protected paths are clean, DeepSeek has no A-level blockers, and no Unity production files changed.
