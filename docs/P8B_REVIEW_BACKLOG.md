# P8-B Review Backlog

| ID | Severity | Status | Area | Risk | Required Follow-Up |
|---|---|---|---|---|---|
| P8B-R01 | High | Guarded | Science/visual separation | `visualHeightMeters` could be mistaken for `tsunamiHeightMeters`, `waterLevelMeters`, or inundation depth. | Keep validation/tests enforcing field separation and cinematic-only labels. |
| P8B-R02 | High | Guarded | Scope | Visual implementation could accidentally change gameplay success/failure rules. | Keep P8-B visual read-only and verify `AffectsGameplaySuccessFailure=false`. |
| P8B-R03 | High | Guarded | Stage boundary | P8-B could drift into P8-C road/building/bridge/underground interactions. | Keep interaction modes `data_only` until P8-C starts. |
| P8B-R04 | High | Guarded | Collapse | Collapse proxy could become gameplay before P8-D. | Keep collapse proxy disabled/data-only and hazard-driven collapse false. |
| P8B-R05 | Medium | Guarded | Performance | Excessive segment count, per-frame mesh rebuild, transparency overdraw, too many light curtain objects, unbounded particles, material/shader risk, or missing culling could hurt P7 high-detail performance. | Run `tools/p8/inspect_p8b_visual_performance_risk.ps1` before visual work lands. |
| P8B-R06 | Medium | Open | Evidence | Manual sample or evidence-planned data could be described as official. | Require reviewed evidence before official hazard claims. |
| P8B-R07 | Medium | Open | P8-C handoff | P8-C may assume infrastructure behavior exists. | Use `docs/P8B_TO_P8C_HANDOFF_CHECKLIST.md` before P8-C implementation. |
| P8B-R08 | Medium | Guarded | Later phases | P9 systems or P10 systems could enter P8-B. | Preflight and DeepSeek review must reject P9/P10 additions. |

## Current Scope Statement

P8-B guard hardens validation, tests, performance inspection, and review prompts. It does not implement a full fluid simulation, visual light curtain scene objects, gameplay success/failure changes, P9 systems, or P10 systems.
