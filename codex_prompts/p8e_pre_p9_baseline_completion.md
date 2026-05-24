# P8-E Hardening / Pre-P9 Baseline Completion Prompt

Task name: P8-E Hardening / Pre-P9 Baseline Completion.

Continue PBL8 after P8-E final closeout on `p8-tsunami-hazard-risk-front-foundation`.

Objectives:

- keep P8 at exactly five stages: P8-A through P8-E;
- complete real/proxy PLATEAU semantic object binding v1 for road, building, bridge, underground, entrance, waterfront, shelter proxy, navigation target proxy, humanitarian candidate proxy, and high-rise candidate marker;
- prepare persistent marker/hazard-status readiness for all 110 non-official humanitarian high-rise candidates;
- harden the dynamic light curtain progression model with arrival-time-first behavior, onshore speed fallback, shallow-depth slowdown, and visual/physical field separation;
- produce a specific P2-P6 passed/proxy/blocked matrix;
- document P5 route/candidate geometry handoff and WGS84/PLATEAU transform limits;
- update the P9 handoff package so P9 focuses on final gameplay landing and does not redo P8 foundations.

Guardrails:

- do not modify `Chuo_BaseMap.unity`;
- do not reset or stage `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`;
- do not modify ProjectSettings, Packages, or Assets/PLATEAU;
- do not implement P9 crowd/spawn/congestion/failure gameplay;
- do not implement real indoor scene gameplay;
- do not change gameplay success/failure rules;
- do not make humanitarian candidates official shelters or selectable life-first targets;
- do not claim official route status, official building safety approval, official collapse prediction, or official inundation contour where only derived boundary exists;
- do not use `visualHeightMeters` or `maxTsunamiHeightMeters` as `inundationDepthMeters`.

Validation requested:

- `tools/p8/run_p8e_hardening_preflight.ps1`
- `tools/p8/run_p8e_preflight.ps1`
- `tools/p8/run_p8_final_preflight.ps1`
- Unity GUI EditMode tests
- Unity GUI PlayMode tests
- DeepSeek review with `deepseek_review_prompt_p8e_hardening.md`
