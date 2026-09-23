# NewMap Manual Blocker Audit

Generated: 2026-05-29T00:00:00+09:00

Scene: `Assets/Scenes/Chuo_BaseMap.unity`

Manual blocker audit result:

- Mouse look existed only as RMB orbit and was not tied to gameplay cursor state.
- The scene header had no assigned sun, so runtime lighting now creates an explicit day/night/rain directional light and ambient setup.
- Runtime local training proxy targets are now diagnostics-only; normal manual mode keeps `DebugDiagnosticsRoot` inactive and does not spawn those visible test markers.
- The runtime support surface is now invisible and aligned to the resolved ground surface instead of floating/slicing visually.
- Building LOD2 quality is not claimed: the project decision history says the current base map is a Buildings/LOD1 import. Lighting and shader fallback were improved without reimporting map data.

JSON: `Assets/Data/P10/newmap_manual_blocker_audit.json`
