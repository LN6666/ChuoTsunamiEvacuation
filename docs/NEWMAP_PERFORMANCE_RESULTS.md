# NewMap Performance Results

Scene size: `12,157,291,044` bytes, about 11.32 GiB.

Streaming scan:

- MeshRenderers: 44,115.
- MeshColliders: 44,115.

Unity editor scene-load audit after root setup:

- Loaded Renderers: 18,596.
- Loaded Colliders: 18,596.
- Unity log memory observation after scene load: about 6.11 GB.

Current classification: `ready_with_limitations`.

Reason:

- Temporary player build started and bootstrapped `Chuo_BaseMap`.
- 40-second post-rebuild process sample completed against `ChuoTsunamiEvacuation_NewMapPre`.
- Max private memory: `21,928,771,584` bytes, above the 12 GB ordinary-PC risk gate.
- Max working set: `7,504,310,272` bytes.
- Runtime FPS probe reported 3,718 frames over 30.01 seconds: average `123.91` FPS.
- Max frame time was `6363.29` ms, with 2 frames over 66 ms; a manual visual stutter check is still required because the automated run used a minimized player window.
- Player log parse reported 0 errors and 0 warnings for the sampled run.

Temporary build:

`D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapPre\ChuoTsunamiEvacuation_NewMapPre.exe`

Performance result JSON:

`Assets/Data/P10/newmap_performance_results.json`

Player log summary JSON:

`Assets/Data/P10/newmap_player_log_summary.json`

No fake performance pass is claimed.
