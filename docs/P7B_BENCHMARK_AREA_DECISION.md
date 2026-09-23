# P7-B Benchmark Area Decision

Date: 2026-05-22

Decision status: Conservative candidate carry-forward. No Unity import, scene modification, asset modification, or final benchmark area approval is made by this document.

## Decision

Carry three path/name-based candidates into P7-B review and mark final Unity benchmark selection as pending.

Preferred planning candidate for the first LOD3-focused feasibility review:

`53393690` building / LOD3 appearance / bridge / road cluster.

Reason: it has the strongest current LOD3 path/name evidence and matching bridge/road source files. However, it is not approved for import yet because the evidence is still filename-based and the size is non-trivial.

## Candidate Scores

Scores are conservative planning scores from 0 to 5. They are not quality measurements and do not prove geometry usability.

| Candidate | LOD3 availability | Asset size controllability | Goal relation | Risk | Expected Unity import difficulty | Expected Windows EXE benchmark usefulness | Total | Decision |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| `53393690` LOD3 building / bridge / road cluster | 5 | 2 | 4 | 2 | 2 | 4 | 19 | Primary LOD3 planning candidate; final import pending. |
| `53393672` compact bridge / road / building cluster | 0 | 5 | 4 | 4 | 4 | 3 | 20 | Fallback if size control is prioritized over LOD3 evidence. |
| `53394611` underground / road / building cluster | 0 | 2 | 5 | 2 | 2 | 4 | 15 | Fallback for underground feasibility after LOD3 review. |

`53393672` scores highest on size control, but it does not satisfy the LOD3 evidence goal. For the first P7-B LOD-focused decision, `53393690` remains the preferred planning candidate.

## Criteria Definitions

| Criterion | Meaning |
|---|---|
| LOD3 availability | Strength of current path/name evidence for LOD3. |
| Asset size controllability | Whether the candidate can be bounded before import. |
| Goal relation | Relationship to shelter/high-rise, bridge, riverfront, road, and underground goals. |
| Risk | Lower uncertainty around source footprint and scope. |
| Expected Unity import difficulty | Expected effort and instability risk for a later approved Unity import. |
| Expected Windows EXE benchmark usefulness | Whether the candidate is representative enough to produce meaningful performance data. |

## Evidence Summary

| Candidate | Core local evidence | Main limitation |
|---|---|---|
| `53393690` | `udx/bldg/53393690_bldg_6697_op.gml`; `udx/bldg/53393690_bldg_6697_appearance`; matching `brid` and `tran` files; 70 local LOD3 filename hits in the appearance folder. | LOD3 signal is texture-path based; core candidate is about 311 MB before optional city furniture or vegetation context. |
| `53393672` | Matching `bldg`, `brid`, and `tran` files; about 26.50 MB by helper summary. | No LOD3/LOD4 path token evidence. |
| `53394611` | Matching `bldg`, `ubld`, `tran`, and `brid` files; the only explicit underground-building mesh-code file found by P7-A inventory. | No LOD3/LOD4 path token evidence; underground usability is unverified. |

## LOD4 Decision

LOD4 is not assumed available. Current reports and helper output show no local LOD4 path/name hits. P7-B must not plan a selected LOD4 benchmark unless a later approved check finds actual evidence.

## Required Next Action Before Import

Before any Unity import or scene work:

1. Human review must choose whether `53393690`, `53393672`, or `53394611` is the first import target.
2. A Markdown import/benchmark plan must list exact source files and rollback criteria.
3. The benchmark record must define Editor and Windows x64 metrics to capture.
4. Protected Unity paths remain untouched until an approved import plan explicitly allows a local-only benchmark artifact.
