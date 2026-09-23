# NewMap Final Ground Micro Raise

- Active scene: `Assets/Scenes/Chuo_BaseMap.unity`
- Config: `Assets/Data/P10/newmap_final_ground_micro_raise_config.json`
- Report: `Assets/Data/P10/newmap_final_ground_micro_raise_report.json`

The accepted ground-cover raise remains the base behavior. This final pass adds a small `0.3m` configurable micro-raise on top of the current gameplay ground/support Y. Imported PLATEAU building geometry is not moved.

Expected runtime values:
- Previous gameplay ground Y: `3.0`
- Additional raise: `0.3m`
- New gameplay ground/support Y: `3.3`
- Player, NPCs, active targets, green frames, interaction zones, route markers, and air-wall vertical coverage are resnapped through the existing runtime support-ground path.

This is a visual/gameplay support adjustment only. It does not claim GIS-grade terrain or building-base validation.
