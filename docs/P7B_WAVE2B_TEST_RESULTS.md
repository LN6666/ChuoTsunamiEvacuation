# P7-B Wave 2-B Test Results

Date: 2026-05-23

## Validation Commands

Required preflight:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2b_preflight.ps1
```

DeepSeek review:

```powershell
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p7b_wave2b.md
```

## Current Results

Latest local validation: PASS.

Preflight command result:

```text
P7-B Wave 2-B preflight: PASS
```

Dry-run inspection summary from preflight:

| Candidate | Files | Size | LOD3 path/name hits | LOD4 path/name hits | Estimate |
|---|---:|---:|---:|---:|---|
| `53393690` | 5,843 | 605.38 MB | 70 | 0 | bounded-heavy |
| `53393672` | 658 | 61.22 MB | 0 | 0 | bounded-small |
| `53394611` | 4,104 | 1.25 GB | 0 | 0 | too-heavy-wholesale |

Protected path result:

```text
Protected path check passed: Chuo_BaseMap, ProjectSettings, Packages, Assets/Data, Assets/PLATEAU, Unity scripts/tests/editor files, and scenes are untouched.
```

The P7 scope guard passed. It emitted only expected-context notices for existing boundary/prompt language.

## Unity Tests

Unity tests were skipped because this Wave 2-B package changes only docs, tools, and prompts.

Expected Wave 2-B scope is docs/tools/prompts only:

- no Unity scene changes,
- no Unity script changes,
- no Unity test changes,
- no asset import,
- no `Chuo_BaseMap.unity` change,
- no `ProjectSettings` or `Packages` change.

If any Unity file changes unexpectedly, stop before running tests and resolve the scope violation.
