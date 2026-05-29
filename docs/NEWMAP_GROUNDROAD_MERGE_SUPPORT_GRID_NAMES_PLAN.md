# NewMap Ground/Road Merge Support Grid Names Plan

## Confirmed Scope

This plan implements the user-confirmed NewMap ground/road merge task using `Assets/Scenes/Chuo_GroundRoad_Import_Source.unity` as a supplemental sampling source. It does not re-import PLATEAU data and does not blindly merge the imported source scene into `Assets/Scenes/Chuo_BaseMap.unity`.

## Implementation Steps

1. Validate the supplemental source scene with Unity Editor APIs and write source validation JSON/Markdown.
2. Generate ground-height samples from imported road/terrain/relief/bridge renderers and colliders, with water marked non-walkable and Chuo_BaseMap building bases used only as fallback samples.
3. Add runtime adaptive support-grid loading from the generated local sample cache.
4. Replace the single global support plane as the primary solution with invisible per-cell support colliders under `GameplaySupportRoot`.
5. Snap player spawn, NPC spawn, active targets, green frames, route markers, and interaction anchors to local support height when possible.
6. Keep support/grid renderers disabled in normal gameplay and audit remaining blue/debug ground candidates.
7. Preserve existing mouse drag look, spawn rejection, lighting, air walls, official/non-official semantics, and P2-P10 flow.
8. Keep name enrichment offline at runtime; online lookup is allowed only in preprocessing tools and writes a local cache.
9. Generate the requested reports, preflight tools, build/log tools, tests, and DeepSeek review prompts.

## Constraints

- Do not commit the user-imported supplemental source scene data.
- Do not overwrite or delete imported buildings or map assets.
- Do not claim official routes, GIS-grade validation, or terrain accuracy beyond proxy sampling.
- Do not create a final release/archive, PBL11, or P10-E/F/G.
