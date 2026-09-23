# NewMap Building Clipping Fix

Generated: 2026-05-29T00:00:00+09:00

Status: `runtime_clipping_sources_fixed_pending_manual_scene_confirmation`

The confirmed runtime clipping source was the visible support surface and old proxy visuals. The support surface is now invisible and aligned to gameplay ground. Local training proxy targets are diagnostics-only in normal manual mode.

Imported PLATEAU geometry was not moved. If building self-intersections remain visible after this pass, they are classified as imported scene/data limitations until a targeted PLATEAU import audit proves otherwise.

JSON: `Assets/Data/P10/newmap_building_clipping_status.json`
