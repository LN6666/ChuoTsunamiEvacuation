# P10-C-Pre Build Startup Flow

Default build scene:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

Runtime flow:

1. Player loads the first build scene.
2. `P10CPrePlayableStartupBootstrap` installs in player builds.
3. Start Menu appears immediately.
4. Language and Rules UI are available before gameplay.
5. Start Game loads or confirms the P7 high-detail scene.
6. Runtime bootstrap creates missing player/camera/UI/gameplay references.
7. P2-P10 diagnostics run against the active scene.
8. Missing or placeholder high-detail evidence is shown as a visible diagnostic.

Profiling mode:

`P10CMMFpsStutterExporter` only installs when `-p10cMmEnableFpsExporter` is passed.
