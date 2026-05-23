# P8-A Scene Compatibility Gate Prompt

Continue PBL8 / P8-A using the established PBL4/PBL5/PBL6 workflow.

Role:
Codex A / P8Compat

Current worktree:
D:\UnityProjects\ChuoTsunamiEvacuation-P7

Current branch:
p8-tsunami-hazard-risk-front-foundation

Task name:
P8-A Scene Compatibility Gate for P2-P6 on P7_HighDetail_Chuo

Execution style:
Work autonomously.
Inspect branch and git status.
Preserve the local P7_HighDetail_Chuo baseline.
Do not reset, checkout, or overwrite Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity.
Create/update docs/tools/tests.
Run preflight.
Run Unity GUI EditMode and PlayMode tests.
Run DeepSeek review.
Commit/push only if no A-level blockers.
Report final status clearly.

Important context:
P8-A setup is complete and pushed.
P7_HighDetail_Chuo.unity is the user-approved practical baseline for P8/P9/P10.
Chuo_BaseMap.unity is legacy fallback.
P8-A hazard data layer exists, but full P2-P6 compatibility on the new map still needs a stronger smoke gate before P8-B scene-anchor work.

P8 stage count:
P8 has exactly four stages:
- P8-A
- P8-B
- P8-C
- P8-D

Do not create P8-0, P8-E, P8-F, P8-G.

Strict prohibitions:
- Do not modify Chuo_BaseMap.unity.
- Do not modify ProjectSettings or Packages.
- Do not reset or overwrite P7_HighDetail_Chuo.unity.
- Do not change gameplay success/failure rules.
- Do not implement P8-B light curtain yet.
- Do not implement P8-C hazard interactions yet.
- Do not implement P8-D collapse proxy yet.
- Do not implement P9 crowd/spawn/indoor gameplay.
- Do not implement P10 packaging.

Allowed paths:
- docs/P8A_*.md
- docs/P8_*.md
- tools/p8/
- Assets/Scripts/P8/
- Assets/Tests/EditMode/P8/
- Assets/Tests/PlayMode/P8/
- codex_prompts/p8a_scene_compatibility_gate.md
- deepseek_review_prompt_p8a_compat.md

Required tasks:

1. Inspect baseline scene state, including branch/status, dirty status of `P7_HighDetail_Chuo.unity`, scene existence, local-only/dirty state, and whether `Chuo_BaseMap` is untouched. Create/update `docs/P8A_SCENE_COMPATIBILITY_GATE.md` and `docs/P8A_P7_HIGHDETAIL_BASELINE_STATUS.md`.
2. Create/update `docs/P8A_P2_P6_COMPATIBILITY_SMOKE_REPORT.md` with P2-P6 compatibility evidence and explicit pending checks where full runtime validation is not possible.
3. Create `docs/P8A_P8B_SCENE_ANCHOR_PLAN.md` defining P8-B anchoring to `P7_HighDetail_Chuo`, hazard JSON usage from `Assets/Data/P8`, and P9/P10 avoidance.
4. Create/update `tools/p8/inspect_p8a_scene_compatibility.ps1` and `tools/p8/run_p8a_compat_preflight.ps1`.
5. Add/update EditMode and PlayMode tests under P8 test folders for compatibility reporting, hazard-loader neutrality, high-detail baseline reference, and no P8-B/P8-C/P8-D behavior.
6. Save this prompt as `codex_prompts/p8a_scene_compatibility_gate.md`.
7. Create `deepseek_review_prompt_p8a_compat.md`.
8. Run P8-A compatibility preflight, existing P8-A preflight, Unity GUI EditMode tests, and Unity GUI PlayMode tests.
9. Run DeepSeek review using `deepseek_review_prompt_p8a_compat.md`.
10. Commit and push only if validation passes and there are no A-level blockers.
