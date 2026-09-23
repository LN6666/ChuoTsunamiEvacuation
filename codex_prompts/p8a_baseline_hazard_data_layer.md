# Codex Prompt Trace: P8-A Baseline Hazard Data Layer

Date: 2026-05-23.

## Task

Start PBL8 from the completed PBL7 practical high-detail baseline.

Project: ChuoTsunamiEvacuation.

Worktree: `D:\UnityProjects\ChuoTsunamiEvacuation-P7`.

Current P7 baseline: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.

Legacy fallback: `Assets/Scenes/Chuo_BaseMap.unity`.

Branch target: `p8-tsunami-hazard-risk-front-foundation`.

## P8-A Scope

P8-A setup includes:

- Baseline handoff from P7.
- P2-P6 compatibility gate on `P7_HighDetail_Chuo`.
- Tsunami hazard data layer schema/config foundation.
- Science-vs-visual layer separation.
- Tests, preflight, and review.

P8-A must not implement the dynamic light curtain, hazard interaction, collapse proxy behavior, P9 systems, P10 packaging, or gameplay success/failure rule changes.

## P8 Stage Count

P8 has exactly five stages:

- P8-A: Baseline handoff + P2-P6 compatibility gate + hazard data layer.
- P8-B: Official/evidence tsunami hazard layer v1 + dynamic risk front.
- P8-C: P2-P6 new-map smoke/proxy adaptation + infrastructure hazard states.
- P8-D: Infrastructure damage/blockage/lightweight collapse proxy.
- P8-E: Final P8 closeout and pre-P9 handoff verification.

No extra P8 stages are allowed.

## Baseline Preservation

`P7_HighDetail_Chuo.unity` may contain local-only high-detail imported scene state and must not be reset, checked out, overwritten, or deleted.

`Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, `Assets/PLATEAU`, and non-P8 `Assets/Data` paths must remain untouched.

## Hazard Schema Requirements

The P8 data layer must support:

- `scenarioId`
- `sourceMode`
- `hazardLayerVersion`
- `timeOriginSeconds`
- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `waterLevelMeters`
- `tsunamiHeightMeters`
- `inundationBoundary`
- `hazardIntensity`
- `confidence`
- `evidenceSourceId`
- `notes`
- `geometryType`
- `visualHeightMeters`
- `visualHeightIsCinematicOnly`
- `boundaryIsEvidenceBasedOrPrototype`
- `affectedInfrastructureTypes`
- `buildingDamageState`
- `collapseProxyState`
- `collapseProbability`
- `collapseRandomSeed`
- `hazardDrivenCollapse`

Scientific hazard values must remain separate from cinematic visual parameters. Large visual heights must be marked cinematic-only and must not be represented as real tsunami physical height.

## Validation

Required validation:

- P8-A preflight.
- Unity GUI EditMode tests.
- Unity GUI PlayMode tests.
- DeepSeek review.
- Commit and push only if there are no A-level blockers and the local baseline scene is preserved.
