# NewMap Debug Object Cleanup

Generated: 2026-05-29T00:00:00+09:00

Status: `implemented_pending_manual_visual_confirmation`

Normal manual mode no longer spawns visible local training proxy targets. Those targets remain available only for diagnostics through `NewMapRuntimeBootstrap.EnableLocalTrainingProxyTargetsForDiagnostics` or `-newmapSelfAuditSmoke`, preserving P2-P10 automated regression paths without showing test markers in the production/manual scene.

`DebugDiagnosticsRoot` is set inactive by runtime bootstrap. The gameplay collision support surface moved to `GameplaySupportRoot`, stays active for collision, and has its renderer disabled.

Required gameplay visuals remain: official markers, non-official candidate warnings, green frames, active route guidance, Stage 2 hazard/light curtain, NPCs, and UI.

JSON: `Assets/Data/P10/newmap_debug_object_cleanup_report.json`
