# P7-B Small-Area Feasibility

Date: 2026-05-22

Stage: P7-B

Scope: Documentation and command-line feasibility only. No Unity scene, Unity script, `Assets/Data`, package, project setting, import, or generated PLATEAU asset change is approved by this report.

## Inputs Reviewed

- `docs/P7_ASSET_INVENTORY_REPORT.md`
- `docs/P7_LOD_AVAILABILITY_REPORT.md`
- `docs/P7_BENCHMARK_AREA_CANDIDATES.md`
- `tools/p7/scan_p7_assets.ps1` path/name inference rules
- Read-only local file metadata under `D:\PLATEAU_DATA\Chuo_2025_CityGML`

## P7-A Findings

P7-A scanned 72,384 files totaling 7.04 GB across the local PLATEAU source and the project `Assets` tree. The scan was explicitly path/name/file-metadata based; it did not parse CityGML geometry, open Unity, inspect meshes, or verify visual quality.

High-level category signals from P7-A:

| Category | Files | Size | P7-B meaning |
|---|---:|---:|---|
| Buildings | 68,148 | 2.88 GB | Broad building import is too large for a first high-detail test. |
| Bridges | 754 | 306.19 MB | Bridge source signals exist, but geometry continuity is unverified. |
| Roads/transportation | 22 | 405.75 MB | Road/transport source files exist, but walkability is unverified. |
| Water | 3 | 41.88 MB | Riverfront/water context exists at coarse folder level. |
| Underground buildings | 1 | 21.70 MB | One underground-building source file exists by path/category signal. |

LOD token signals from P7-A:

| LOD token | Files | Size | Interpretation |
|---|---:|---:|---|
| LOD3 | 76 | 6.29 MB | Path/name evidence exists, concentrated in building appearance filenames. |
| LOD2 | 180 | 2.69 MB | Path/name evidence exists, but not enough by itself to define a benchmark area. |
| LOD1 | 1 | 2.96 KB | Codelist metadata only. |
| LOD4 | 0 | 0 B | No LOD4 path/name evidence is available from current inventory outputs. |

## Safe Inferences

- A full Chuo high-detail import is not justified by current evidence. The building folder alone is 2.88 GB, and broad import would mix unknown geometry quality, texture count, material count, object count, and Editor/build performance risk.
- The strongest current LOD3 path/name cluster is `udx/bldg/53393690_bldg_6697_appearance`, with local read-only metadata showing 70 filenames containing an LOD3 token.
- P7-B can conservatively carry a small candidate set into benchmark planning:
  - `53393690` for LOD3 building/bridge/road feasibility planning.
  - `53393672` for compact bridge/road/building size-control feasibility.
  - `53394611` for underground/road/building feasibility.
- Bridge, road, water, and underground source categories exist locally, but they are only source-category signals until geometry and Unity import behavior are inspected.

## What Cannot Be Inferred

- Actual CityGML LOD contents, geometry validity, or mesh quality.
- Whether LOD3 textures correspond to usable imported LOD3 building geometry.
- Whether LOD4 exists in CityGML contents. Current path/name inventory does not show it.
- Whether roads are walkable, bridges are visually continuous, underground assets are usable, or riverfront context aligns with the selected mesh code.
- Unity import time, object count, material count, texture count, memory use, draw calls, FPS, Windows EXE performance, or build size.
- Shelter/high-rise gameplay suitability, entrance location, indoor access, or official evacuation validity.

## Why P7-B Starts Small

P7-B should start with a small area because the first high-detail import must answer practical benchmark questions before the project commits to a broader strategy:

- Can Unity import the selected source slice without destabilizing the Editor?
- How many objects, textures, and materials are generated?
- Does the selected slice produce useful visual context for P7 without changing gameplay systems?
- Is Windows x64 EXE benchmarking meaningful for the chosen slice?
- Which rollback criteria should block broader P7-C work?

## Why Full Chuo High-Detail Import Is Not Allowed Yet

Full Chuo high-detail import is blocked because P7-A found large source folders and mostly unknown LOD tokens. A broad import before a measured small-area benchmark could create oversized generated assets, slow or unstable Editor sessions, large local-only scenes, and unclear rollback cost. It would also blur the P7 boundary by encouraging scene and asset changes before area feasibility is documented.

## LOD4 Availability Position

LOD4 is not assumed available. Current P7-A inventory reports and the P7-B selector show no local `lod4` path/name hits. This is not proof that no LOD4 data exists inside CityGML contents, because geometry was not parsed, but it is enough to block any P7-B claim that LOD4 is available.

## Recommended Next Action

Carry the three candidate clusters into P7-B review:

1. Use `53393690` as the primary path/name-based LOD3 feasibility candidate.
2. Keep `53393672` as a smaller bridge/road fallback if size controllability becomes the first priority.
3. Keep `53394611` as the underground feasibility candidate.

Do not import any candidate until a human-approved benchmark/import plan defines exact files, rollback criteria, and measurement fields.
