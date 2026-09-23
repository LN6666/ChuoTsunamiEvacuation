# New Map Project State Audit

Audit date: 2026-05-26

## Workspace

Main project directory:

```text
D:\UnityProjects\ChuoTsunamiEvacuation
```

This audit uses the main ChuoTsunamiEvacuation worktree only. P9, P10RealMap, P7, and other phase worktrees are not treated as the active worktree for the Chuo_BaseMap reset.

## Unity Project Structure

Required Unity project folders are present:

| Path | Status |
|---|---|
| `Assets/` | Present |
| `Packages/` | Present |
| `ProjectSettings/` | Present |

Unity project validity for reset preparation: valid.

## Unity Version

`ProjectSettings/ProjectVersion.txt` reports:

```text
6000.4.6f1
```

## Package State

`Packages/manifest.json` includes the PLATEAU SDK package:

```text
com.synesthesias.plateau-unity-sdk
```

Current package source:

```text
file:D:/UnityPackages/PLATEAU-SDK-for-Unity-v4.2.0.3.tgz
```

`Packages/packages-lock.json` resolves the package as a local tarball. The local tarball path exists on this machine at audit time.

## Chuo_BaseMap Scene State

Before reset preparation, `Assets/Scenes/Chuo_BaseMap.unity` was missing while `Assets/Scenes/Chuo_BaseMap.unity.meta` existed.

A minimal local baseline scene has been created at:

```text
Assets/Scenes/Chuo_BaseMap.unity
```

The baseline contains only root GameObjects for organization. No PLATEAU map data has been imported. No gameplay systems, generated meshes, crowd simulation, or tsunami fluid simulation were added.

The scene and its `.meta` are ignored by Git under the existing generated PLATEAU scene policy.

## Reset Boundaries

Confirmed boundaries for this reset:

- Do not import PLATEAU map data yet.
- Do not delete existing scripts, data, or docs.
- Do not run `git reset --hard`.
- Do not run `git clean -fd`.
- Do not claim P2-P10 are reconnected.
- Keep PLATEAU raw data outside Git.

## Validation Tool

Run the preflight from the project root:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_preflight.ps1
```
