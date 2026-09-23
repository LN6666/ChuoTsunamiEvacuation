# P7-C Bridge / Underground / Road Feasibility

## Current Status

Bridge, underground, and road categories are target settings for P7-C high-detail import. They are not currently verified as loaded, renderable Unity objects in `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.

| Category | Source evidence | Loaded in high-detail scene | Renderable evidence | P7-D suitability |
|---|---|---|---|---|
| Roads | `udx\tran` exists locally | Not verified | Not verified | Pending actual import |
| Bridges | `udx\brid` exists locally | Not verified | Not verified | Pending actual import |
| Underground | `udx\ubld` exists locally | Not verified | Not verified | Pending actual import |

## Feasibility Notes

- Roads should support future visibility and routing context, but P7-C does not replace P2-P6 movement or routing logic.
- Bridges may be important for visual and navigation context, but must be checked for mesh/material count and collider policy.
- Underground spaces may be large and expensive. They require explicit visibility grouping and should not receive complex MeshColliders by default.

## Required Follow-Up

Manual Unity inspection or SDK import automation must confirm:

- category objects exist in the high-detail scene
- renderers are present
- expected LODs are available
- material and texture counts are measurable
- chunks or layer roots can be toggled safely

Until those checks pass, bridge, underground, and road readiness remains pending.
