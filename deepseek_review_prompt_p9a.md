# DeepSeek Review Prompt: P9-A Crowd / Spawn / Entrance-Safe-Floor Proxy Foundation

Review the current git diff for P9-A in `ChuoTsunamiEvacuation`.

Branch:

- `p9-crowd-spawn-evacuation-proxy-foundation`

Base:

- `origin/p8-tsunami-hazard-risk-front-foundation`

## Review Goals

Check for A-level blockers and B-level follow-ups.

## Required Scope Checks

- P9 has exactly four stages: P9-A, P9-B, P9-C, P9-D.
- No additional P9 stage artifacts are introduced.
- P9-A is scaffold only.
- No real indoor scene gameplay is implemented.
- No stair, fire-route, interior-template, or indoor shelter gameplay is implemented.
- No final evacuation failure gameplay is implemented.
- No NPC/crowd-caused player failure is implemented.
- No P8-D/E final state dependency is assumed.
- Missing P8 handoff data returns safe no-effect fallback state.
- No `Chuo_BaseMap.unity` mutation.
- No `P7_HighDetail_Chuo` scene mutation.
- No `ProjectSettings` or `Packages` changes.
- No `Assets/PLATEAU` changes.
- No P2-P6 success/failure rule changes.
- No real tsunami fluid/hazard system implementation.
- No P10 release packaging.

## Required Artifact Checks

Confirm these exist and are in allowed paths:

- `docs/P9_STAGE_PLAN.md`
- `docs/P9_BOUNDARIES.md`
- `docs/P9_RELATION_TO_P8.md`
- `docs/P9_NO_INDOOR_SCENE_DECISION.md`
- `docs/P9A_P8_HANDOFF_CONTRACT.md`
- `docs/P9A_METRICS_AND_LOGGING_PLAN.md`
- `docs/P9A_FOUNDATION_SUMMARY.md`
- `docs/P9A_TEST_RESULTS.md`
- `docs/P9A_KNOWN_LIMITATIONS.md`
- `docs/P9A_REVIEW_BACKLOG.md`
- `docs/P9A_NEXT_STEPS.md`
- `Assets/Data/P9/p9_spawn_point_schema.json`
- `Assets/Data/P9/p9_spawn_points_sample.json`
- `Assets/Data/P9/p9_crowd_agent_schema.json`
- `Assets/Data/P9/p9_crowd_agents_sample.json`
- `Assets/Data/P9/p9_entrance_safe_floor_proxy_schema.json`
- `Assets/Data/P9/p9_entrance_safe_floor_proxy_sample.json`
- `Assets/Scripts/P9/`
- `Assets/Tests/EditMode/P9/`
- `Assets/Tests/PlayMode/P9/`
- `tools/p9/run_p9a_preflight.ps1`
- `tools/p9/validate_p9_json.ps1`
- `codex_prompts/p9a_crowd_spawn_evacuation_proxy_foundation.md`

## Data Rules

- Spawn schema supports `spawnPointId`, `spawnType`, position/coordinate fields, `sourceMode`, capacity/weight, `activeScenarioIds`, notes, and `deterministicSeed`.
- Crowd profile schema supports `agentProfileId`, `agentType`, `speedMetersPerSecond`, `targetPreference`, `evacuationBehaviorMode`, `crowdRadius`, `panicLevelProxy`, `sourceMode`, and notes.
- Entrance/safe-floor schema supports `proxyId`, related shelter/candidate IDs, entrance position, safe-floor label, vertical evacuation status, official shelter flag, humanitarian candidate flag, non-official warning flag, interaction mode, and notes.
- Humanitarian high-rise candidates remain non-official and require warning.
- Crowd profile wording does not claim a real psychology or real crowd model.

## Runtime Rules

- Runtime scripts fail safe on missing/invalid data.
- Runtime scripts do not require scene dependencies.
- Runtime scripts do not require `Chuo_BaseMap` or `P7_HighDetail_Chuo`.
- Runtime scripts do not mutate final success/failure state.
- P8 handoff adapter accepts missing P8-D/E data and returns safe no-effect defaults.

## Validation Evidence To Check

Confirm reported results for:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p9/run_p9a_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Return:

- Verdict: PASS or BLOCKED.
- A-level blockers, if any.
- B-level follow-ups.
- Any Unity compile/test risks.
- Any protected-path or scope risks.
