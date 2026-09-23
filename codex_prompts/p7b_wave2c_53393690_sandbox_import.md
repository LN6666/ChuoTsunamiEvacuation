# Codex Prompt: P7-B Wave 2-C 53393690 Sandbox Import

Working tree:

`D:\UnityProjects\ChuoTsunamiEvacuation-P7BWave2C`

Branch:

`p7b-wave2c-53393690-sandbox-import`

Base:

`origin/p7-high-detail-city-foundation`

## Objective

Import the full approved candidate package `53393690` into the isolated P7Benchmark sandbox path:

`Assets/P7Benchmark/Imported/53393690/`

This approval applies only to candidate `53393690` in the P7Benchmark sandbox. It does not approve full Chuo import, production scene integration, `Assets/PLATEAU` changes, `Chuo_BaseMap.unity` changes, `ProjectSettings` or `Packages` changes, fallback candidate import, or P8/P9 systems.

## Fixed P7 Scope

P7 has exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

## Required Validation

Run:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2c_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p7b_wave2c.md
```

Commit and push only if Wave 2-C preflight, EditMode tests, PlayMode tests, protected-path checks, and DeepSeek review all pass with no A-level blockers.
