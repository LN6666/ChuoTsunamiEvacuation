# P7-D Manual PLATEAU Import Checklist

## Status

P7-D is BLOCKED until this checklist is completed in Unity.

## Scene To Open

Open:

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

Do not open or modify `Assets/Scenes/Chuo_BaseMap.unity`.

## Source Data

Use local PLATEAU CityGML source:

`D:\PLATEAU_DATA\Chuo_2025_CityGML`

If the SDK writes generated texture/map side-effect files beside source data, stop and record the changed paths before continuing.

## Target Import Settings

| Category | Target |
|---|---|
| Buildings | Import, target LOD3 |
| Roads | Import, target LOD3 |
| Urban planning decision information | Import, target LOD1 |
| Land use | Import |
| Underground city / underground streets / underground spaces | Import, target LOD3 if available |
| City furniture / urban equipment | Import, target LOD2 or LOD3 depending availability/performance |
| Water body | Import, target LOD1 |
| Vegetation | Import, target LOD3 if available |
| Bridges | Import, target LOD3 |
| Disaster risk | Import |
| Relief / terrain | Import |

## Import Rules

- Do not use `Chuo_BaseMap.unity` as the import destination.
- Do not modify `ProjectSettings` or `Packages`.
- Prefer isolated P7 paths if the SDK asks for asset output:
  - `Assets/P7HighDetail/Imported/`
  - `Assets/P7HighDetail/PLATEAU/`
- Do not commit large generated assets until the Git/LFS/cloud archive decision is explicit.
- Save `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` after import.
- Record any generated asset paths and approximate sizes.

## Post-Import Codex Validation Prompt

After manual import is complete, ask Codex:

Continue PBL7 / P7-D after manual PLATEAU SDK import. Validate `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`, inspect actual loaded categories and LOD coverage, run P2-P6 compatibility checks, run Windows EXE profiling if possible, update P7-D reports, run P7-D preflight, Unity GUI tests, DeepSeek final review, and commit/push only if no A-level blockers.
