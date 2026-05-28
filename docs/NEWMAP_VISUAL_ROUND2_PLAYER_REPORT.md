# NewMap Visual Round 2 Player Report

Generated: 2026-05-29T00:00:00+09:00

Status: `completed_on_new_chuo_basemap`

Build path:

`D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapVisualRound2Pre\ChuoTsunamiEvacuation_NewMapVisualRound2Pre.exe`

Validation:

- temp round-2 player build passed
- copied runtime configs: NPC distribution, mouse drag look, lighting profiles
- launch smoke passed with `-newmapSelfAuditSmoke`
- Player.log parse: 0 errors, 0 warnings
- mouse drag-look smoke: passed
- day/night lighting smoke: passed
- ground alignment smoke: support 1.97m, visual ground 1.97m, spawn delta 0.04m
- FPS/stutter sample: 180s, 160 requested/spawned/active NPCs, 3324.68 average FPS in hidden smoke run, 2 frames over 66ms

Remaining manual checks:

- physical mouse drag feel
- visible player/building/ground alignment near spawn and active targets
- night readability from the real camera view
- material/photo-texture acceptability

JSON: `Assets/Data/P10/newmap_visual_round2_player_report.json`
