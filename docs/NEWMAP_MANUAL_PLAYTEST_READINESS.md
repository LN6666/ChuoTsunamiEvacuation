# NewMap Manual Playtest Readiness

JSON: `Assets/Data/P10/newmap_manual_playtest_readiness.json`

Decision: `ready_with_documented_limitations`

The new `Chuo_BaseMap` is the active playable baseline for manual testing. P2, P5, P6, P8, P9, and P10 are connected through documented runtime proxies where real map anchors are unavailable. P3/P4 old sample shelter records remain disabled because they cannot be verified on the reset map. No official route or official shelter activation is claimed.

Latest validation:

- Final preflight: PASS
- EditMode: 108 passed, 0 failed
- PlayMode: 19 passed, 0 failed
- Hardening temporary player build target: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapHardeningPre\ChuoTsunamiEvacuation_NewMapHardeningPre.exe`
- Player.log: 0 errors, 0 warnings
- Performance gate: `ready_with_documented_limitations`
- DeepSeek hardening review: no A-level issues; all hardening requirements met.

Hardening update:

- Active runtime targets increased to 4 local non-official training targets.
- `newmap_proxy_crowd_delay` was added for P6/P9 congestion-delay validation.
- Hardening Player.log: 0 errors, 0 warnings.
- Hardening performance sample: average FPS 46.2, max frame 20,655.05 ms, max private memory 21,950,038,016 bytes.

Readiness remains `ready_with_documented_limitations` because memory and startup/frame spike remain B-level limitations. Ordinary-PC readiness and no-spike startup readiness are not claimed.
