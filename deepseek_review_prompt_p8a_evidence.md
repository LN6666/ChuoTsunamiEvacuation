# DeepSeek Review Prompt: P8-A Evidence Source Plan + Hazard Data Semantics Hardening

You are reviewing the git diff for P8-A evidence/schema/docs/tools/test hardening in the Unity + PLATEAU ChuoTsunamiEvacuation project.

Review only the changed files. Prioritize A-level blockers: compile risk, runtime risk, scene mutation, scope violation, false official-data claims, schema ambiguity, or validation gaps.

## Required Scope Checks

- Confirm no scene mutation occurred.
- Confirm `Assets/Scenes/Chuo_BaseMap.unity` is not changed.
- Confirm `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is not changed.
- Confirm `ProjectSettings/` and `Packages/` are not changed.
- Confirm no P8-B visual light curtain implementation was added.
- Confirm no P8-C/P8-D road, building, bridge, underground, or collapse gameplay interaction was implemented.
- Confirm no P9 or P10 systems were added.
- Confirm no live official data fetch, download, scrape, or ingestion was added.

## Required Semantics Checks

- Confirm P8 still has exactly five stages: P8-A, P8-B, P8-C, P8-D, and P8-E.
- Confirm evidence source categories include official tsunami/inundation maps, Tokyo/Chuo hazard maps, Cabinet Office / MLIT / local government data, academic tsunami simulation papers, PLATEAU / CityGML category sources, OSM or route context with attribution handling, and manual sample data.
- Confirm no official values are falsely claimed.
- Confirm manual sample data is clearly marked as non-authoritative.
- Confirm `evidenceSourceId` and `sourceMode` semantics are clear.
- Confirm `confidence`, `geometryType`, and `boundaryIsEvidenceBasedOrPrototype` semantics are clear.
- Confirm inundation depth, water level, tsunami height, and cinematic visual height are clearly separated.
- Confirm arrival front, visual risk front, data boundary, and visual curtain boundary are clearly separated.
- Confirm `visualHeightMeters` may be large only when `visualHeightIsCinematicOnly=true`.
- Confirm science fields are separated from visual fields in schema/config/docs/tests.
- Confirm collapse proxy fields remain data/config only and do not affect gameplay.

## Required Validation Checks

- Confirm `tools/p8/validate_p8_hazard_json.ps1` checks required fields, source modes, evidence IDs, geometry types, confidence bounds, visual height cinematic flag, manual/sample labeling, official-claim fail-safety, and disabled collapse gameplay behavior.
- Confirm EditMode tests cover hazard sample loading, missing required evidence ID failure, large visual height without cinematic flag failure, sourceMode fail-safe behavior, science/visual separation, and collapse proxy parsing without gameplay effects.
- Confirm P8-A preflight passed or identify any failure.
- Confirm Unity GUI EditMode and PlayMode tests passed or identify any failure.

## Output Format

Return:

- Verdict: PASS or BLOCKED.
- A-level blockers, if any.
- B/C-level recommendations, if any.
- Notes on test/preflight evidence.
- A concise final assessment of whether this diff is safe to commit and push.
