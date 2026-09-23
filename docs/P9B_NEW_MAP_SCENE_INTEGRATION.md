# P9-B New-Map Scene Integration

P9-B adds a scene-safe runtime integration layer for the P7 high-detail baseline.

Implemented runtime layer:

- `P9SceneRuntimeBootstrap`
- `P9SpawnRuntimeGenerator`
- `P9RuntimeMarker`
- runtime-only weighted spawn markers
- runtime-only entrance/safe-floor markers
- runtime-only humanitarian candidate markers
- runtime-only collapse/debris risk zone markers

The layer generates GameObjects at runtime or in temporary test scenes. It does not edit `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`, `Assets/Scenes/Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, or `Assets/PLATEAU`.

P8-E handoff files consumed by P9-B:

- `Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json`
- `Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json`
- `Assets/Data/P8/p8e_semantic_binding_v1.json`
- `Assets/Data/P8/p8e_p2_p6_new_map_adaptation_matrix.json`
- `Assets/Data/P8/p8e_route_candidate_geometry_handoff.json`
- `Assets/Data/P8/risk_front_progression_model_config.json`

Scene-object semantic coverage remains proxy/data-level. P9-B uses P8-E semantic binding v1 for gameplay-usable proxy anchoring only and does not claim full scene-object binding.
