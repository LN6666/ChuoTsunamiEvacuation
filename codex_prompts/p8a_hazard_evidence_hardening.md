# P8-A Hazard Evidence Hardening Prompt

Role:
Codex B / P8Evidence

Task name:
P8-A Evidence Source Plan + Hazard Data Semantics Hardening

Execution style:
Work autonomously.
Create or reuse a dedicated worktree.
Create/update docs/data/tools/tests.
Run P8-A preflight.
Run Unity GUI EditMode and PlayMode tests if Unity files change.
Run DeepSeek review.
Commit/push only if no A-level blockers.
Report final status clearly.

Worktree:
Create or reuse:
D:\UnityProjects\ChuoTsunamiEvacuation-P8Hazard

Branch:
p8a-hazard-evidence-hardening

Base:
origin/p8-tsunami-hazard-risk-front-foundation

P8 stage count:
P8 has exactly four stages:
- P8-A
- P8-B
- P8-C
- P8-D

Do not create P8-0, P8-E, P8-F, P8-G.

Strict scope:
This task is evidence/schema/docs/tools hardening only.
Do not modify Chuo_BaseMap.
Do not modify P7_HighDetail_Chuo.
Do not modify ProjectSettings or Packages.
Do not implement visual light curtain.
Do not implement road/building/bridge/underground interactions.
Do not implement collapse proxy gameplay.
Do not implement P9/P10 systems.
Do not fetch/download/scrape live official data.

Allowed paths:
- docs/P8A_*.md
- docs/P8_*.md
- Assets/Data/P8/
- Assets/Scripts/P8/
- Assets/Tests/EditMode/P8/
- tools/p8/
- codex_prompts/p8a_hazard_evidence_hardening.md
- deepseek_review_prompt_p8a_evidence.md

Task summary:

- Create P8-A evidence source registry and official source review protocol.
- Define hazard variable semantics for arrival time, depth, water level, tsunami height, hazard intensity, boundaries, confidence, evidence source, geometry type, source mode, and prototype/evidence boundary status.
- Clarify science versus cinematic visualization rules.
- Harden JSON schema, sample data, visualization config, infrastructure interaction config, validation tooling, and EditMode tests.
- Save a DeepSeek review prompt that checks boundaries, stage count, semantics, validation, and absence of official-value claims.
- Run P8-A preflight, Unity GUI EditMode tests, Unity GUI PlayMode tests, and DeepSeek review.
- Commit and push only if validation passes and DeepSeek has no A-level blockers.
