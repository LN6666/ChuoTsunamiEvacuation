# P7-B Wave 2-B LOD3 Candidate Dry Run

Date: 2026-05-23

Stage: P7-B Wave 2-B

Scope: command-line metadata dry run only.

## Decision Context

Candidate `53393690` remains a planning candidate only. It is not an approved Unity import target.

Fallback candidates are `53393672` and `53394611`.

The LOD3 evidence is path/name/file-metadata evidence only. LOD3 geometry quality, visual quality, material correctness, and Unity import quality are not verified yet.

LOD4 is not available based on current evidence: the dry-run metadata scan found `0` LOD4 path/name hits under the local `udx` root.

No full Chuo import is allowed. No real asset import was performed. No candidate files were copied into Unity.

`Chuo_BaseMap.unity` is untouched.

P8 hazard work and P9 crowd/interior-shelter work are excluded.

This is a P7Benchmark feasibility step, not production integration.

Any real LOD3 import requires a separate explicit human approval gate.

## Inspection Tool

Tool:

```powershell
powershell -ExecutionPolicy Bypass -File tools/p7/inspect_p7b_lod3_candidate.ps1
```

The tool uses `Get-ChildItem` and file metadata only. It does not parse CityGML, import assets, copy files, move files, delete files, or write to `Assets/Data`, `Assets/PLATEAU`, or `D:\PLATEAU_DATA\Chuo_2025_CityGML`.

## Local Metadata Summary

The Wave 2-B inspector scans same-mesh-code path hits under `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx`. This is intentionally broader than the P7-B Wave 1 core `bldg` / `brid` / `tran` planning subset, so it reveals extra same-mesh-code categories that could make a future import heavier than the earlier scoped estimate.

| Candidate | Role | Files | Size | LOD3 path/name hits | LOD4 path/name hits | Import estimate |
|---|---|---:|---:|---:|---:|---|
| `53393690` | Preferred planning candidate | 5,843 | 605.38 MB | 70 | 0 | Bounded but not small; must be narrowed or explicitly approved before import. |
| `53393672` | Fallback planning candidate | 658 | 61.22 MB | 0 | 0 | Smallest fallback footprint, but no LOD3 path/name evidence. |
| `53394611` | Fallback planning candidate | 4,104 | 1.25 GB | 0 | 0 | Too heavy for a first wholesale import experiment; narrow before any import. |

## Candidate 53393690

`53393690` is the preferred planning candidate because P7-B Wave 1 found the strongest current LOD3 path/name signal there.

This dry-run found:

| Extension | Files | Size |
|---|---:|---:|
| `.jpg` | 5,837 | 209.49 MB |
| `.gml` | 6 | 395.88 MB |

Category hints:

| Category | Files | Size |
|---|---:|---:|
| `bldg` | 5,181 | 259.77 MB |
| `frn` | 616 | 257.02 MB |
| `brid` | 43 | 15.38 MB |
| `fld` | 1 | 16.53 MB |
| `tran` | 1 | 44.40 MB |
| `veg` | 1 | 12.27 MB |

Largest metadata samples:

| Path | Size |
|---|---:|
| `udx/bldg/53393690_bldg_6697_op.gml` | 204.13 MB |
| `udx/frn/53393690_frn_6697_op.gml` | 111.67 MB |
| `udx/tran/53393690_tran_6697_op.gml` | 44.40 MB |
| `udx/fld/pref/sumidagaw-shingashigawa-ryuiki/53393690_fld_6697_l2_op.gml` | 16.53 MB |
| `udx/veg/53393690_veg_6697_op.gml` | 12.27 MB |
| `udx/brid/53393690_brid_6697_op.gml` | 6.88 MB |

Current interpretation: `53393690` is not small enough to treat as a casual import. A future experiment should start from an explicitly approved narrowed subset and rollback plan.

## Fallback Candidate 53393672

`53393672` is a compact fallback for bridge, road, and building context.

This dry-run found:

| Extension | Files | Size |
|---|---:|---:|
| `.jpg` | 654 | 33.54 MB |
| `.gml` | 4 | 27.68 MB |

Category hints:

| Category | Files | Size |
|---|---:|---:|
| `bldg` | 587 | 33.42 MB |
| `brid` | 69 | 22.91 MB |
| `tran` | 1 | 3.71 MB |
| `fld` | 1 | 1.18 MB |

Current interpretation: `53393672` is the most size-controlled fallback, but it has no LOD3 or LOD4 path/name hits in this dry run.

## Fallback Candidate 53394611

`53394611` remains a fallback for underground-building feasibility because it includes the `ubld` category.

This dry-run found:

| Extension | Files | Size |
|---|---:|---:|
| `.jpg` | 4,095 | 860.96 MB |
| `.gml` | 9 | 415.84 MB |

Category hints:

| Category | Files | Size |
|---|---:|---:|
| `veg` | 262 | 823.25 MB |
| `bldg` | 3,125 | 221.60 MB |
| `frn` | 680 | 118.16 MB |
| `tran` | 1 | 36.15 MB |
| `brid` | 32 | 31.84 MB |
| `fld` | 3 | 24.10 MB |
| `ubld` | 1 | 21.70 MB |

Current interpretation: `53394611` is too heavy for a wholesale first import experiment and has no LOD3 or LOD4 path/name hits in this dry run.

## Dry-Run Result

Wave 2-B confirms that candidate metadata can be inspected safely from the command line. It does not confirm that any candidate is visually useful in Unity.

The next import-related action must be a separate human-approved plan, not an automatic continuation of this dry run.
