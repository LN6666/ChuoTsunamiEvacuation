# Codex Prompt Trace: P9-A Crowd / Spawn / Entrance-Safe-Floor Proxy Foundation

Start PBL9 based on current P8 progress, while P8-C/D/E continues in parallel.

Project:
ChuoTsunamiEvacuation

Current phase:
PBL9 / P9-A

Task name:
Crowd / Spawn / Entrance-Safe-Floor Proxy Foundation

Execution style:
Use the established PBL4/PBL5/PBL6/PBL7/PBL8 workflow:
- Work autonomously inside Codex.
- Create or reuse a dedicated P9 worktree.
- Create/update docs, scripts, data schemas, Unity scripts, tests, tools.
- Run P9 preflight.
- Run Unity GUI EditMode and PlayMode tests.
- Run DeepSeek review.
- Commit/push only if there are no A-level blockers.
- Report final status clearly.

Worktree:
Create or reuse:
D:\UnityProjects\ChuoTsunamiEvacuation-P9

Branch:
p9-crowd-spawn-evacuation-proxy-foundation

Base:
origin/p8-tsunami-hazard-risk-front-foundation

Important:
P8 is still in progress.
P8-C supplemental work and P8-D/E will continue separately.
P9-A must not depend on unfinished P8-D/E damage/collapse outputs.
P9-A must build the P9 foundation only.

P9 positioning:
Crowd Interaction + Real/Rule-based Spawn Points + Entrance/Safe-floor / Vertical Evacuation Proxy + Evacuation Failure Mechanism.

Updated P9 scope:
Real indoor shelter scenes are cancelled because no LOD4/BIM interior scene is available.
Do not build indoor stair/fire-route/interior-template gameplay.
Use external/abstracted proxy flow:
- shelter entrance marker,
- safe-floor / vertical evacuation status,
- evacuation complete proxy,
- entrance queue / congestion state,
- future failure mechanism.

P9-A scope:
Foundation/scaffold only:
- stage plan and boundaries,
- spawn point schema,
- crowd/NPC agent schema,
- entrance/safe-floor proxy schema,
- congestion state model,
- metrics/logging model,
- P8 handoff contract,
- tests/preflight/review.

Do not implement final failure gameplay yet.
Do not make NPCs cause player failure yet.
Do not implement final P8 damage/collapse integration yet.
Do not modify P7_HighDetail_Chuo scene yet.
Do not modify Chuo_BaseMap.
Do not implement P10 release packaging.

P9 stage plan:
P9 should use a compact staged plan. Create exactly:
- P9-A: Crowd / Spawn / Entrance-Safe-Floor Proxy Foundation
- P9-B: New-map Scene Integration + Spawn/Crowd Runtime Prototype
- P9-C: Evacuation Failure / Congestion / Vertical Evacuation Proxy Gameplay
- P9-D: P9 Final Integration + Handoff to P10

Do not create P9-E/F/G unless the user later approves.

Strict prohibitions:
- Do not modify Chuo_BaseMap.unity.
- Do not modify ProjectSettings or Packages.
- Do not modify Assets/PLATEAU.
- Do not modify Assets/Data outside P9-specific data paths.
- Do not modify P7_HighDetail_Chuo scene in P9-A.
- Do not implement real indoor scene gameplay.
- Do not implement real fluid/hazard systems from P8.
- Do not directly change P2-P6 gameplay success/failure rules in P9-A.
- Do not make NPC/crowd cause player failure in P9-A.
- Do not use P8-D/E damage/collapse state as if finalized.
- Do not add dependencies.
- Do not download live data.

Allowed paths:
- docs/P9_*.md
- docs/P9A_*.md
- tools/p9/
- Assets/Data/P9/
- Assets/Scripts/P9/
- Assets/Tests/EditMode/P9/
- Assets/Tests/PlayMode/P9/
- codex_prompts/p9a_crowd_spawn_evacuation_proxy_foundation.md
- deepseek_review_prompt_p9a.md

Tasks:

1. Worktree and branch setup.
2. P9 stage plan and boundaries.
3. P8 handoff contract.
4. Spawn point schema.
5. Crowd/NPC agent schema.
6. Entrance / safe-floor / vertical evacuation proxy schema.
7. Runtime foundation scripts.
8. Metrics/logging model.
9. Tests.
10. P9 preflight.
11. Documentation.
12. Prompt traceability.
13. DeepSeek review prompt.
14. Validation.
15. DeepSeek review.
16. Commit/push if preflight, Unity tests, DeepSeek, and protected path checks pass.
17. Final report with worktree, branch, files, validation, DeepSeek verdict, blockers, follow-ups, commit/push, final status, parallel work, and P8-D/E wait points.
