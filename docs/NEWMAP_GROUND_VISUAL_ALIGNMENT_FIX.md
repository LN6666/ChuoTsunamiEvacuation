# NewMap Ground Visual Alignment Fix

Generated: 2026-05-29T00:00:00+09:00

Status: `implemented_pending_player_build_validation`

The player now spawns at the resolved ground surface plus a small `0.04m` skin offset. The runtime collision support proxy is invisible and its top surface is aligned to that same ground height.

Changes:

- Removed the previous `+1.15m` player/support offset.
- Runtime support renderer is disabled.
- Local training proxy target Y offsets were removed in diagnostics mode.
- Fall recovery remains a safety net only.

JSON: `Assets/Data/P10/newmap_ground_visual_alignment_status.json`
