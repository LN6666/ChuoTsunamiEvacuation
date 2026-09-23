# P8-B Dynamic Risk Front Prompt

Continue PBL8 / P8-B using the established PBL4/PBL5/PBL6/PBL7 workflow.

Role: Codex A / P8RiskFront.

Current worktree: `D:\UnityProjects\ChuoTsunamiEvacuation-P7`.

Current branch: `p8-tsunami-hazard-risk-front-foundation`.

Task name: One-shot Dynamic Tsunami Risk Front / Cinematic Light Curtain Visualization.

Implement a dynamic tsunami risk front / cinematic light curtain visualization based on the P8-A hazard data layer.

Critical constraints:

- Preserve `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Do not reset, checkout, overwrite, delete, clean, or stage the high-detail scene unless explicitly instructed.
- Do not modify `Chuo_BaseMap.unity`.
- Do not modify ProjectSettings, Packages, PLATEAU assets, or data outside `Assets/Data/P8`.
- P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E.
- Do not create P8-0, P8-F, or P8-G.
- Do not implement P8-C, P8-D, P9, or P10 systems.
- Do not change P2-P6 gameplay success/failure rules.
- Do not make official tsunami route/hazard claims.
- Do not perform live web requests or live routing.
- Do not implement full real-time fluid simulation.

Required validation:

- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_riskfront_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_compat_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p8b_riskfront.md`

Commit only if all validation passes and DeepSeek reports no A-level blockers.
