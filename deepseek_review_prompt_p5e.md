# DeepSeek Review Prompt: P5-E Route Geometry Rendering QA

You are reviewing the Unity project `ChuoTsunamiEvacuation` on branch `p5e-route-geometry-rendering`.

Phase:

P5-E - Verified Route Geometry Rendering + Real Gameplay QA 1.0.

Review goal:

Check whether Codex safely strengthened route geometry parsing, WGS84 transform validation, selected/limited route-preview rendering behavior, and real_qualified gameplay QA without violating Phase 5 safety boundaries.

Important scope boundaries:

- `sourceMode` default must remain `test`.
- `real_qualified` remains opt-in.
- Do not modify `Chuo_BaseMap.unity`.
- Do not modify PLATEAU imported files.
- Do not modify `ProjectSettings` or `Packages`.
- Unity runtime must read copied static JSON from `Assets/Data` only.
- Unity runtime must not read `data_pipeline/processed`, `raw`, `downloads`, `cache`, `tmp`, `.venv`, OSM cache files, raw PLATEAU data, or live GIS/routing sources.
- No live routing, web requests, official navigation, flood simulation, NPC/crowd simulation, or route-based success/failure.
- OSM routes must remain "estimated prototype route, not official evacuation route".
- OSM/ODbL attribution must be preserved.
- Do not render all 135 routes by default.
- Do not render route lines unless coordinate transform validation passes.

Files to review closely:

- `Assets/Scripts/Data/P5CStaticDataLoader.cs`
- `Assets/Scripts/Data/P5DRoutePreviewTransformValidator.cs`
- `Assets/Scripts/Gameplay/P5DRealQualifiedShelterRuntimeGenerator.cs`
- `Assets/Scripts/Gameplay/P5DRoutePreviewMetadata.cs`
- `Assets/Scripts/Data/RealQualifiedShelterDataLoader.cs`
- `Assets/Scripts/Data/RealQualifiedShelterFeedbackFormatter.cs`
- `Assets/Scripts/Result/ResultPanelController.cs`
- `Assets/Tests/EditMode/P5CStaticDataLoaderTests.cs`
- `Assets/Tests/EditMode/P5DRealQualifiedGameplayDataTests.cs`
- `Assets/Tests/PlayMode/P5DRealQualifiedGameplayPlayModeTests.cs`
- `docs/P5E_ROUTE_RENDERING_QA.md`
- `docs/P5_WORKPLAN.md`
- `docs/TASKS.md`
- `docs/PROGRESS_LOG.md`
- `docs/REVIEW_BACKLOG.md`
- `docs/UNITY_TESTING_WORKFLOW.md`

Key facts from P5-E implementation:

- Actual route geometry in `Assets/Data/real_chuo_osm_routes_sample.json` is GeoJSON-like `LineString`.
- Coordinate reference system is `EPSG:4326`.
- Coordinate order is `[longitude, latitude]`.
- Route ids use `routeId`.
- Origin ids use `originId`.
- Target ids are represented by `shelterId` and `plateauBuildingId`; there is no separate `targetId` field.
- Distance/time fields are `routeDistanceMeters` and `estimatedTravelTimeSeconds`.
- Current Unity runtime code has no verified WGS84-to-Unity/PLATEAU world-coordinate transform.
- Therefore current real `EPSG:4326` route line rendering should produce zero line objects.
- Distance/time feedback should remain visible even when line rendering is disabled.

Please check for:

1. Compile risks or Unity lifecycle risks.
2. NullReferenceException risks in loader, validator, runtime generator, metadata, and tests.
3. Whether malformed route geometry fails safely without crashing or creating renderable line geometry.
4. Whether WGS84 validation catches swapped lon/lat, out-of-bounds, collapsed, or implausible route geometry.
5. Whether current `EPSG:4326` real route rendering remains disabled until a verified transform exists.
6. Whether selected/limited route preview behavior avoids rendering all 135 routes by default.
7. Whether `real_qualified` status policy remains correct:
   - `official_confirmed` selectable.
   - `official_confirmed_with_review` selectable.
   - candidate/unknown/not-qualified statuses non-playable/debug-only.
8. Whether route/qualification/hazard metadata remain feedback-only and cannot affect success/failure.
9. Whether OSM/ODbL attribution and estimated prototype route disclaimers are preserved.
10. Whether docs accurately reflect the code and current limitations.

Validation to consider:

GUI/headful automated Unity tests should be run with:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Expected review output:

- Verdict: PASS / PASS WITH B-LEVEL FOLLOW-UPS / BLOCKED.
- A-level blockers first, if any.
- B-level follow-ups next.
- Explicit notes on route transform safety, route rendering status, source-mode safety, and gameplay-rule safety.

