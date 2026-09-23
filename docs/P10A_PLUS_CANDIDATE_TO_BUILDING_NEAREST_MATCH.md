# P10-A+ Candidate-To-Building Nearest-Match

P10-A+ attempted to strengthen candidate-to-building binding using available P8-E candidate coordinates, fallback building ids, and semantic binding handoff data.

Report:

- `Assets/Data/P10/p10a_plus_candidate_to_building_nearest_match_report.json`

Result:

- 110 candidate records were evaluated
- fallback building/proxy ids are reported where available
- candidate coordinate status is reported
- threshold result and confidence are reported
- nearest-match/proxy limitation is explicit for every record

Evidence level:

- candidate coordinate evidence exists
- fallback building/proxy id evidence exists
- independent Unity scene object identity proof is not available
- source geometry is insufficient for exact scene-object building binding

Required wording:

- nearest-match proxy
- not official shelter evidence
- exact PLATEAU Unity object identity not proven
- coordinate/proxy anchoring only

P10-B implication:

Runtime smoke can visually inspect whether markers look plausibly placed in the high-detail scene, but P10-B should not convert this into exact object identity unless new proof is produced.
