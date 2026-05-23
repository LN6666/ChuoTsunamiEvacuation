# P7-D Asset Archive And Recovery Plan

Validation date: 2026-05-23.

## What Was Imported

Manual PLATEAU SDK import populated `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` with renderable scene-embedded PLATEAU objects:

- Buildings: 103,190 objects.
- Roads / transport: 14,524 objects.
- Bridges: 13 objects.
- Underground: 1 object.

The actual detected LOD range is LOD0-LOD2. Average LOD3 is not achieved. The imported high-detail scene should be treated as local only until an archive or large-asset strategy is approved.

## Storage State

| Asset/state | Location | Git state |
|---|---|---|
| High-detail imported scene | `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` | Modified locally, 22.55 GB, do not blindly commit |
| Converted P7HighDetail asset roots | `Assets/P7HighDetail/*` | Not present |
| PLATEAU global asset root | `Assets/PLATEAU` | Not present |
| P7Benchmark raw candidate | `Assets/P7Benchmark/Imported/53393690/` | Existing sandbox state |
| Local raw PLATEAU source | `D:\PLATEAU_DATA\Chuo_2025_CityGML` | Local-only raw data |

## What GitHub Should Preserve

- Code, docs, tools, tests, prompts, and review prompts.
- P7-D validation and closeout reports.
- Small reproducible metadata and manifests.
- Existing approved P7Benchmark sandbox metadata.

## What Must Remain Local Or Be Archived Outside Git

- The 22.55 GB imported scene unless a later LFS/release strategy explicitly accepts it.
- Large generated city assets.
- Windows EXE build folders and final packaged game outputs.
- Unity profiler binary captures.
- Raw PLATEAU data under `D:\PLATEAU_DATA`.

## Cloud Drive Requirement

Before the cloud PC is deleted, archive the final playable build and any required local-only map assets to cloud drive. At minimum, preserve:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` if future phases still depend on it.
- Final Windows game package.
- Any build manifest that records Unity version, branch, commit hash, and local-only asset paths.

## Do Not Commit

Do not commit `Library`, `Temp`, `Builds`, `Logs`, cache folders, profiler binaries, or raw PLATEAU data.
