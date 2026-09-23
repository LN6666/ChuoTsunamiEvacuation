# P10-C-Pre High-Detail Scene Source

Read-only source for this task:

`D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity`

Expected source metadata:

`D:\UnityProjects\ChuoTsunamiEvacuation-P7\Assets\Scenes\P7HighDetail\P7_HighDetail_Chuo.unity.meta`

Target scene path in this worktree:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

Current finding: source exists and is the actual large high-detail scene. The P9 target path exists, but is a small tracked placeholder/status shell. This hotfix does not commit or mutate the 22.5 GB scene. Startup diagnostics report the limitation visibly.

External source path portability:

- EditMode checks use `P10C_PRE_P7_SOURCE_SCENE` and `P10C_PRE_P7_SOURCE_SCENE_META` when those environment variables are set.
- If the external P7 source worktree is unavailable on another machine, the source-worktree availability test is inconclusive instead of failing the whole suite.

Read-only source baseline observed before this hotfix:

- `git status --short -- Assets/Scenes/P7HighDetail`: ` M Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- Source scene bytes: `22554882711`
- Source scene `LastWriteTimeUtc`: `2026-05-23T07:54:28.3463384Z`

The preflight treats that exact status/size/timestamp as a documented pre-existing baseline. Any other P7 source dirty state is a hard failure.
