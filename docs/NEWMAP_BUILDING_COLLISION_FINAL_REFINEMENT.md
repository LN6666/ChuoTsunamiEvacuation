# NewMap Building Collision Final Refinement

- Config: `Assets/Data/P10/newmap_building_collision_final_refinement.json`
- Report: `Assets/Data/P10/newmap_building_collision_final_refinement_report.json`

This pass keeps the accepted tight building footprint proxy system and adds a final refinement layer:

- Default X/Z shrink target: `0.85`
- Oversized/road-adjacent proxy shrink target: `0.75`
- Max proxy piece size: `60m`
- Target clearance: at least `4m`
- Spawn clearance: `6m`
- Oversized proxies are split when possible; if a proxy still cannot be made safe for approaches, it can be disabled.

The runtime still preserves building blocking via player building-avoidance bounds. This does not re-enable old inflated scene colliders and does not claim GIS-grade building footprints.
