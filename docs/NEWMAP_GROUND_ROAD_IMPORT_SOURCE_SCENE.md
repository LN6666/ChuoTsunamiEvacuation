# NewMap Ground/Road Import Source Scene

## Purpose

`Assets/Scenes/Chuo_GroundRoad_Import_Source.unity` is a separate supplemental source scene for manually importing PLATEAU ground, road, terrain, relief, bridge, and water data.

It exists to keep `Assets/Scenes/Chuo_BaseMap.unity` stable while the user performs a narrow PLATEAU SDK import for ground and road support data.

## Scope

This scene is only a staging source for future merge or sampling work.

No PLATEAU data has been imported as part of this preparation task. The scene is intentionally empty except for root GameObjects that provide clear import targets.

## Root Objects

| Root | Intended Use |
|---|---|
| `GroundRoadImportRoot` | General parent or marker for this supplemental import scene. |
| `ImportedRoadRoot` | Target for road, transportation, or `tran` data. |
| `ImportedTerrainRoot` | Target for terrain data if the SDK separates it from relief/DEM. |
| `ImportedReliefRoot` | Target for relief or DEM data. |
| `ImportedBridgeRoot` | Target for bridge data if needed for road continuity. |
| `ImportedWaterRoot` | Target for water data if needed for coastline, river, canal, or visual reference. |
| `ImportDiagnosticsRoot` | Target for temporary import notes, bounds markers, or diagnostics. |

## Constraints

- Do not import buildings or `bldg` into this source scene.
- Do not import city furniture, vegetation, underground, or other heavy unrelated layers unless explicitly needed.
- Do not overwrite or save over `Assets/Scenes/Chuo_BaseMap.unity`.
- Do not delete or overwrite existing map assets.
- Do not create release or archive outputs from this preparation step.
- Do not create P10-E, P10-F, or P10-G work items from this preparation step.

## Validation

From the project root:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_ground_road_import_source_preflight.ps1
```

The preflight checks that the supplemental source scene exists, the base map still exists, the base map was not replaced by this small source scene, the required import roots are present, and no forbidden release/archive or P10-E/F/G artifacts are present.
