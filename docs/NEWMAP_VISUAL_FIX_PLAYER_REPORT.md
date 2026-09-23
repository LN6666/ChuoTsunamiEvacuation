# NewMap Visual Fix Player Report

Generated: 2026-05-29T00:00:00+09:00

Status: `completed_on_new_chuo_basemap`

Build path:

`D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapVisualFixPre\ChuoTsunamiEvacuation_NewMapVisualFixPre.exe`

Validation:

- temp player build passed
- NPC config copied into player data: yes
- launch smoke passed with `-newmapSelfAuditSmoke`
- Player.log parse: 0 errors, 0 warnings
- FPS/stutter sample: 180s, 160 requested/spawned/active NPCs, 3598.53 average FPS in hidden smoke run, 2 frames over 66ms
- clear day and night visual sanity are covered by runtime lighting configuration/tests, with manual visual confirmation still required
- physical mouse-look confirmation remains a manual checklist item

JSON: `Assets/Data/P10/newmap_visual_fix_player_report.json`
