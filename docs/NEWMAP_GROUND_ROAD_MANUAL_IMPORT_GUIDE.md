# NewMap Ground/Road Manual Import Guide

## Manual Steps

1. Open `D:\UnityProjects\ChuoTsunamiEvacuation` in Unity.
2. Open `Assets/Scenes/Chuo_GroundRoad_Import_Source.unity`.
3. Use the PLATEAU SDK to import only ground/road-related data.
4. Prefer importing:
   - road / transportation / `tran`
   - terrain / relief / `dem`
   - bridge if needed
   - water if needed
5. Do not import buildings / `bldg` again.
6. Do not import city furniture, vegetation, underground, or other heavy unrelated layers unless explicitly needed.
7. Save `Assets/Scenes/Chuo_GroundRoad_Import_Source.unity` after import.
8. Then run Codex merge/sampling tools.

## Import Policy

Keep the manual PLATEAU import narrow. The source scene is only for supplemental ground, road, terrain, relief, bridge, or water data needed to improve NewMap ground and road sampling.

Do not save imported objects into `Assets/Scenes/Chuo_BaseMap.unity`. Do not overwrite existing map assets. Do not treat this guide as confirmation that ground/road import is complete.

## Before Merge Or Sampling

Run the source-scene preflight before manual import to confirm the staging scene starts clean:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_ground_road_import_source_preflight.ps1
```

After the manual import is saved, ask Codex to inspect the imported hierarchy and run the next merge or sampling tools.
