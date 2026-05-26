# New Map PLATEAU SDK Setup

## Current SDK Configuration

The Unity manifest already points to the local PLATEAU SDK tarball:

```text
Package: com.synesthesias.plateau-unity-sdk
Source: file:D:/UnityPackages/PLATEAU-SDK-for-Unity-v4.2.0.3.tgz
```

The package lock resolves the SDK as a local tarball. This is suitable for a fresh Chuo_BaseMap import preparation because it avoids changing package source or installing a new dependency during reset setup.

## Readiness Check

From the main project directory:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/check_plateau_sdk_readiness.ps1
```

The script checks:

- Unity project folders.
- Unity editor version file.
- PLATEAU SDK entry in `Packages/manifest.json`.
- PLATEAU SDK entry in `Packages/packages-lock.json`.
- Existence of the local SDK tarball.
- Existence of `Assets/Scenes/Chuo_BaseMap.unity`.
- Required baseline scene roots.

## Fresh Import Preparation

Manual Unity preparation before importing:

1. Open `D:\UnityProjects\ChuoTsunamiEvacuation` with Unity `6000.4.6f1`.
2. Let Unity resolve packages.
3. Confirm the PLATEAU SDK package loads without package manager errors.
4. Open `Assets/Scenes/Chuo_BaseMap.unity`.
5. Confirm the baseline root objects exist.

Do not import map data during preflight. The import should be a separate manual milestone after the SDK menu and baseline scene are confirmed in Unity.

## Import Policy For The Next Manual Step

When the project is ready for import, keep the import narrow:

- Use the local Chuo PLATEAU source path documented for this project: `D:\PLATEAU_DATA\Chuo_2025_CityGML`.
- Start with Buildings / LOD1 unless a later plan changes the scope.
- Keep imported geometry under `MapRoot`.
- Keep runtime/gameplay roots separate from imported PLATEAU geometry.
- Do not commit generated scene files or raw PLATEAU data.

This document does not reconnect P2-P10 systems and does not assert that generated PLATEAU geometry is gameplay-ready.
