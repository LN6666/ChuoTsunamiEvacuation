# P7-B Wave 2-B Visual Feasibility Plan

Date: 2026-05-23

Status: deferred until separate human approval.

## Purpose

This plan records what a future visual feasibility check would need to prove after Wave 2-B. It is not approval to perform that check now.

## Preconditions

Before any real import or visual verification:

- the human developer must approve a new Markdown plan,
- the source subset must be narrowed from metadata evidence,
- the target Unity paths must be explicitly allowed,
- rollback criteria must be written before import,
- Unity tests must be planned if Unity files change,
- `Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, `Assets/Data`, and `Assets/PLATEAU` must remain protected unless separately approved.

## Candidate Order

1. `53393690`: preferred planning candidate because it has 70 LOD3 path/name hits, but it is a 605.38 MB broad same-mesh-code footprint and must be narrowed before import.
2. `53393672`: compact fallback at 61.22 MB, but has no current LOD3 path/name evidence.
3. `53394611`: underground-building fallback, but the broad footprint is 1.25 GB and has no current LOD3 path/name evidence.

## Future Visual Checks

A later approved import experiment should verify:

- whether imported geometry actually contains useful LOD3 detail,
- whether textures and materials resolve correctly,
- whether bridge, road, building, and optional underground context are visually coherent,
- object count, material count, texture count, triangle count, draw calls, memory, FPS, and build-size impact,
- whether the candidate works in an isolated P7Benchmark context before any production scene discussion.

## Explicit Non-Goals

Wave 2-B does not implement:

- P8 reserved hazard, flooding, or warning-visualization systems,
- P9 reserved crowd, spawning, congestion, or interior-shelter systems,
- gameplay success/failure rule changes,
- full Chuo import,
- production scene integration.

## Stop Criteria For A Future Import

A future import experiment should stop and roll back if:

- generated files land outside approved paths,
- `Chuo_BaseMap.unity` changes,
- `ProjectSettings` or `Packages` change unexpectedly,
- `Assets/Data` or `Assets/PLATEAU` changes,
- import size exceeds the approved plan,
- Unity becomes unstable or tests fail with protected-path churn.
