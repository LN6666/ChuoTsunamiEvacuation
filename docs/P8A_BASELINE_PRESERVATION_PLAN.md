# P8-A Baseline Preservation Plan

Date: 2026-05-23.

## Preservation Policy

The accepted P7 baseline scene is:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

P8-A preserves this scene as local working state. It is not staged or committed by default because the imported map content is large and may require a later archive, release package, or LFS decision.

## Prohibited Actions

- Do not run `git reset --hard`.
- Do not run `git clean -fd`.
- Do not checkout or restore `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Do not delete imported P7 high-detail map assets.
- Do not overwrite the scene from P8 tools.

## Branch Strategy

P8-A uses branch `p8-tsunami-hazard-risk-front-foundation` created from the completed P7 closeout commit while keeping local scene changes in place.

Future dual-Codex worktrees must not start until this P8-A preservation gate is committed and pushed.

## Dedicated Worktree Validation

Dedicated P8 worktrees may contain only the tracked lightweight scene shell while the accepted 22 GB local imported scene remains in the P7 worktree.

P8-A preflight tools verify the large local baseline without copying or modifying it. The lookup order is:

1. `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` in the current worktree.
2. `P8A_BASELINE_SCENE_PATH`, if set.
3. `D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity`.

If a dedicated worktree cannot see the sibling P7 worktree, set `P8A_BASELINE_SCENE_PATH` to the accepted local scene path before running preflight.

## Archive Before VM Deletion

Before the cloud PC is deleted, archive the accepted high-detail scene, related imported local assets, and final packaged game output to a cloud drive or release package. GitHub should preserve code, docs, tests, schemas, configs, scripts, manifests, and reproducible project state, not blind copies of cache, build, profiler, or temp output.
