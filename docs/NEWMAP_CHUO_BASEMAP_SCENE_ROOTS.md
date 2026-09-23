# NewMap Chuo_BaseMap Scene Roots

Required roots:

- `MapRoot`
- `RuntimeSystemsRoot`
- `PlayerSpawnRoot`
- `ShelterMarkerRoot`
- `CandidateMarkerRoot`
- `HazardVisualRoot`
- `NavigationRoot`
- `CrowdRoot`
- `CollapseDebrisRoot`
- `GreenFrameRoot`
- `UIAnchorRoot`
- `DebugDiagnosticsRoot`
- `PerformanceMetricsRoot`

The runtime bootstrap calls `NewMapRuntimeBootstrap.EnsureRoots()` when `Chuo_BaseMap` loads. The editor command `NewMapSceneSetupUtility.EnsureChuoBaseMapRuntimeSetupCommandLine` also creates missing scene roots and saves the scene when run from Unity.

Imported map objects must not be destroyed or moved by this setup. Runtime systems are organized under the named roots.
