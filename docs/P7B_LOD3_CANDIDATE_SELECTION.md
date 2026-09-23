# P7-B LOD3 Candidate Selection

Date: 2026-05-22

Inference policy: Path/name/file-metadata inference only. This report does not verify CityGML geometry, Unity import output, LOD quality, texture correctness, or visual fidelity.

## Basis

P7-A found 76 LOD3 path/name hits totaling 6.29 MB. The visible LOD3 hits are building appearance image filenames, not parsed CityGML geometry records. Therefore, these are candidate signals only.

## Candidate LOD3 Path Clusters

| Rank | Path cluster | Local metadata signal | Interpretation | Risk |
|---:|---|---|---|---|
| 1 | `Local PLATEAU source/udx/bldg/53393690_bldg_6697_appearance` | 70 local files with `lod3` in the filename, 5,180 appearance files total, 58.35 MB total folder size | Strongest current LOD3 path/name candidate. Use for first LOD3 feasibility planning if high-detail building appearance is the priority. | LOD3 signal is image filename based; it does not prove usable LOD3 geometry or acceptable Unity import output. |
| 2 | `Local PLATEAU source/udx/bldg/53394600_bldg_6697_appearance` | 6 local files with `lod3` in the filename, 3,658 appearance files total, 49.20 MB total folder size | Secondary weak LOD3 path/name candidate. | Much weaker LOD3 signal and no current bridge/road/underground priority attached in P7-B reports. |
| 3 | `Local PLATEAU source/udx/bldg` | P7-A folder-level summary reports LOD2 and LOD3 tokens under the building folder | Broad building category contains all known LOD3 path/name evidence. | Too broad for first import; must be narrowed to mesh-code or path cluster before Unity work. |

## Preferred LOD3 Planning Candidate

`53393690` is the preferred LOD3 planning cluster because it combines:

- the strongest current LOD3 filename-token concentration,
- a building GML file, `udx/bldg/53393690_bldg_6697_op.gml`,
- matching bridge and road source files, `udx/brid/53393690_brid_6697_op.gml` and `udx/tran/53393690_tran_6697_op.gml`,
- enough size to be meaningful for a benchmark, but still narrower than broad `udx/bldg`.

This is not a final Unity import decision. It is a path/name-based candidate selection for review.

## LOD4 Position

No LOD4 path/name candidate is selected. Current P7-A reports and the P7-B selector do not show local LOD4 path/name evidence. P7-B must not claim LOD4 availability unless a later approved geometry-aware check provides evidence.

## Follow-Up Checks Before Import

- Confirm actual CityGML LOD tags or imported mesh details with an approved geometry-aware check.
- Confirm whether `53393690` visual output is useful in Unity before expanding scope.
- Count imported objects, materials, textures, triangles, draw calls, memory, FPS, and build-size impact during the benchmark stage.
- Keep generated Unity scenes and imported PLATEAU outputs local-only unless a later plan explicitly approves tracked artifacts.
