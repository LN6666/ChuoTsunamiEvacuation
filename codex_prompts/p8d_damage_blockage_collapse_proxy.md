# P8-D Codex Prompt Trace

Task name: Infrastructure damage / blockage / lightweight collapse proxy + humanitarian high-rise hazard status integration.

Worktree: `D:\UnityProjects\ChuoTsunamiEvacuation-P7`

Branch: `p8-tsunami-hazard-risk-front-foundation`

## Required Scope

- Extend P8-C infrastructure hazard states with P8-D damage, blockage, low-floor warning, entrance blocked, restricted, inaccessible, and lightweight visual-collapse proxy states.
- Use P8-B/P8-C evidence fields: `arrivalTimeSeconds`, `inundationDepthMeters`, derived/prototype boundary state through P8-C, `hazardIntensity`, `confidence`, `sourceMode`, and `evidenceSourceId`.
- Keep `maxTsunamiHeightMeters` separate from `inundationDepthMeters`.
- Keep `visualHeightMeters` cinematic only.
- Integrate `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json` as non-official humanitarian candidate status input.
- Create P8-D configs, docs, tests, preflight, and DeepSeek review prompt.

## Prohibitions

- Do not modify `Assets/Scenes/Chuo_BaseMap.unity`.
- Do not reset, overwrite, delete, or stage `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Do not modify `ProjectSettings`, `Packages`, or `Assets/PLATEAU`.
- Do not implement true structural collapse, physics collapse, debris, destruction, or rigidbody simulation.
- Do not implement P9 crowd/spawn/failure/selectable vertical evacuation gameplay.
- Do not change gameplay success/failure rules.
- Do not make humanitarian candidates official shelters, safe, approved, or selectable.

## Stage Boundary

P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E. Do not create P8-0, P8-F, or P8-G.
