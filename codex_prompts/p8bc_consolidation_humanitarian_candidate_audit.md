# P8-B/C Consolidation Humanitarian Candidate Audit Prompt

Continue PBL8 on `p8-tsunami-hazard-risk-front-foundation`.

Task: P8-B/C consolidation gate plus humanitarian high-rise candidate audit before P8-D.

Required outcomes:

- P8 has exactly five stages: P8-A through P8-E only.
- P8-B Problem 1 is consolidated on official Tokyo Metropolitan Government tsunami damage-estimation spatial layer v1.
- P8-C Problem 2 and Problem 3 are consolidated at smoke/proxy level.
- Humanitarian/high-rise candidate names are listed only from actual project files.
- P8-D, P8-E, and P9 lifecycle ownership is documented but not implemented.

Do not modify `Chuo_BaseMap.unity`, ProjectSettings, Packages, `Assets/PLATEAU`, or non-P8 data paths.

Do not implement P8-D collapse proxy, P8-E closeout, P9 gameplay, real indoor scenes, or P10 packaging in this goal.

Required validation:

- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8bc_consolidation_preflight.ps1`
- existing P8-A/P8-B/P8-C preflights
- Unity GUI EditMode tests
- Unity GUI PlayMode tests
- DeepSeek review with `deepseek_review_prompt_p8bc_consolidation.md`
