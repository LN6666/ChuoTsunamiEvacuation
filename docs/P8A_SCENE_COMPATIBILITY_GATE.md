# P8-A Scene Compatibility Gate

Date: 2026-05-23.

## Gate Purpose

P8-A adds a stronger compatibility smoke gate before P8-B creates scene-anchored risk-front visualization objects.

This gate protects the accepted practical baseline:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

It does not change gameplay success/failure rules, does not implement the P8-B light curtain, does not apply P8-C hazard interactions, and does not implement P8-D collapse proxy behavior.

## Stage Boundary

P8 has exactly five stages:

- P8-A
- P8-B
- P8-C
- P8-D
- P8-E

Do not create P8-0, P8-F, or P8-G.

## Baseline Rules

- `P7_HighDetail_Chuo.unity` is preserved as the user-approved practical high-detail baseline for P8/P9/P10.
- `Chuo_BaseMap.unity` is a legacy fallback policy and must remain untouched if restored locally.
- P8-B must use `P7_HighDetail_Chuo.unity` as the scene baseline.
- The current LOD/category limitations are accepted known limitations, not blockers for P8-A.
- Missing or partial layers must be handled through data, proxy, marker, or rule-based approaches until better scene evidence exists.

## Compatibility Evidence

P8-A compatibility is source-level and smoke-gate evidence, not a full runtime validation of every P2-P6 workflow inside the high-detail scene.

Validated by this gate:

- P2-P6 scripts remain present.
- Existing gameplay success/failure scripts remain outside P8-A changes.
- P8-A hazard data is read-only and fail-safe.
- P8-A does not hard-bind new work to `Chuo_BaseMap.unity`.
- P8-A docs explicitly mark pending runtime checks where the high-detail scene has not been fully exercised.

Pending before visual scene objects are created in P8-B:

- Representative player spawn/collision check on `P7_HighDetail_Chuo.unity`.
- Camera framing check on staged high-detail map targets.
- Shelter trigger and result-flow smoke test on staged high-detail anchors.
- P5 marker/route/candidate smoke against high-detail placement assumptions.
- P6 navigation/NPC prototype smoke with staged high-detail targets.
