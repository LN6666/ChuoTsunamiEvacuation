# P7-B Wave 2-A Benchmark Scene Skeleton

Date: 2026-05-22

Stage: P7-B Wave 2-A

Status: Implemented pending Unity validation.

## Purpose

Wave 2-A creates the smallest possible Unity-side benchmark scene skeleton for later small-area high-detail LOD3 benchmark work. It is a harness foundation only. It does not import real PLATEAU assets and does not validate any real Chuo geometry.

## Scene Path

```text
Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity
```

The scene is created by:

```text
Assets/Editor/P7Benchmark/P7BenchmarkSceneBuilder.cs
```

Command-line creation is available through:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/create_p7b_benchmark_scene.ps1
```

## Scene Contents

The generated scene contains only:

- `P7BenchmarkRoot`
- `P7BenchmarkMarker`
- `P7BenchmarkMetricsRecorder`
- primitive placeholder ground, road, bridge, underground-category, and LOD3-candidate volumes
- one camera
- one directional light

The placeholders are simple Unity primitives. They are not imported PLATEAU geometry and are not evidence of LOD3 or LOD4 visual quality.

## Isolation Rules

Wave 2-A does not modify:

- `Assets/Scenes/Chuo_BaseMap.unity`
- existing Unity scenes
- existing gameplay scripts
- `Assets/PLATEAU`
- `Assets/Data`
- `ProjectSettings`
- `Packages`

The scene has no `EvacuationGameManager`, shelter, tsunami, NPC, route, hazard, or result-system dependency. It does not change gameplay success or failure rules.

## Deferred Work

LOD3 candidate import is deferred to Wave 2-B or a later approved task. Wave 2-B still requires explicit approval before any real asset import, candidate data copy, generated PLATEAU asset, or broader scene work.

LOD4 is not assumed available. Existing P7-A/P7-B inventory findings found no LOD4 path/name hits.

Windows EXE profiling is not completed by Wave 2-A. EXE profiling remains later P7 work, with P7-D retaining final closeout responsibility.

