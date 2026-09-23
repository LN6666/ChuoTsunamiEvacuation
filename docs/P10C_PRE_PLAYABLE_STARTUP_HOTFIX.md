# P10-C-Pre Playable Startup Hotfix

Status: implemented and locally validated. Preflight, EditMode, PlayMode, temporary build, actual P7 integration verification, and DeepSeek review pass with the documented placeholder-scene limitation.

The temporary P10-C-Pre player now defaults to playable startup mode. A player-only runtime bootstrap creates a visible Start Menu, English/Japanese selector, rules panel, pause panel, EventSystem, Canvas, player, camera, P2 gameplay UI, ResultPanel, shelter entrances, and P2-P10 runtime diagnostics.

The FPS/stutter exporter is explicit opt-in with `-p10cMmEnableFpsExporter`. Auto-quit is disabled by default and only available with the explicit `-p10cMmAutoQuit` profiling argument.

The scripted actual P7 integration verification launches the rebuilt temporary player with `-p10cPreAutoStartGame`. It confirms the Start Game flow reaches the P7 target scene path, creates player/camera/UI/ResultPanel/shelter entrances, loads P8/P9/P10 data from the built player data folder, spawns P9 runtime markers and lightweight crowd agents, validates the P9 final outcome handoff, and triggers the P10 green ground frames after a verification tsunami start.

Important limitation: the P7 source worktree contains the actual 22.5 GB high-detail scene. The P9 target scene currently exists as a small tracked placeholder/status shell. The startup path logs and displays this as a limitation instead of showing a blank screen.
