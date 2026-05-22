# P7-C High-Detail Chuo Scene Plan

## Scope

P7-C now prepares a real high-detail Chuo test/demo scene target in addition to the P7Benchmark chunk-loading sandbox.

The scene target is:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

This scene is separate from `Assets/Scenes/Chuo_BaseMap.unity`. P7-C must not modify `Chuo_BaseMap.unity`.

## Scene Purpose

`P7_HighDetail_Chuo.unity` is the intended P7-C high-detail preparation scene and P7-D Windows EXE profiling target. P7-D must not profile only the old `P7_Benchmark_Skeleton.unity` or an empty test scene as the final high-detail result.

The scene shell must include explicit roots for:

- Buildings
- Roads
- Bridges
- Underground
- CityFurniture
- Water
- Vegetation
- Relief
- DisasterRisk
- LandUse
- UrbanPlanningDecision
- P2P6Compatibility

## Current Implementation Status

P7-C creates the scene shell and metadata roots if full PLATEAU SDK import cannot be automated safely. The shell is not evidence that high-detail assets are already loaded.

Actual PLATEAU import status must be determined from Unity scene/renderable object evidence, not from target settings screenshots. Screenshots and SDK option notes are treated as target settings only.

## Required Layer Coverage

The scene must be ready to include:

- buildings at target LOD3
- roads at target LOD3
- bridges at target LOD3
- underground city, streets, and spaces at target LOD3 if available
- city furniture at target LOD2 or LOD3 depending availability and performance
- water body at target LOD1
- vegetation at target LOD3 if available
- relief/terrain
- disaster risk data
- land use
- urban planning decision information at target LOD1

No LOD4 availability is assumed.

## P7-D Handoff

P7-D must profile `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` after actual high-detail assets are loaded or explicitly confirm that import remains blocked. If the scene contains only placeholders and metadata, P7-D must not call the EXE profiling result final.

## P8/P9/P10 Baseline Intent

If P7-D confirms compatibility and Windows EXE performance, P8, P9, and P10 should use this new high-detail scene as the default baseline. `Chuo_BaseMap.unity` becomes a legacy fallback only if P7-D passes the new scene.

P7-C does not implement P8 tsunami systems, P9 crowd systems, or P10 release packaging.
