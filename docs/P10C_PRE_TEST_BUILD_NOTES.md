# P10-C-Pre Test Build Notes

P10-C-Pre is a pre-release performance gate before official P10-C. It may create a temporary Windows x64 profiling/test build, but this is not the final release build and not the final release package.

## Temporary Build Plan

- Branch: `p10c-pre-performance-gate-optimization`
- Build script: `tools/p10/build_p10c_pre_test_build.ps1`
- Default output: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe`
- Build type: temporary Windows x64 development profiling/test build
- Build scene list: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`
- Build artifacts: outside the repo by default and must not be committed

## Current Status

Validated on 2026-05-25 JST:

- Temporary Windows x64 profiling/test build: succeeded
- Output: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe`
- Build summary: `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\p10c_pre_build_summary.json`
- Build warnings/errors: 4 warnings, 0 errors
- Build data fix: `Assets/Data` is copied into `ChuoTsunamiEvacuation_P10CPre_Data\Data` for this temporary build because existing runtime loaders read `Application.dataPath/Data`.

No release package, archive, final EXE package, P10-E, P10-F, or P10-G is created by this task.

## Guardrails

- `ProjectSettings/`, `Packages/`, `Assets/PLATEAU/`, `Assets/Scenes/Chuo_BaseMap.unity`, and `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` remain protected.
- Temporary build output must stay outside Git or under ignored build paths.
- Any build failure must be documented instead of converted into a success claim.
- Unity-generated ProjectSettings, URP settings, and Addressables helper churn is reverted/removed by the build script after a clean start.
