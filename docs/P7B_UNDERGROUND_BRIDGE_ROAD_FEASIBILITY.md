# P7-B Underground / Bridge / Road Feasibility

Date: 2026-05-22

Inference policy: Path/name/file-metadata inference only. No geometry, topology, visual correctness, walkability, or Unity import behavior is verified.

## Summary

P7-A found local source-category evidence for underground buildings, bridges, roads/transportation, and water/riverfront context. These signals are enough for P7-B feasibility planning, but not enough to claim usable gameplay geometry.

## Feasibility Signals

| Topic | Local path/name evidence | Size signal | Safe interpretation | Not verified |
|---|---|---:|---|---|
| Underground | `Local PLATEAU source/udx/ubld/53394611_ubld_6697_op.gml` | 21.70 MB | One underground-building source file exists and should be carried as an underground feasibility candidate. | Interior access, underground walkability, station/tunnel meaning, geometry quality, Unity import result. |
| Bridge | `Local PLATEAU source/udx/brid` and mesh-code examples such as `53393690_brid_6697_op.gml`, `53393672_brid_6697_op.gml` | P7-A category summary: 306.19 MB; direct GML folder helper summary: 21 files / 129.59 MB | Bridge source data exists and can support bridge visual-context feasibility planning. | Bridge continuity, deck/walkway usability, collision, relation to actual pedestrian routes. |
| Road / transportation | `Local PLATEAU source/udx/tran` | 22 files / 405.75 MB | Road/transportation source data exists and can support small-area road context feasibility planning. | Sidewalks, crossings, route network, road blockers, navigation validity, gameplay walkability. |
| Riverfront / waterfront | `Local PLATEAU source/udx/wtr` | 3 files / 41.88 MB | Water source data exists as riverfront/waterfront context. | Exact riverbank geometry, visual alignment with candidate mesh codes, hazard behavior, water simulation. |
| Elevated structures | Explicit bridge files and transportation category names only | Not separately measured | Elevated context may exist through bridge/transport assets. | Elevated-road classification, pedestrian deck details, structural quality, gameplay relevance. |

## Candidate Relationship

| Candidate | Underground | Bridge | Road | Riverfront/water | Use |
|---|---|---|---|---|---|
| `53393690` | Not evident | Present by matching `brid` file | Present by matching `tran` file | Coarse `533936` water context exists, not exact 8-digit mesh match | Primary LOD3 path/name planning candidate with bridge/road context. |
| `53393672` | Not evident | Present by matching `brid` file | Present by matching `tran` file | No exact local water match found by helper | Compact bridge/road fallback candidate. |
| `53394611` | Present by matching `ubld` file | Present by matching `brid` file | Present by matching `tran` file | Coarse `533946` water context exists, not exact 8-digit mesh match | Underground feasibility fallback candidate. |

## Constraints

- Do not infer streets, sidewalks, entrances, interior routes, or shelter access paths from these source names.
- Do not implement P8 hazard systems or P9 interior shelter systems from this evidence.
- Do not import a full category folder to verify a single feasibility question.
- Treat external PLATEAU source files as enumerated inputs only; this P7-B work does not modify them.

## Recommended Follow-Up

Use `tools/p7/select_p7b_candidate_area.ps1` to print the current candidate summary, then review the three candidates before any Unity import plan:

1. `53393690` for LOD3 plus bridge/road feasibility.
2. `53393672` for smaller bridge/road import-size feasibility.
3. `53394611` for underground feasibility.
