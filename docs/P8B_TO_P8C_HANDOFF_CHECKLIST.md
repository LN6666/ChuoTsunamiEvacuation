# P8-B to P8-C Handoff Checklist

P8-C may rely on these P8-B guard outcomes:

- Hazard layer loaded through the P8-A loader and validator.
- Hazard-layer v1 records expose `arrivalTimeSeconds`, `inundationBoundary`, `inundationDepthMeters`, `hazardIntensity`, `confidence`, `sourceMode`, and `evidenceSourceId`.
- P8-B evidence registry identifies Tokyo Metropolitan Government tsunami damage estimation as the primary Chuo evidence candidate.
- Current P8-C gate is `CONDITIONAL PASS`: official metropolitan tsunami evidence identified, but spatial extraction is manual/pending.
- The P8-B front uses those fields for selection, progression, visual warning status, and provenance reporting.
- Risk front visual exists or is ready only after P8-B visual implementation passes this guard.
- Affected infrastructure types are defined in `Assets/Data/P8/infrastructure_hazard_interaction_config.json`.
- Science fields remain separate from cinematic visual fields.
- Performance guard exists for segment count, mesh rebuild, transparency, object count, particle, shader, culling, and enable/disable risks.

P8-C must not assume:

- Official hazard values unless evidence is reviewed and source status is upgraded through an approved review step.
- P8-C must not assume official hazard values unless evidence is reviewed.
- A complete official Chuo inundation-depth raster or polygon layer exists before extraction is reviewed.
- Chuo standalone tsunami-map absence means tsunami evidence is absent.
- Maximum tsunami height references around 2.4m to 2.46m are a spatial inundation-depth grid.
- Road/building/bridge/underground interactions are implemented by P8-B.
- Collapse proxy gameplay is enabled before P8-D.
- Visual curtain height is a physical tsunami height, water level, or inundation depth.
- Gameplay success/failure rules changed in P8-B.
- Procedural fallback boundaries are evidence-based geometry.

## Required P8-C Entry Checks

- Confirm `tools/p8/run_p8b_front_v1_preflight.ps1` passes.
- Confirm `tools/p8/run_p8b_evidence_preflight.ps1` passes.
- Confirm `tools/p8/run_p8b_guard_preflight.ps1` passes.
- Confirm `tools/p8/run_p8a_preflight.ps1` still passes.
- Confirm Unity EditMode and PlayMode tests pass.
- Review `docs/P8B_REVIEW_BACKLOG.md` for open risk items.
- Keep road/building/bridge/underground interactions still not implemented until P8-C explicitly adds them.
- Confirm road/building/bridge/underground interactions are still not implemented before starting P8-C behavior work.

P8-C should connect hazard data to infrastructure categories only where P7 scene evidence supports the category. Prototype geometry is allowed under `CONDITIONAL PASS` only when labels remain explicit. Proxy or rule-based fallback behavior must remain explicit and must not be treated as official hazard modeling.
