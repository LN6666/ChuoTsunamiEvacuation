# NewMap Ground Cover Raise To Building Base

Decision: the accepted visible gameplay ground cover remains the playable ground standard. For this pass, imported buildings are fixed visual references and are not randomly moved.

Implementation:
- Runtime samples building-like renderer base heights and computes a robust raise offset for the current gameplay ground cover.
- The raise is capped at 10m by `Assets/Data/P10/newmap_ground_cover_raise_config.json`.
- Ground cover visual tiles, colliders, support surface, player spawn, NPC spawn, target markers, green frames, interaction zones, and air-wall vertical placement follow the raised support Y.
- Existing building snapdown and Round3 building-road vertical movement are disabled while the ground-raise config is enabled.

This is gameplay visual alignment only. It does not claim GIS-grade terrain, road, or PLATEAU elevation accuracy.

Player-smoke result:
- old cover Y: 0m
- selected raised cover Y: 3m
- sampled building bases: 10,787
- remaining average positive gap: 0.45m
- remaining max sampled positive gap: 3.12m
- buildings moved by this pass: 0
- Player.log: 0 errors / 0 warnings
