# NewMap Material LOD Visual Quality Fix

Generated: 2026-05-29T00:00:00+09:00

Status: `fixed_lighting_and_shader_fallback_with_documented_lod1_limitation`

The safe fix in this pass is lighting and runtime shader fallback. Generated runtime materials resolve shaders in this order: URP Lit, Standard, Unlit/Color, Sprites/Default.

The current base map cannot honestly be claimed as LOD2 texture quality. `docs/DECISIONS.md` records the first import as Buildings/LOD1, with LOD2 and textures out of scope for that import. No map data was reimported and no imported materials were bulk-mutated.

Remaining limitation: if a future manual visual target requires LOD2 textured buildings, that needs an approved PLATEAU import/material conversion task.

JSON: `Assets/Data/P10/newmap_material_visual_quality_status.json`
