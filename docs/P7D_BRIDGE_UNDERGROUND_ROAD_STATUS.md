# P7-D Bridge / Underground / Road Status

Validation date: 2026-05-23.

## Summary

Roads, bridges, and one underground object are loaded as renderable Unity objects, but all are below the original LOD3 target. The user accepts this state as part of the practical high-detail baseline for P8/P9/P10, with the limitations documented below.

## Roads

Are roads actually loaded: YES, as PLATEAU `tran` objects.

- Actual detected LOD: LOD0-LOD1.
- Object count: 14,524.
- MeshRenderer evidence: present.
- MeshFilter evidence: present.
- PLATEAUCityObjectGroup evidence: present.
- Status: renderable, low-detail, partial.

Sufficiency for P8/P9: accepted as practical baseline context. These roads may support visual context and early target placement, but they should not be treated as validated high-detail road geometry or official route evidence.

## Bridges

Are bridges actually loaded: YES, as PLATEAU `brid` objects.

- Actual detected LOD: LOD1.
- Object count: 13.
- MeshRenderer evidence: present.
- MeshFilter evidence: present.
- PLATEAUCityObjectGroup evidence: present.
- Status: renderable, low-detail, partial.

Sufficiency for P8/P9: accepted with limitations. Bridge objects exist, but the LOD and coverage are too thin for final bridge-specific evacuation, congestion, or accessibility claims without proxy/rule-based follow-up.

## Underground

Are underground streets/spaces actually loaded: PARTIAL.

- Actual detected LOD: LOD1.
- Object count: 1.
- MeshRenderer evidence: present.
- MeshFilter evidence: present.
- PLATEAUCityObjectGroup evidence: present.
- Status: renderable, low-detail, extremely limited.

Sufficiency for P8/P9: accepted as limited context only. The single detected object does not validate underground street/space coverage. If P9 needs underground or interior behavior, use markers, rule-based nodes, proxy colliders, and representative templates rather than claiming complete geometry.

## Known Limitations

- No LOD3 road/bridge/underground evidence.
- No route topology or walkability validation.
- No official-route safety claim validation.
- No P2-P6 runtime smoke test in the populated scene yet.
- No EXE profiling result yet.
