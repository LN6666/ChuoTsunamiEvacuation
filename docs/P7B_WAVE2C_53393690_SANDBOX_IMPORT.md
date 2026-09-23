# P7-B Wave 2-C 53393690 Sandbox Import

Date: 2026-05-23

Stage: P7-B Wave 2-C

## Scope

Candidate `53393690` was imported only into:

`Assets/P7Benchmark/Imported/53393690/`

This is a P7Benchmark sandbox import. It is not full Chuo import and not production scene integration.

The import does not approve or perform:

- `Chuo_BaseMap.unity` modification,
- `Assets/PLATEAU` modification,
- `Assets/Data` modification,
- `ProjectSettings` modification,
- `Packages` modification,
- fallback candidate import,
- existing gameplay script changes,
- production scene changes,
- P8/P9 feature work.

## Human Approval Boundary

The human approval for Wave 2-C applies only to the full candidate package `53393690` under the isolated P7Benchmark sandbox path.

Fallback candidates `53393672` and `53394611` are referenced only in documentation.

Any later production integration, scene wiring, conversion pipeline, or broader PLATEAU import requires separate explicit approval.

## Imported Package

Source root:

`D:\PLATEAU_DATA\Chuo_2025_CityGML`

Target root:

`Assets/P7Benchmark/Imported/53393690/`

Imported file count:

`5,843`

Imported bytes:

`634,782,243`

Imported size:

`605.38 MB`

LOD path/name evidence:

- LOD3 path/name hits: `70`
- LOD4 path/name hits: `0`

## Git Storage Note

The imported sandbox path is tracked with a narrow Git LFS rule:

`Assets/P7Benchmark/Imported/53393690/**`

This rule is required because the approved candidate package contains individual GML files larger than standard GitHub blob limits. The rule does not apply to `Assets/PLATEAU`, production scenes, `ProjectSettings`, or `Packages`.

## Visual Status

Visual and geometry quality remain unverified. The imported package contains CityGML and texture source files; Wave 2-C does not convert CityGML into renderable Unity meshes.
