# P7-D Bridge / Underground / Road Status

Validation date: 2026-05-23.

## Summary

Roads, bridges, and one underground object are loaded as renderable Unity objects, but all are below the original LOD3 target and are not sufficient as final P8/P9 realism evidence.

## Roads

Are roads actually loaded: YES, as PLATEAU `tran` objects.

- Actual detected LOD: LOD0-LOD1.
- Object count: 14,524.
- MeshRenderer evidence: present.
- MeshFilter evidence: present.
- PLATEAUCityObjectGroup evidence: present.
- Status: renderable, low-detail, partial.

Sufficiency for P8/P9: conditional only. These roads may support visual context and early target placement, but they should not be treated as validated high-detail road geometry or official route evidence.

## Bridges

Are bridges actually loaded: YES, as PLATEAU `brid` objects.

- Actual detected LOD: LOD1.
- Object count: 13.
- MeshRenderer evidence: present.
- MeshFilter evidence: present.
- PLATEAUCityObjectGroup evidence: present.
- Status: renderable, low-detail, partial.

Sufficiency for P8/P9: limited. Bridge objects exist, but the LOD and coverage are too thin for final bridge-specific evacuation, congestion, or accessibility claims.

## Underground

Are underground streets/spaces actually loaded: PARTIAL.

- Actual detected LOD: LOD1.
- Object count: 1.
- MeshRenderer evidence: present.
- MeshFilter evidence: present.
- PLATEAUCityObjectGroup evidence: present.
- Status: renderable, low-detail, extremely limited.

Sufficiency for P8/P9: not sufficient for underground evacuation systems. The single detected object does not validate underground street/space coverage and P9 must not implement indoor or underground evacuation assumptions from this evidence.

## Blockers

- No LOD3 road/bridge/underground evidence.
- No route topology or walkability validation.
- No official-route safety claim validation.
- No P2-P6 runtime smoke test in the populated scene yet.
- No EXE profiling result yet.
