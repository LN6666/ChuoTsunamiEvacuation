# P8-A P7 High-Detail Baseline Status

Date: 2026-05-23.

## Observed Baseline State

Branch inspected: `p8-tsunami-hazard-risk-front-foundation`.

Scene path:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

Observed state at P8-A compatibility-gate start:

- The scene exists locally.
- The scene is tracked by Git.
- The scene has an unstaged local dirty state and must not be reset, restored, checked out, or overwritten.
- The scene size is approximately 22.55 GB, matching the expected practical imported-map baseline scale.
- The local imported-map state is preserved as the P8/P9/P10 practical baseline.

## Legacy Fallback State

`Assets/Scenes/Chuo_BaseMap.unity` is not present in this checkout at inspection time.

It remains the legacy fallback policy from earlier project phases. If restored locally, it must remain untouched and must not become the P8-B scene baseline.

## Accepted Known Limitations

The current P7 high-detail baseline remains accepted with known limitations:

- Average LOD3 is not achieved or claimed.
- Category coverage is incomplete.
- Some hazard-relevant layers are missing or partial.
- P2-P6 source compatibility is accepted, while full high-detail runtime smoke remains pending.

## P8-B Baseline Requirement

P8-B must anchor risk-front visualization work to `P7_HighDetail_Chuo.unity`.

P8-B must not depend on `Chuo_BaseMap.unity`, must not create official route or hazard-safety claims, and must verify staged scene anchors before creating visual risk-front objects.
