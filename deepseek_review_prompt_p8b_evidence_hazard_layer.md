# DeepSeek Review Prompt: P8-B Evidence Hazard Layer

Review the current P8-B evidence hazard layer update on `p8-tsunami-hazard-risk-front-foundation`.

## Context

P8-B already includes:

- Dynamic cinematic tsunami risk front.
- P8BGuard validation and performance hardening.
- Hazard-layer-driven v1 front behavior.

This update corrects the evidence model: Chuo City may not publish a standalone tsunami hazard map equivalent to flood hazard maps, but Tokyo Metropolitan Government tsunami damage estimation map/report sources are the primary Chuo evidence candidate. P8-B must not fall back to generic flood proxy as the main driver.

## Required Review

Classify findings as:

- A-level blocker: must fix before commit/push.
- B-level issue: should fix soon but does not block.
- C-level note: minor cleanup or follow-up.

Give a final verdict: blocked, safe to commit, or safe to commit with B/C follow-up.

## Must Check

- P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, and P8-E.
- Tokyo Metropolitan Government tsunami damage estimation is prioritized as the primary Chuo evidence candidate.
- Chuo standalone tsunami-map absence is not mistaken for evidence absence.
- Required source registry entries exist:
  - `tokyo_damage_estimation_map_tsunami`
  - `tokyo_damage_estimation_report_tsunami`
  - `chuo_city_tsunami_liquefaction_page`
  - `supplementary_pdf_tokyo_bay_tsunami_height_chuo`
  - `flood_proxy_chuo_hazard_map`
- `flood_proxy_chuo_hazard_map` is clearly non-tsunami proxy fallback only.
- Required evidence categories exist:
  - `official_tsunami_metropolitan`
  - `official_tsunami_report_reference`
  - `official_flood_proxy`
  - `academic_model_candidate`
  - `manual_extraction_required`
  - `evidence_planned`
- No false claim of complete official Chuo spatial inundation layer.
- Maximum tsunami height references around 2.4m to 2.46m are not confused with a spatial inundation-depth grid.
- `maxTsunamiHeightMeters` is separate from `inundationDepthMeters`.
- `inundationDepthMeters` and `inundationBoundary` are marked placeholder/prototype/pending unless actually extracted.
- Risk front reports:
  - official metropolitan source known,
  - spatial extraction pending,
  - current boundary/depth prototype/manual,
  - not final official inundation surface.
- P8-C gate is explicit and currently `CONDITIONAL PASS`.
- P8-C is not implemented yet.
- P8-D collapse proxy gameplay is not implemented.
- P9/P10 systems are not introduced.
- Cinematic `visualHeightMeters` remains separate from physical `tsunamiHeightMeters`, `waterLevelMeters`, and `inundationDepthMeters`.
- Gameplay success/failure rules are unchanged.
- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` baseline was not reset, lost, staged, or overwritten.
- `Assets/Scenes/Chuo_BaseMap.unity` is untouched.
- `ProjectSettings` and `Packages` are clean.
- Tests and preflights pass:
  - `tools/p8/run_p8b_evidence_preflight.ps1`
  - `tools/p8/run_p8b_front_v1_preflight.ps1`
  - `tools/p8/run_p8b_riskfront_preflight.ps1`
  - `tools/p8/run_p8b_guard_preflight.ps1`
  - `tools/p8/run_p8a_preflight.ps1`
  - Unity EditMode GUI tests
  - Unity PlayMode GUI tests

## Protected Paths

Do not recommend committing or modifying:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- `Assets/Scenes/Chuo_BaseMap.unity`
- `ProjectSettings/`
- `Packages/`
- `Assets/PLATEAU/`
- raw PLATEAU data
- generated logs or review reports

The high-detail scene may remain dirty as local baseline state, but it must not be staged.
