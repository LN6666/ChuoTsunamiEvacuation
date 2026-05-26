# NewMap Full Integration Audit

Audit date: 2026-05-26

Workspace: `D:\UnityProjects\ChuoTsunamiEvacuation`

Branch: `phase5-qualification-routing-plateau`

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

Unity version: `6000.4.6f1`

PLATEAU SDK: `com.synesthesias.plateau-unity-sdk` from `file:D:/UnityPackages/PLATEAU-SDK-for-Unity-v4.2.0.3.tgz`

## Scene

`Chuo_BaseMap.unity` exists.

Scene size: `12,157,291,044` bytes, about 11.32 GiB.

Streaming scene scan result:

| Metric | Count |
|---|---:|
| GameObjects | 44,204 |
| MeshRenderers | 44,115 |
| MeshFilters | 44,115 |
| MeshColliders | 44,115 |
| Cameras before runtime bootstrap | 0 |
| Canvases before runtime bootstrap | 0 |

Unity editor scene audit after root setup:

| Metric | Count |
|---|---:|
| Required roots | 13 |
| Missing roots | 0 |
| Loaded Renderers | 18,596 |
| Loaded Colliders | 18,596 |

Unity log observation: loading the scene during editor setup reported about 6.11 GB memory in use after scene load.

The scene object names are UUID-style `bldg_...` and `tran_...`. They do not expose the previous P5 `13102-bldg-*` IDs, so old qualified shelter/building targets cannot be honestly attached by ID.

## Existing Systems

The worktree contains P2-P5 era runtime code, data loaders, result UI, shelter interaction, tsunami wall/risk prototypes, P5 static data loaders, and P5-D real-qualified runtime generators.

No existing `Assets/Data/P10` folder was present before this task.

## Initial Root State

Present in the scene before new full integration tooling:

- `MapRoot`
- `RuntimeSystemsRoot`
- `PlayerSpawnRoot`
- `ShelterMarkerRoot`
- `HazardVisualRoot`
- `NavigationRoot`
- `CrowdRoot`
- `UIAnchorRoot`
- `DebugDiagnosticsRoot`

Missing before setup:

- `CandidateMarkerRoot`
- `CollapseDebrisRoot`
- `GreenFrameRoot`
- `PerformanceMetricsRoot`

The new editor/runtime setup ensures all required roots exist.

## Target Policy

Old P3/P4/P5/route targets are preserved in data and documentation but are disabled when no verified new-map anchor exists. They are not spawned as active gameplay targets.

The active gameplay targets are three local non-official runtime training proxies under the new map bootstrap. They are documented proxies for E interaction, safe-floor success, entrance-blocked failure, and safe-floor failure.
