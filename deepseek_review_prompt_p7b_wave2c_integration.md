# DeepSeek Review Prompt: P7-B Wave 2-C Integration

Review the integration of PBL7 / P7-B Wave 2-C into the main P7 branch.

Main branch:

`p7-high-detail-city-foundation`

Integrated branch:

`origin/p7b-wave2c-53393690-sandbox-import`

Integrated commit:

`6ca0fc6 feat(p7-b): import 53393690 candidate into benchmark sandbox`

## Review Boundary

This is an integration review, not a new import or production integration.

Wave 2-C imported only candidate `53393690` into:

`Assets/P7Benchmark/Imported/53393690/`

The integration must not approve or imply:

- full Chuo import,
- `Assets/PLATEAU` changes,
- `Assets/Data` changes,
- `Chuo_BaseMap.unity` changes,
- production scene integration,
- existing gameplay script changes,
- `ProjectSettings` changes,
- `Packages` changes,
- P8 tsunami hazard, inundation, light curtain, flood, or risk-front systems,
- P9 crowd, spawn, indoor evacuation, congestion, or failure systems,
- converting or visually importing CityGML into renderable mesh form.

P7 must remain exactly five stages:

- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

No P7-E, P7-F, or P7-G stage should be introduced.

## Integration Evidence

Merge result:

- `git merge --ff-only origin/p7b-wave2c-53393690-sandbox-import`
- result: fast-forward from `5213252` to `6ca0fc6`
- conflicts: none

Imported candidate summary:

- candidate: `53393690`
- target: `Assets/P7Benchmark/Imported/53393690/`
- imported source file count: `5,843`
- imported bytes: `634,782,243`
- imported size: `605.38 MB`
- all imported candidate files are confined to the P7Benchmark sandbox target, with only necessary Unity parent folder `.meta` files outside the candidate subfolder.

Validation results after integration:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7b_wave2c_preflight.ps1
# PASS
# Imported candidate check passed: 5843 files, 634782243 bytes.
# Git LFS tracking check passed.

powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
# PASS: total=146 passed=146 failed=0 skipped=0 inconclusive=0

powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
# PASS: total=29 passed=29 failed=0 skipped=0 inconclusive=0
```

Unity generated temporary `ProjectSettings/ProjectSettings.asset` churn during PlayMode testing. That churn was reverted before review. `Packages` did not churn.

Protected path checks after integration:

- `Chuo_BaseMap.unity`: clean
- `ProjectSettings`: clean after reverting Unity test churn
- `Packages`: clean
- `Assets/PLATEAU`: clean
- `Assets/Data`: clean
- production scenes outside `Assets/Scenes/P7Benchmark/`: clean
- existing gameplay scripts outside `Assets/Scripts/P7Benchmark/`: clean
- imported candidate content is confined to `Assets/P7Benchmark/Imported/53393690/` plus required Unity parent folder metadata.

Git LFS state before pushing main:

- main branch is ahead of `origin/p7-high-detail-city-foundation` by the Wave 2-C commit.
- `git lfs status` reports LFS objects to be pushed to `origin/p7-high-detail-city-foundation`, which is expected before pushing the integrated main branch.
- there are no unstaged or uncommitted LFS changes.

## Required Checks

Confirm:

- the fast-forward merge is safe,
- only candidate `53393690` is integrated,
- imported files are only in the approved P7Benchmark sandbox target,
- no fallback candidate package was imported,
- no `Assets/PLATEAU` changes exist,
- no `Assets/Data` changes exist,
- no `Chuo_BaseMap.unity` changes exist,
- no `ProjectSettings` or `Packages` changes remain,
- no production scene integration occurred,
- no existing gameplay scripts changed,
- no full Chuo import occurred,
- P7 remains exactly five stages,
- no P7-E, P7-F, or P7-G stage was created,
- no P8/P9 systems were implemented,
- Unity EditMode and PlayMode GUI tests passed,
- visual quality claims remain conservative,
- imported CityGML was not converted to renderable Unity mesh form in this integration,
- LOD3 visual and geometry quality remain unverified,
- LOD4 is not assumed available,
- production integration remains future gated work.

## Output Format

Classify findings:

- A-level: must fix before push.
- B-level: follow-up allowed after push if no safety issue.
- C-level: note or polish only.

Return:

- overall verdict,
- whether push to `p7-high-detail-city-foundation` is safe,
- A-level blockers, if any,
- B-level follow-ups, if any.
