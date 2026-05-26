# DeepSeek Review Prompt: NewMap Final Stabilization

Review the current git diff for D:\UnityProjects\ChuoTsunamiEvacuation.

Verify:

- Assets/Scenes/Chuo_BaseMap.unity is the active baseline for the NewMap runtime and final temp build.
- No deprecated old-map scene is used as an active gameplay target.
- Final status wording uses only the strict allowed classifications.
- Disabled/out-of-map targets are truly disabled and cannot spawn markers, routes, green frames, selection, or ResultPanel success.
- P2-P10 are completed, proxy-completed, disabled, blocked, or failed with evidence.
- Tourism Mode and Evacuation Mode work.
- Player, camera, UI, ResultPanel, active target flow, two-stage tsunami, Stage 2 green frames, NPC/crowd, entrance/safe-floor, collapse/debris, weather/stamina, localization/rules/pause are integrated.
- Performance measurement is meaningful and memory/spike risk is honestly documented.
- No final release/archive and no P10-E/F/G work was created.
- No official route false claim and no non-official safety false claim exist.
- Identify any A-level blocker that prevents manual playtest.
