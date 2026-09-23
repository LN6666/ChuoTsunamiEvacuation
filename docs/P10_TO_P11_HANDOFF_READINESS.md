# P10 To P11 Handoff Readiness

Status: ready for P10 to P11 handoff after user confirmation.

Final P10 tuning target:

- Active scene: `Assets/Scenes/Chuo_BaseMap.unity`
- Final temp EXE: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapFinalP10TuningPre\ChuoTsunamiEvacuation_NewMapFinalP10TuningPre.exe`
- Evacuation max stamina: `3500`
- Evacuation sprint speed reduction: `20%` from previous P10 build
- New Evacuation sprint speed: `4.59 m/s`
- Tourism Mode stamina: disabled
- Tourism sprint speed: unchanged

Validation completed:

- Final P10 preflight: passed
- EditMode: passed, `127/127`
- PlayMode: passed, `43/43`
- Temporary player build: passed
- Player.log parse: passed clean
- DeepSeek: `PASS_no_A_level_blockers_deepseek_review_20260531_023810`

Known limitations to carry into P11:

- Remaining building-floating outliers require manual visual review.
- Routes remain estimated prototype guidance, not official evacuation routes.
- Non-official candidates remain non-official and warning-only.
- Performance uses documented far-NPC proxy behavior for doubled NPC count.

P11 reminder:

P11 should handle final release/package/archive and final RC review. This P10 task did not create those artifacts or move the project into P11.
