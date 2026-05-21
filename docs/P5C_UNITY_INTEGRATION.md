# P5-C Unity Integration

## Status

P5-B is complete. The P5-B5 DeepSeek review passed with no A-level blockers.

P5-C is complete as a minimal, read-only Unity integration for qualified buildings, estimated route metadata, and decision feedback.

The P5-C DeepSeek review completed on 2026-05-20 with PASS and B-level follow-ups only. No A-level blockers were found.

## Runtime Data Boundary

Unity runtime reads copied static JSON only from `Assets/Data/`:

- `Assets/Data/real_chuo_integrated_route_qualification.json`
- `Assets/Data/real_chuo_osm_routes_sample.json`
- `Assets/Data/real_chuo_building_qualification.json`
- `Assets/Data/real_chuo_shelter_building_matches.json`

Unity runtime does not read `data_pipeline/processed`, `data_pipeline/raw`, `data_pipeline/downloads`, `data_pipeline/cache`, `tmp`, `.venv`, OSM cache files, raw PLATEAU data, or live routing/GIS sources.

The committed default remains:

- `sourceMode = test`
- `enableP5COverlay = false`

## Mapping

Observed P5-B root shape:

- all four Unity-copied P5-C files use top-level `records`
- route sample records are under `records`, not `routes`
- route geometry is GeoJSON-like `LineString` with WGS84 `[longitude, latitude]` pairs

Primary Unity lookup:

- selected shelter/building key: `shelterId`
- integrated qualification route link: `nearestRouteId`
- route record key: `routeId`
- PLATEAU evidence key preserved: `plateauBuildingId`

Fallback:

- if `shelterId` has no P5-C integrated record, UI feedback returns `P5-C qualification/route data unavailable for this shelter`
- if `nearestRouteId` is absent or cannot be resolved, no route record is inferred from other routes
- no unsafe shelter/building/route mapping is invented
- current `real_sample` shelter IDs are not assumed to safely map to P5-B official shelter IDs, so the unavailable evidence fallback is expected when no exact safe `shelterId` match exists

## Visualization

P5-C visualization is prototype/debug visualization only.

When manually enabled with `sourceMode = real_sample` and `enableP5COverlay = true`, Unity generates a limited collider-free marker subset on the existing schematic debug platform.

Marker labels expose:

- qualification status
- confidence
- manual review flag
- route distance/time when available
- warnings in detailed metadata
- estimated prototype route labeling

Route line behavior:

- route geometry is loaded and preserved as WGS84 `LineString` data
- route lines are not rendered for the current data because P4 has no verified WGS84-to-Unity/PLATEAU coordinate transform
- this avoids drawing misleading route geometry
- all 135 routes are not drawn by default

The existing `H` hazard toggle and `M` metadata/details toggle remain the debug controls. P5-C markers have no gameplay colliders and do not affect success/failure.

## Decision Feedback

For non-test shelter sources, ResultPanel detail text can append a concise P5-C block:

```text
Evidence qualification:
{qualificationStatus} ({classification}) / {confidence}
Manual review: {yes/no}

Estimated prototype route:
{distanceMeters} m / {estimatedTimeSeconds} s
estimated prototype route, not an official evacuation route
Not an official evacuation route.
OSM/ODbL attribution applies.
```

Long warning lists are truncated to the first two warnings with a `+N more warnings` line.

P5-C qualification, route, and hazard information is informational only. It does not determine gameplay success/failure, shelter enterability, climb result, scenario logic, or tsunami risk behavior.

## Scope Boundaries

P5-C does not implement:

- real-time routing
- live OSM/web requests
- official evacuation route guidance
- flood simulation
- disaster road closure modeling
- NPC/crowd behavior
- gameplay-rule rewrite
- PLATEAU scene modification

OSM routes are estimated prototype routes, not official evacuation routes. OSM/ODbL attribution is preserved in Unity data, UI metadata, and this documentation.

## Manual Validation Checklist

### Batchmode Test Limitation

Validation attempt on 2026-05-20:

- `powershell -ExecutionPolicy Bypass -File tools\run_unity_tests.ps1 -Mode EditMode`
- `powershell -ExecutionPolicy Bypass -File tools\run_unity_tests.ps1 -Mode PlayMode`

Both commands launched Unity 6000.4.6f1 but failed at the wrapper level with a blank Unity exit code and did not create:

- `test-results/editmode-results.xml`
- `test-results/playmode-results.xml`

Checked log:

- `C:\Users\liangjn\AppData\Local\Unity\Editor\Editor.log`

Log findings:

- Unity loaded the project and opened `Assets/Scenes/Chuo_BaseMap.unity`
- script compilation ran and reported compiler `ExitCode: 0`
- `ChuoTsunamiEvacuation.EditModeTests.dll` and `ChuoTsunamiEvacuation.PlayModeTests.dll` were processed
- no Unity Test Runner XML or test-run summary was emitted

Do not treat this as a passing automated test run. Use the Unity Editor Test Runner for EditMode and PlayMode validation until the batchmode/XML issue is fixed.

1. Confirm `Assets/Data/shelter_source_config.json` has `sourceMode = test`.
2. Enter Play Mode and confirm default test shelter entry, climb, and ResultPanel flow still work.
3. Press `H` and confirm hazard debug visualization still toggles.
4. Press `M` in a metadata-enabled debug run and confirm metadata/details still toggle.
5. Temporarily set `sourceMode = real_sample` and `enableP5COverlay = true`.
6. Enter Play Mode and confirm P5-C qualified markers appear without colliders.
7. Confirm route lines do not render for WGS84 data and route distance/time remain visible in metadata.
8. Confirm ResultPanel P5-C feedback appears for matching non-test shelter IDs, or shows the unavailable fallback when no safe mapping exists.
9. Confirm route/hazard/qualification data does not change success/failure.
10. Restore `sourceMode = test` and `enableP5COverlay = false` before committing.

## Future Follow-Ups

- decide whether P5-C evidence should move from debug/metadata visibility into the main ResultPanel for all players
- add a defensive formatter guard or confirm asmdef dependency guarantees if assembly boundaries change
- refine nearest-match semantic confidence logic
- add an artificial out-of-network route failure test
- repeat QGIS spot checks before publication/user-facing use
- define a verified Unity/PLATEAU coordinate transform before rendering WGS84 route lines
