# P10-C-Pre Player Log Diagnostics

Expected Player.log markers:

- `P10CPreStartupDiagnostics`
- active startup scene path
- target high-detail scene path
- whether the target scene is active/loadable
- pre-bootstrap renderer count
- placeholder-likely status
- data root and P8/P9/P10 folder status
- localization load status
- camera/player/EventSystem/Canvas counts
- shelter and entrance counts
- P4/P5/P6/P8/P9/P10 integration counts
- profiling exporter and auto-quit settings

Latest scripted verification evidence:

- `phase=startup_menu_built` appears before gameplay bootstrap.
- `phase=start_game_completed` appears after Start Game.
- Final counts include `players=1`, `cameras=1`, `resultPanels=1`, `entrances=34`.
- Final integration counts include `p4RealShelters=5`, `p5Qualified=27`, `p5Candidates=5`, `p6Npc=1`, `p8RiskFrontLoaded=True`, `p9Crowd=16`, `p9SpawnMarkers=12`, `p9EntranceSafeFloorMarkers=4`, `p9CollapseDebrisZones=3`, and `p10GreenFrames=2`.
- `placeholderLikely=True` is expected in this worktree because the tracked P9 scene is the small placeholder/status shell, not the protected 22.5 GB P7 source scene.

Blank startup is treated as a bug. Missing gameplay references must produce Player.log diagnostics and visible UI text.
