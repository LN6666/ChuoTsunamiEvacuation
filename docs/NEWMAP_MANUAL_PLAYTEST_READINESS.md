# NewMap Manual Playtest Readiness

JSON: `Assets/Data/P10/newmap_manual_playtest_readiness.json`

Decision: `ready_with_documented_limitations`

The new `Chuo_BaseMap` is the active playable baseline for manual testing. P2, P5, P6, P8, P9, and P10 are connected through documented runtime proxies where real map anchors are unavailable. P3/P4 old sample shelter records remain disabled because they cannot be verified on the reset map. No official route or official shelter activation is claimed.

Latest validation:

- Final preflight: PASS
- EditMode: 108 passed, 0 failed
- PlayMode: 18 passed, 0 failed
- Final temporary player build: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapFinalPre\ChuoTsunamiEvacuation_NewMapFinalPre.exe`
- Player.log: 0 errors, 0 warnings
- Performance gate: `ready_with_memory_limitations`
- DeepSeek: no A-level blocker preventing manual playtest
