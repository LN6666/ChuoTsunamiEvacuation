# DeepSeek Review Prompt: P8-D Damage / Blockage / Collapse Proxy

Review the current git diff for P8-D in `D:\UnityProjects\ChuoTsunamiEvacuation-P7`.

Check for A-level blockers:

- P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E.
- P8-D implements lightweight damage/blockage/collapse proxy only.
- No true structural collapse, physics collapse, debris, destruction, or rigidbody simulation is implemented.
- No P9 systems, crowd/spawn/failure/congestion/selectable vertical evacuation gameplay, or real indoor scene gameplay are implemented.
- No gameplay success/failure rules are changed.
- Humanitarian high-rise candidates remain non-official.
- Non-official warning is required for humanitarian candidates.
- Candidate status does not make candidates official shelters, safe, approved, or selectable.
- P8-D proxy status uses `inundationDepthMeters` and `hazardIntensity`.
- `maxTsunamiHeightMeters` is not used as physical depth.
- `visualHeightMeters` is not used as physical hazard depth.
- Deterministic seed, low probability, and max sample count control `collapsed_proxy_visual`.
- `collapsed_proxy_visual` is clearly visual/status only and not an engineering assessment.
- `Chuo_BaseMap.unity` is untouched.
- `ProjectSettings`, `Packages`, and `Assets/PLATEAU` are clean.
- `P7_HighDetail_Chuo.unity` is preserved and not staged.
- Tests and preflights pass.

Also report B-level follow-ups for P8-E:

- persistent visibility anchoring;
- final non-official disclaimers;
- P9 handoff for entrance / safe-floor / evacuation-complete proxy.
