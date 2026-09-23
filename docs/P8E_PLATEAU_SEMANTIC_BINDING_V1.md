# P8-E PLATEAU Semantic Binding V1

P8-E hardening creates a best-effort semantic binding report at `Assets/Data/P8/p8e_semantic_binding_v1.json`.

This is not a full Unity scene-object binding. It links P8 hazard categories to local PLATEAU/P7/P8 evidence where possible and documents blockers where only proxy or data-only binding exists.

| category | binding mode | evidence | confidence | P9 boundary |
|---|---|---|---|---|
| road | plateau_metadata | `Assets/P7Benchmark/Imported/53393690/udx/tran/53393690_tran_6697_op.gml` | medium | Route use remains proxy/estimated; no official route or road-geometry validation claim. |
| building | plateau_metadata | `Assets/P7Benchmark/Imported/53393690/udx/bldg/53393690_bldg_6697_op.gml`, P8 candidate audit | medium | Building status is proxy/status only; no safety or damage prediction. |
| bridge | plateau_metadata | `Assets/P7Benchmark/Imported/53393690/udx/brid/53393690_brid_6697_op.gml` | medium | Bridge restriction is proxy only. |
| underground | data_only | P8-C/P8-D configs | low | No real subway/underground geometry assumption. |
| entrance | proxy_marker | P8-C/P8-D configs and candidate audit | low | No real entrance placement; P9 must use entrance/safe-floor proxy carefully. |
| waterfront | plateau_metadata | local flood/waterfront-adjacent PLATEAU evidence | low | Not tsunami data and not full scene binding. |
| shelter_proxy | proxy_marker | P8 handoff and P8-C config | medium | Official shelters remain separate from humanitarian candidates. |
| navigation_target_proxy | proxy_marker | P8-C config | medium | P9 owns final navigation gameplay. |
| humanitarian_candidate_proxy | data_only | 110-candidate audit | high | Non-official hazard status only. |
| highrise_candidate_marker | proxy_marker | 110-candidate audit and marker readiness data | high | Persistent visibility requires explicit non-official label. |

The report deliberately sets `completeSceneSemanticBinding=false` and `sceneMutationPerformed=false`.
