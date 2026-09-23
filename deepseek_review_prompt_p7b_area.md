# DeepSeek Review Prompt: P7-B Area Feasibility

Review the current git diff for P7-B Codex A.

Project:

`D:\UnityProjects\ChuoTsunamiEvacuation-P7BArea`

Branch:

`p7b-area-feasibility`

## Scope To Verify

This P7-B work must be docs/tools/prompts only. It must not import assets, modify Unity scenes, modify gameplay scripts, modify `Assets/Data`, modify `ProjectSettings`, modify `Packages`, or modify imported PLATEAU assets.

Protected paths must remain untouched:

- `Assets/Scenes/`
- `Assets/Scripts/`
- `Assets/Data/`
- `Assets/PLATEAU/`
- `ProjectSettings/`
- `Packages/`

## Required Checks

Please verify:

- No protected paths changed.
- No Unity scene, script, asset, setting, package, or PLATEAU import change exists.
- `tools/p7/select_p7b_candidate_area.ps1` is read-only and only emits Markdown/JSON to stdout.
- `tools/p7/run_p7b_area_feasibility.ps1` runs the selector and `tools/p7/run_p7_preflight.ps1`, and exits non-zero on failure.
- P7 has exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D.
- P7-B does not implement P8 hazard systems or P9 crowd/spawn/indoor evacuation systems.
- LOD findings are clearly marked as unverified path/name/file-metadata inference.
- LOD4 is not assumed available.
- Underground, bridge, road, and riverfront findings are not claimed as geometry, visual, route, entrance, or gameplay correctness.
- The small-area benchmark decision is conservative and does not approve full Chuo import.
- `docs/TASKS.md`, `docs/REVIEW_BACKLOG.md`, and `docs/P7_DECISION_LOG.md` updates are scoped to P7-B feasibility.

## Expected Output

Return:

- Verdict: PASS / PASS WITH FOLLOW-UP / BLOCKED.
- A-level blockers first, if any.
- B-level follow-ups, if any.
- Confirmation that protected paths are untouched.
- Confirmation that helper scripts are read-only.
- Confirmation that P7-B remains pre-import feasibility only.
