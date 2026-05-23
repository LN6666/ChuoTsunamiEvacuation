# P8-A Baseline Handoff From P7

Date: 2026-05-23.

## Accepted Baseline

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is the user-approved practical high-detail baseline for P8, P9, and P10.

`Assets/Scenes/Chuo_BaseMap.unity` remains the legacy fallback policy. P8 must not modify it.

## P7 Evidence Carried Into P8

- P7-D verified renderable PLATEAU content in the practical scene.
- Actual detected LOD coverage is LOD0-LOD2.
- Average LOD3 is not achieved or claimed.
- Full-category high-detail PLATEAU import completeness is not claimed.
- Missing or partial water, relief/terrain, disaster risk, land use, urban planning, vegetation, and city furniture evidence remains a known condition.

## P8-A Handoff Rule

P8 starts from `P7_HighDetail_Chuo.unity`. The lower-than-original-target LOD/category coverage is accepted as a practical baseline limitation, not a blocker.

P8 hazard work must account for missing or low-detail city layers through data-layer, proxy, marker, and rule-based approaches until better evidence or imports are available.

## Local State Warning

The P7 high-detail scene may contain local-only imported map state and may appear dirty in Git because the imported scene is too large for normal source tracking.

P8-A must not run destructive Git commands that reset, checkout, clean, or overwrite `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
