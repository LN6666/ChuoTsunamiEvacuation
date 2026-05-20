# DeepSeek V4 Pro Review Prompt: Phase 5-C Unity Integration

Project:
ChuoTsunamiEvacuation / PBL-ALL

Branch:
phase5-qualification-routing-plateau

Review scope:
Review the current P5-C Unity integration diff only. P5-C is a minimal, read-only Unity integration of P5-B qualified building and estimated route outputs. Do not request new gameplay features, route rendering, live routing, flood simulation, NPC behavior, scene-wide refactors, or a UI rewrite.

Upstream status:
- P5-B5 DeepSeek review: PASS.
- P5-B5 had no A-level blockers.
- P5-B can be treated as complete for this review.

P5-C implementation summary:
- Static P5-B JSON outputs were copied into `Assets/Data`.
- Unity runtime reads copied static JSON from `Assets/Data` only.
- Unity runtime has no dependency on `data_pipeline/processed`, `data_pipeline/raw`, `data_pipeline/downloads`, or `data_pipeline/cache`.
- `sourceMode` remains `"test"` by default.
- `enableP5COverlay` remains `false` by default.
- P5-C data is informational only and does not affect gameplay success/failure.
- OSM route text preserves `"estimated prototype route, not an official evacuation route"`.
- OSM/ODbL attribution is preserved in Unity data/UI metadata/docs.
- Route line rendering was not attempted because WGS84 route geometry to Unity/PLATEAU coordinate conversion is not yet safely verified.

Unity-side static data files:
- `Assets/Data/real_chuo_integrated_route_qualification.json`
- `Assets/Data/real_chuo_osm_routes_sample.json`
- `Assets/Data/real_chuo_building_qualification.json`
- `Assets/Data/real_chuo_shelter_building_matches.json`

Key implementation files to review:
- `Assets/Scripts/Data/P5CStaticDataLoader.cs`
- `Assets/Scripts/Data/P5CDecisionFeedbackFormatter.cs`
- `Assets/Scripts/Gameplay/P5CQualificationOverlayRuntimeGenerator.cs`
- `Assets/Scripts/Core/EvacuationGameManager.cs`
- `Assets/Scripts/Result/ResultMetrics.cs`
- `Assets/Scripts/Data/ShelterSourceConfigLoader.cs`
- `Assets/Data/shelter_source_config.json`
- `Assets/Tests/EditMode/P5CStaticDataLoaderTests.cs`
- `Assets/Tests/PlayMode/P5CDebugLayerPlayModeTests.cs`
- `docs/P5C_UNITY_INTEGRATION.md`
- `docs/P5_WORKPLAN.md`
- `docs/PROGRESS_LOG.md`
- `docs/REVIEW_BACKLOG.md`
- `docs/TASKS.md`

Safety boundaries confirmed before review:
- `Chuo_BaseMap.unity` untouched.
- PLATEAU imported files untouched.
- `ProjectSettings` untouched.
- `Packages` untouched.
- No raw/cache/download/tmp/.venv/large GIS files modified.
- Route/hazard/qualification information is informational only.
- Route/hazard/qualification information does not determine success/failure, shelter enterability, climb result, scenario logic, or tsunami risk behavior.
- Runtime P5-C loading rejects data-pipeline/raw/cache/download/tmp paths.
- P5-C route geometry is loaded as evidence metadata but not rendered as Unity route lines.

Validation status:
- Manual Unity Editor Test Runner, EditMode: 89 passed / 0 failed.
- Manual Unity Editor Test Runner, PlayMode: 8 passed / 0 failed.
- After the P5-C dataset-id assertion fix, the four previously failing P5-C representative parsing tests were rerun: 4 passed / 0 failed.
- Filtered P5-C EditMode fixture after assertion fix: 14 passed / 0 failed.
- Full EditMode rerun after assertion fix: 89 passed / 0 failed.
- XML produced for the filtered P5-C rerun: `test-results/editmode-p5c-results.xml`.
- XML produced for the full EditMode rerun: `test-results/editmode-results.xml`.

Known B-level limitations / follow-ups:
- WGS84 route geometry is loaded but not rendered until the Unity/PLATEAU coordinate transform is verified.
- Future route test should include an artificial out-of-network route failure case.
- Repeat QGIS spot checks before publication or user-facing use.
- Nearest-match semantic confidence logic can be refined later.

Review request:
Please review the current P5-C implementation and documentation for A-level blockers and B-level issues.

A-level blockers to check:
- Any runtime read from `data_pipeline/processed`, raw, downloads, cache, tmp, .venv, OSM cache, live GIS, or live routing sources.
- Any modification or accidental dependency on `Chuo_BaseMap.unity`, PLATEAU imported assets, `ProjectSettings`, or `Packages`.
- `sourceMode` default changed away from `"test"`.
- `enableP5COverlay` default changed away from `false`.
- Route, hazard, or qualification data influencing success/failure, shelter enterability, climb result, scenario logic, or tsunami risk behavior.
- OSM route outputs presented as official evacuation routes.
- OSM/ODbL attribution missing from Unity-side data/UI metadata/docs.
- Unsafe shelter/building/route ID inference or invented mappings.
- Route geometry rendered despite unverified WGS84-to-Unity/PLATEAU coordinate conversion.
- Existing gameplay regressions in player movement, camera, tsunami risk wall, scenario system, shelter entry, stair climb, result flow, `H` hazard toggle, or `M` metadata/details toggle.

B-level issues to check:
- Loader failure-mode clarity for missing or malformed optional P5-C data.
- Confidence/manual-review/warning field preservation and presentation.
- ResultPanel wording clarity and warning truncation behavior.
- Marker overlay clutter or unintended colliders/interactions.
- Test coverage gaps around route failures and unavailable mappings.
- Documentation clarity around prototype/debug visualization and non-official route status.
- Any remaining ambiguity in shelter/building/route ID mapping.

Scope/boundary checks:
- P5-C should remain static evidence-data loading, conservative prototype/debug visualization, and decision feedback only.
- P5-C should not implement real-time routing, official navigation, flood simulation, NPC/crowd behavior, mobile support, multiplayer, or a gameplay-rule rewrite.
- P5-C should not render route lines until coordinate conversion is safely verified.

Please conclude with:
1. Verdict: PASS / PASS WITH B-LEVEL ISSUES / FAIL.
2. A-level blockers, if any.
3. B-level issues and recommended follow-up tasks.
4. Whether P5-C can be marked complete.
5. Whether the implementation is safe to commit after review findings are addressed.
