# New Map Chuo_BaseMap Baseline

## Scene

Baseline scene path:

```text
Assets/Scenes/Chuo_BaseMap.unity
```

The scene was created because the `.unity` file was missing and only `Assets/Scenes/Chuo_BaseMap.unity.meta` existed.

## Root Objects

The baseline scene contains these root GameObjects:

| Root | Intended Use |
|---|---|
| `MapRoot` | Parent for imported PLATEAU geometry. |
| `RuntimeSystemsRoot` | Parent for future runtime controllers. |
| `PlayerSpawnRoot` | Parent for future player spawn markers. |
| `ShelterMarkerRoot` | Parent for future shelter markers and proxies. |
| `HazardVisualRoot` | Parent for future tsunami/risk visualization objects. |
| `NavigationRoot` | Parent for future navigation helpers and baked/runtime navigation objects. |
| `CrowdRoot` | Reserved parent for future crowd-related placeholders, if explicitly planned. |
| `UIAnchorRoot` | Parent for future scene UI anchors. |
| `DebugDiagnosticsRoot` | Parent for future debug diagnostics objects. |

## Baseline Constraints

This baseline is intentionally empty:

- No PLATEAU data is imported.
- No scripts are attached.
- No real map data is generated.
- No generated scene backup is deleted.
- No P2-P10 gameplay systems are reconnected.

The baseline exists only to give the fresh map import a clean scene target and stable root hierarchy.

## Validation

Run:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_preflight.ps1
```

The preflight should pass before any manual PLATEAU import begins.
