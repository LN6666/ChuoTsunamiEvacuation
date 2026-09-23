# P10-A Coordinate Anchoring Final QA

P10-A keeps P9-D coordinate anchoring as a gameplay usability solution. It is coordinate-based proxy/nearest-match anchoring, not GIS-grade proof.

Validated expectations:

- 116 total anchor targets represented by the P9-D final report sample
- 110 humanitarian candidates
- 28 named candidates
- 82 ID-only candidates
- invalid coordinate rejection is covered by P9-D tests
- distance threshold rejection is covered by P9-D tests
- confidence scoring is deterministic
- fallback statuses remain explicit
- humanitarian candidates remain non-official and warning-required
- route proxy wording remains conservative

Boundary wording:

- coordinate anchors are proxy/nearest-match anchors
- candidate-to-building binding is confidence-scored gameplay proxy binding
- P5 route points are coordinate-projected gameplay proxy guidance
- routes are not official evacuation routes
- routes are not fully road-geometry validated
- exact PLATEAU Unity object identity is not claimed

Machine-readable QA status:

- `Assets/Data/P10/p10a_anchor_final_qa_status.json`
