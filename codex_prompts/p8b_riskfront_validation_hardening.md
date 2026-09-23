# P8-B Risk Front Validation, Performance Guard, and Review Hardening Prompt

Continue PBL8 / P8-B using the established PBL4/PBL5/PBL6 workflow.

Role:
Codex B / P8BGuard

Task name:
P8-B Risk Front Validation, Performance Guard, and Review Hardening

Execution style:
Work autonomously.
Create or reuse dedicated worktree.
Create/update docs/tools/tests.
Do not mutate P7_HighDetail scene.
Run preflight/tests/DeepSeek.
Commit/push only if no A-level blockers.
Report final status clearly.

Worktree:
D:\UnityProjects\ChuoTsunamiEvacuation-P8BGuard

Branch:
p8b-riskfront-validation-hardening

Base:
origin/p8-tsunami-hazard-risk-front-foundation

Purpose:
Harden the P8-B dynamic risk front implementation before/alongside visual implementation:
- validation tools,
- test hardening,
- performance guard,
- science-vs-visual safety,
- P8-C handoff readiness.

P8 stage count:
P8 has exactly five stages:
- P8-A
- P8-B
- P8-C
- P8-D
- P8-E

Do not create P8-0, P8-F, P8-G.

Strict scope:
- Do not modify P7_HighDetail_Chuo.
- Do not modify Chuo_BaseMap.
- Do not modify ProjectSettings or Packages.
- Do not implement visual light curtain scene objects.
- Do not implement road/building/bridge/underground interactions.
- Do not implement collapse proxy.
- Do not implement P9/P10 systems.
- Do not change gameplay success/failure rules.

Allowed paths:
- docs/P8B_*.md
- tools/p8/*p8b*.ps1
- Assets/Scripts/P8/
- Assets/Tests/EditMode/P8/
- Assets/Tests/PlayMode/P8/
- Assets/Data/P8/
- codex_prompts/p8b_riskfront_validation_hardening.md
- deepseek_review_prompt_p8b_guard.md

Task 1: Risk-front validation hardening
Create/update:
tools/p8/validate_p8b_riskfront_config.ps1
tools/p8/run_p8b_guard_preflight.ps1

Validate:
- visualHeightMeters large values require visualHeightIsCinematicOnly=true.
- tsunamiHeightMeters/waterLevelMeters are not confused with visualHeightMeters.
- boundaryIsEvidenceBasedOrPrototype is explicit.
- sourceMode does not claim official values.
- geometryType valid.
- collapse proxy remains data-only until P8-D.
- P8-B does not include P8-C/D behavior.

Task 2: Performance guard
Create:
docs/P8B_PERFORMANCE_GUARD.md
tools/p8/inspect_p8b_visual_performance_risk.ps1

It should warn about:
- excessive segment counts,
- expensive per-frame mesh rebuild,
- high transparency overdraw,
- too many light curtain objects,
- unbounded particle usage,
- material/shader risk,
- lack of culling or enable/disable strategy.

Do not add new packages.

Task 3: Test hardening
Add/update tests under:
Assets/Tests/EditMode/P8/
Assets/Tests/PlayMode/P8/

Tests:
- cinematic height guard.
- invalid config fails.
- risk front config remains separate from scientific fields.
- P8-B code does not reference P9 namespaces/classes.
- P8-B config does not enable collapse gameplay.
- performance risk inspector returns warnings for unsafe settings.

Task 4: P8-C handoff readiness
Create:
docs/P8B_TO_P8C_HANDOFF_CHECKLIST.md

It must define what P8-C can rely on:
- hazard layer loaded,
- risk front visual exists or is ready,
- affected infrastructure types are defined,
- road/building/bridge/underground interactions are still not implemented,
- P8-C must not assume official hazard values unless evidence is reviewed.

Task 5: Documentation hardening
Create/update:
docs/P8B_VALIDATION_HARDENING.md
docs/P8B_SAFETY_GUARDRAILS.md
docs/P8B_REVIEW_BACKLOG.md

Must emphasize:
- visual curtain is cinematic only,
- no full fluid simulation,
- no gameplay success/failure changes,
- no P9 systems.

Task 6: Prompt traceability
Save this prompt as:
codex_prompts/p8b_riskfront_validation_hardening.md

Task 7: DeepSeek review prompt
Create:
deepseek_review_prompt_p8b_guard.md

It must check:
- no scene mutation,
- no P8-B visual scene implementation here,
- no protected paths changed,
- no P9/P10 systems,
- validation/test/performance guard exists,
- science vs visual separation enforced,
- tests/preflight pass.

Task 8: Validation
Run:
powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_guard_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui

If Unity creates ProjectSettings/Packages churn, revert it and do not stage.

Task 9: DeepSeek
Run:
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p8b_guard.md

Task 10: Commit/push
If no A-level blockers:
commit:
test(p8-b): harden risk front validation and performance guards

Push:
p8b-riskfront-validation-hardening

Final report:
- worktree/branch,
- changed files,
- preflight result,
- EditMode result,
- PlayMode result,
- DeepSeek verdict,
- commit hash,
- push result,
- final git status,
- integration recommendation.
