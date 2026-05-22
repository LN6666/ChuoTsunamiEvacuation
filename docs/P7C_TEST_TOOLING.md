# P7-C Test Tooling

## Tooling Added Or Updated

P7-C uses these validation tools:

- `tools/p7/run_p7c_preflight.ps1`
- `tools/p7/inspect_p7c_benchmark_import.ps1`
- `tools/p7/inspect_p7c_high_detail_scene.ps1`
- `tools/p7/inspect_p7c_plateau_import_readiness.ps1`
- `tools/p7/inspect_p7c_p2_p6_compatibility.ps1`
- `tools/p7/build_p7c_high_detail_scene.ps1`

## Scene Readiness Validator

`inspect_p7c_high_detail_scene.ps1` checks that `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` exists, contains required high-detail layer roots, contains P7-D profiling metadata, and does not reference protected production assets.

## PLATEAU / LOD Coverage Validator

`inspect_p7c_plateau_import_readiness.ps1` prints a category table with target LOD, source evidence, actual Unity evidence, and status. It does not claim average LOD3 unless renderable evidence supports it.

## P2-P6 Compatibility Validator

`inspect_p7c_p2_p6_compatibility.ps1` checks for source and scene-marker evidence for player movement, camera, GameManager, result panel, shelter interaction, P5 data loaders, P6 navigation guidance, and the P6 NPC prototype.

Runtime validation remains pending until actual high-detail assets are loaded.

## Unity Tests

EditMode and PlayMode tests under `Assets/Tests/*/P7Benchmark/` validate:

- P7Benchmark chunk/metrics components remain scene-independent.
- P7HighDetail scene metadata does not depend on `Chuo_BaseMap`.
- P7HighDetail metadata keeps PLATEAU import and runtime smoke status explicit.
- P7Benchmark/P7HighDetail helpers do not modify gameplay success/failure state.

## Preflight Integration

`run_p7c_preflight.ps1` runs the P7-C scope guard, benchmark import inspection, high-detail scene inspection, PLATEAU/LOD readiness inspection, P2-P6 compatibility inspection, protected path checks, and benchmark scene sandbox checks.
